import {
  AfterViewInit, ChangeDetectionStrategy, Component, ElementRef,
  HostListener, inject, input, OnDestroy, output, signal, ViewChild
} from '@angular/core';
import { DateTimeFormatter, Duration, ZonedDateTime, ZoneId } from '@js-joda/core';
import { combineLatest, filter, Subscription } from 'rxjs';
import { getLogLineTimestampValue, LogLine } from '../../Api';
import { TimeRange, TimeRangeDuration, TimeRangeOrNull, emptyLogLineTimeRangeDuration, emptyTimeRangeOrNull, epoch01RangeDuration, equalsTimeRangeDuration, equalsTimeRangeOrNull, getEffectiveRange, getTimeRangeDurationToDebugString, getTimeRangeToDebugString, setTimeRangeDurationIfChanged, setTimeRangeFinishIfChanged, setTimeRangeIfChanged, setTimeRangeOrNullIfChanged, setTimeRangeStartIfChanged } from '@app/Utility/time-range';
import { LogTimeDataService } from '@app/Utility/log-time-data.service';
import { DepDataService } from '@app/Utility/dep-data.service';
import { LucideAngularModule } from 'lucide-angular';
import { AppIconComponent } from '@app/app-icon/app-icon.component';

const epoch0 = ZonedDateTime.of(1970, 1, 1, 0, 0, 0, 0, ZoneId.UTC);
const epoch1 = ZonedDateTime.of(1970, 1, 1, 1, 1, 1, 1, ZoneId.UTC);
const LogLineColors = ["#e74c3c", "#2ecc71", "#3498db", "#f39c12", "#9b59b6", "#1abc9c", "#e67e22", "#34495e"];
const dateTimeFormatterHHmmssSSS = DateTimeFormatter.ofPattern("HH:mm:ss.SSS");

export type TimeScale2ViewModel = {
  displayWidth: number;
  viewBox: string;
  rangeZoom: TimeRangeDuration;
  rangeFilter: TimeRangeOrNull;
  startFilterPositionX: number | null;
  finishFilterPositionX: number | null;
  finishFilterWidth: number | null;
  tickInterval: Duration;
  tickUnit: string;
  majorTickEvery: number;
  listTick: TimescaleTick[];
  listLogTick: LogTick[];
  selectedLogTick: LogTick | null;
};

export type TicksViewModel = {
  displayWidth: number;
  viewBox: string;
  tickInterval: Duration;
  tickUnit: string;
  majorTickEvery: number;
  listTick: TimescaleTick[];
};

export type TimescaleTick = {
  id: number;
  position: number;
  isMajor: boolean;
  label: string;
  ts: ZonedDateTime;
};

export type LogTick = {
  id: number;
  positionX: number;
  positionY: number;
  width: number;
  color: string;
  isHighlighted: boolean;
  isSelected: boolean;
  logLine: LogLine;
  timestamp: ZonedDateTime;
};

@Component({
  selector: 'app-time-scale-2',
  standalone: true,
  imports: [LucideAngularModule],
  templateUrl: './time-scale.component.html',
  styleUrl: './time-scale.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TimeScaleComponent implements AfterViewInit, OnDestroy {
  readonly subscription = new Subscription();
  readonly depDataService = inject(DepDataService);
  readonly depThis = this.depDataService.wrap(this);
  readonly logTimeDataService = inject(LogTimeDataService);

  readonly appIcon = new AppIconComponent();

  // Inputs
  readonly selectedLogLineId = input<number | null>(null);
  readonly highlightedLogLineId = input<number | null>(null);
  readonly visibleRange = input<TimeRangeOrNull | null>(null);

  readonly selectedLogLineIdProp = this.depThis.createProperty<number>({
    name: 'selectedLogLineId',
    initialValue: 0,
    compare: (a, b) => (a ?? 0) === (b ?? 0),
    input: {
      input: this.selectedLogLineId,
      transform: (value) => (value ?? 0)
    },
  });

  readonly highlightedLogLineIdProp = this.depThis.createProperty<number>({
    name: 'highlightedLogLineId',
    initialValue: 0,
    compare: (a, b) => (a === b),
    input: {
      input: this.highlightedLogLineId,
      transform: (value) => (value ?? 0)
    },
  });

  readonly visibleRangeProp = this.depThis.createProperty<TimeRangeOrNull>({
    name: 'visibleRange',
    initialValue: emptyTimeRangeOrNull,
    compare: (a, b) => equalsTimeRangeOrNull(a, b),
    input: {
      input: this.visibleRange,
      transform: (value) => (value ?? emptyTimeRangeOrNull)
    },
  });

  readonly dataComplete = this.depThis.createProperty({
    name: 'dataComplete',
    initialValue: emptyLogLineTimeRangeDuration,
  }).withSourceIdentity(
    this.logTimeDataService.dataComplete.dependencyPublic());

  readonly dataZoom = this.depThis.createProperty({
    name: 'dataZoom',
    initialValue: emptyLogLineTimeRangeDuration,
  }).withSourceIdentity(
    this.logTimeDataService.dataZoom.dependencyPublic());

  readonly dataTimeFiltered = this.depThis.createProperty({
    name: 'dataTimeFiltered',
    initialValue: emptyLogLineTimeRangeDuration,
  }).withSourceIdentity(
    this.logTimeDataService.dataTimeFiltered.dependencyPublic());

  readonly dataLogLineFilteredCondition = this.depThis.createProperty({
    name: 'dataLogLineFilteredCondition',
    initialValue: emptyLogLineTimeRangeDuration,
  }).withSourceIdentity(
    this.logTimeDataService.dataFilteredCondition.dependencyPublic());

  readonly listLogLineVisual = this.depThis.createProperty<readonly LogLine[]>({
    name: 'listLogLineVisual',
    initialValue: [],
  }).withSource({
    sourceDependency: {
      dataLogLine: this.logTimeDataService.dataComplete.dependencyUi()
    },
    sourceTransform:
      (d) => {
        const { listLogLine } = d.dataLogLine;
        const listLogLineLength = listLogLine.length;
        const maxLength = 512;
        if (listLogLineLength <= (maxLength + 1)) {
          return listLogLine;
        } else {
          let nextListLogLineVisual: LogLine[];
          // sample down to 500 items and keep the selected and highlighted items
          nextListLogLineVisual = Array(maxLength + 1);
          for (let pos = 0; pos <= maxLength; pos++) {
            const index = Math.floor(pos * listLogLineLength / 500);
            nextListLogLineVisual[pos] = listLogLine[index];
          }
          return nextListLogLineVisual;
        }
      }
  });


  readonly rangeComplete = this.depThis.createProperty({
    name: 'rangeComplete',
    initialValue: epoch01RangeDuration,
    compare: equalsTimeRangeDuration,
  }).withSource({
    sourceDependency: {
      rangeComplete: this.logTimeDataService.rangeComplete.dependencyInner()
    },
    sourceTransform: (d) => d.rangeComplete
  });


  readonly rangeZoom = this.depThis.createProperty({
    name: 'rangeZoom',
    initialValue: epoch01RangeDuration,
    compare: equalsTimeRangeDuration,
  }).withSource({
    sourceDependency: {
      rangeZoom: this.logTimeDataService.rangeZoom.dependencyInner()
    },
    sourceTransform:
      (d) => d.rangeZoom,
  });


  readonly rangeFilter = this.depThis.createProperty({
    name: 'rangeFilter',
    initialValue: epoch01RangeDuration,
    compare: equalsTimeRangeDuration,
  }).withSource({
    sourceDependency: {
      rangeFilter: this.logTimeDataService.rangeFilter.dependencyInner()
    },
    sourceTransform:
      (d) => d.rangeFilter,
  });
  readonly rangeFilter$ = this.rangeFilter.asObserable();

  // Outputs
  readonly logLineClick = output<LogLine>();
  readonly logLineHover = output<LogLine | null>();

  // Internal state
  readonly displayWidth = this.depThis.createProperty<number>({
    name: 'displayWidth',
    initialValue: 0,
  });

  readonly stateTicks = this.depThis.createProperty<TicksViewModel>({
    name: 'stateTicks',
    initialValue: this.createInitialState(),
  }).withSource({
    sourceDependency: {
      displayWidth: this.displayWidth.dependencyInner(),
      rangeZoom: this.rangeZoom.dependencyInner()
    },
    sourceTransform: ({ displayWidth, rangeZoom }) => {
      const { tickInterval, tickUnit, majorTickEvery } = this.calculateTicksBaseInfo(displayWidth, rangeZoom);
      const listTick = this.generateTicks(rangeZoom, displayWidth, tickInterval, tickUnit, majorTickEvery);
      const stateTicks: TicksViewModel = {
        displayWidth,
        viewBox: `0 0 ${displayWidth} 100`,
        tickInterval,
        tickUnit,
        majorTickEvery,
        listTick: listTick
      };
      return stateTicks;
    },
  });

  readonly $stateTicks = this.stateTicks.asSignal();

  readonly plotCount = this.depThis.createProperty({
    name: 'plotCount',
    initialValue: "15,80",
    compare: (a, b) => a === b,
  }).withSource({
    sourceDependency: {
      dataZoom: this.dataZoom.dependencyUi(),
      displayWidth: this.displayWidth.dependencyUi(),
      stateTicks: this.stateTicks.dependencyUi()
    },
    sourceTransform: ({ dataZoom: { listLogLine, range }, displayWidth, stateTicks: { listTick } }) => {
      const partsCount = listTick.length;
      if (0 === partsCount) {
        return "15,80";
      } else {
        const partsCount_1 = partsCount - 1;
        // count the log lines in the parts
        const listCountPart: number[] = Array(partsCount).fill(0);
        let idxPart = 1;
        for (let idx = 0; idx < listLogLine.length; idx++) {
          const ts = listLogLine[idx].ts;
          if (ts == null) { continue; }
          try {
            while (
              (idxPart < partsCount)
              && (listTick[idxPart].ts != null)
              && (listTick[idxPart].ts.compareTo(ts) <= 0)
            ) {
              if (idxPart < (partsCount_1)) {
                idxPart++;
              } else {
                break;
              }
            }
            if (0 < idxPart && idxPart < partsCount) {
              listCountPart[idxPart - 1]++;
            }
          } catch (error) {
            console.error(error);
          }
        }

        // the maximum number of log lines in a part
        const maxCountPart = Math.max(...listCountPart);

        const visualWidth = displayWidth - 30;
        const width = visualWidth / partsCount;

        const points: string[] = Array(partsCount + 2);

        for (let idx = 0; idx < partsCount; idx++) {
          //const x = 15 + visualWidth * (idx / partsCount);
          //const x= width * idx + width / 2 + 15;
          const x = this.calcPositionX(listTick[idx].ts, range, displayWidth) + width / 2;
          const rel = (maxCountPart === 0) ? 0 : (80 * (listCountPart[idx] / maxCountPart));
          const y = 80 - rel;
          if (Number.isNaN(y) || Number.isNaN(x)) {
            debugger;
          } else {
            points[idx + 1] = `${x},${y}`;
            if (idx === 0) {
              points[0] = `15,${y}`;
            } else if (idx === (partsCount_1)) {
              points[partsCount + 1] = `${displayWidth - 15},${y}`;
            }
          }
        }
        const pointsString = points.join(' ');
        return pointsString;
      }
    }
  });
  readonly $plotCount = this.plotCount.asSignal();

  readonly selectedLogLine = this.depThis.createProperty<LogTick | null>({
    name: 'selectedLogLine',
    initialValue: null as (LogTick | null),
  }).withSource({
    sourceDependency: {
      selectedLogLineId: this.selectedLogLineIdProp.dependencyInner(),
      highlightedLogLineId: this.highlightedLogLineIdProp.dependencyInner(),
      listLogLineVisual: this.listLogLineVisual.dependencyInner(),
      displayWidth: this.displayWidth.dependencyInner(),
      rangeZoom: this.rangeZoom.dependencyInner(),
    },
    sourceTransform: ({ selectedLogLineId, highlightedLogLineId, listLogLineVisual, displayWidth, rangeZoom }) => {
      if (selectedLogLineId == null) { return null; }

      const selectedLogLine = listLogLineVisual.find(l => l != null && l.id === selectedLogLineId)
        ?? null;

      if (selectedLogLine != null && selectedLogLine.ts != null) {
        const positionX = this.calcPositionX(selectedLogLine.ts, rangeZoom, displayWidth);
        const tick: LogTick = {
          id: selectedLogLineId!,
          positionX: positionX - 1,
          positionY: 5 + (selectedLogLineId! % 25) * 2,
          width: 4,
          color: 'darkblue',
          isHighlighted: selectedLogLineId == highlightedLogLineId,
          isSelected: true,
          logLine: selectedLogLine,
          timestamp: selectedLogLine.ts
        };
        return tick
      } else {
        return null;
      }
    },
  });
  readonly $selectedLogLine = this.selectedLogLine.asSignal();

  readonly highlightedLogLine = this.depThis.createProperty<LogTick | null>({
    name: 'highlightedLogLine',
    initialValue: null as (LogTick | null),
  }).withSource({
    sourceDependency: {
      highlightedLogLineId: this.highlightedLogLineIdProp.dependencyInner(),
      listLogLineVisual: this.listLogLineVisual.dependencyInner(),
      displayWidth: this.displayWidth.dependencyInner(),
      rangeZoom: this.rangeZoom.dependencyInner(),
    },
    sourceTransform: ({ highlightedLogLineId, listLogLineVisual, displayWidth, rangeZoom }) => {
      if (highlightedLogLineId == null) { return null; }
      const highlightedLogLine = listLogLineVisual.find(l => l != null && l.id === highlightedLogLineId)
        ?? null;

      if (highlightedLogLine != null && highlightedLogLine.ts != null) {
        const positionX = this.calcPositionX(highlightedLogLine.ts, rangeZoom, displayWidth);
        const tick: LogTick = {
          id: highlightedLogLineId!,
          positionX: positionX - 1,
          positionY: 5 + (highlightedLogLineId! % 25) * 2,
          width: 4,
          color: 'yellow',
          isHighlighted: true,
          isSelected: false,
          logLine: highlightedLogLine,
          timestamp: highlightedLogLine.ts
        };
        return tick;
      } else {
        return null;
      }
    }
  });
  readonly $highlightedLogLine = this.highlightedLogLine.asSignal();

  readonly state = this.depThis.createProperty({
    name: 'state',
    initialValue: this.createInitialState(),
    /*
    report: (property, message, value) => {
      console.log(property.name, message, {
        displayWidth: value?.displayWidth,
        viewBox: value?.viewBox,
        rangeZoom: getTimeRangeDurationToDebugString(value?.rangeZoom),
        rangeFilter: getTimeRangeToDebugString(value?.rangeFilter),
        startFilterPositionX: value?.startFilterPositionX,
        finishFilterPositionX: value?.finishFilterPositionX,
        finishFilterWidth: value?.finishFilterWidth,
        tickInterval: value?.tickInterval?.toString(),
        tickUnit: value?.tickUnit,
        majorTickEvery: value?.majorTickEvery,
        listTick: value?.listTick?.length,
        listLogTick: value?.listLogTick?.length,
        selectedLogTick: value?.selectedLogTick?.id
      });
    },
    */
  }).withSource({
    sourceDependency: {
      displayWidth: this.displayWidth.dependencyInner(),
      rangeZoom: this.rangeZoom.dependencyInner(),
      rangeFilter: this.rangeFilter.dependencyInner(),
      listLogLineVisual: this.listLogLineVisual.dependencyInner(),
      selectedLogLineId: this.selectedLogLineIdProp.dependencyInner(),
      highlightedLogLineId: this.highlightedLogLineIdProp.dependencyInner(),
      stateTicks: this.stateTicks.dependencyInner()
    },
    sourceTransform: ({ displayWidth, rangeZoom, rangeFilter, listLogLineVisual, selectedLogLineId, highlightedLogLineId, stateTicks }) => {
      // Calculate filter positions
      let startFilterPositionX: number | null = null;
      let finishFilterPositionX: number | null = null;
      let finishFilterWidth: number | null = null;

      if (rangeFilter.start && rangeZoom.start.compareTo(rangeFilter.start) < 0) {
        startFilterPositionX = this.calcPositionX(rangeFilter.start, rangeZoom, displayWidth);
      } else {
        startFilterPositionX = 15;
      }
      if (rangeFilter.finish && rangeFilter.finish.compareTo(rangeZoom.finish) < 0) {
        finishFilterPositionX = this.calcPositionX(rangeFilter.finish, rangeZoom, displayWidth);
        if (finishFilterPositionX < 0) {
          console.error("finishFilterPositionX < 0", finishFilterPositionX);
          finishFilterPositionX = displayWidth - 15;
        }
        finishFilterWidth = displayWidth - 15 - finishFilterPositionX;
      } else {
        finishFilterPositionX = displayWidth - 15;
        finishFilterWidth = 15;
      }

      const listLogTick = this.generateLogTicks(listLogLineVisual, rangeZoom, displayWidth, selectedLogLineId, highlightedLogLineId);

      const selectedLogTick = listLogTick.find(t => t.id === selectedLogLineId) ?? null;

      const state: TimeScale2ViewModel = {
        displayWidth,
        viewBox: `0 0 ${displayWidth} 100`,
        rangeZoom,
        rangeFilter,
        startFilterPositionX,
        finishFilterPositionX,
        finishFilterWidth,
        tickInterval: stateTicks.tickInterval,
        tickUnit: stateTicks.tickUnit,
        majorTickEvery: stateTicks.majorTickEvery,
        listTick: stateTicks.listTick,
        listLogTick: listLogTick,
        selectedLogTick: selectedLogTick
      };

      return state;
    }
  });
  readonly $state = this.state.asSignal();

  @ViewChild('containerElement', { static: true }) containerElement!: ElementRef<HTMLDivElement>;
  // @ViewChild('svgElement', { static: false }) svgElement!: ElementRef<SVGSVGElement>;

  private dragState: { mode: 'start' | 'finish' | ''; startClientX: number; startPositionX: number; } = {
    mode: '', startClientX: 0, startPositionX: 0
  };

  constructor() {
    this.depThis.executePropertyInitializer();
  }

  private createInitialState(): TimeScale2ViewModel {
    return {
      displayWidth: 0,
      viewBox: '',
      rangeZoom: { start: epoch0, finish: epoch1, duration: Duration.between(epoch0, epoch1) },
      rangeFilter: { start: null, finish: null },
      startFilterPositionX: null,
      finishFilterPositionX: null,
      finishFilterWidth: null,
      tickInterval: Duration.ofMillis(100),
      tickUnit: 'ms',
      majorTickEvery: 10,
      listTick: [],
      listLogTick: [],
      selectedLogTick: null
    };
  }

  ngAfterViewInit(): void {
    this.updateViewBox();
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  @HostListener('window:resize')
  onResize(): void {
    this.updateViewBox();
  }

  private updateViewBox(): void {
    const width = this.containerElement.nativeElement.clientWidth;
    this.displayWidth.setValue(width);
  }

  private calcPositionX(value: ZonedDateTime, rangeZoom: TimeRangeDuration, displayWidth: number): number {
    const durationMillis = rangeZoom.duration.toMillis();
    if (durationMillis <= 0) return 15;
    const currentMillis = Duration.between(rangeZoom.start, value).toMillis();
    const width = displayWidth - 30;
    return 15 + ((currentMillis / durationMillis) * width);
  }

  private calculateTicksBaseInfo(displayWidth: number, rangeZoom: TimeRangeDuration) {
    const durationMillis = rangeZoom.duration.toMillis();
    const pixelsPerTick = 10;
    const desiredTickCount = (displayWidth - 30) / pixelsPerTick;
    const millisPerTick = durationMillis / desiredTickCount;

    let tickInterval: Duration;
    let tickUnit: string;
    let majorTickEvery: number;

    if (millisPerTick < 1) {
      tickInterval = Duration.ofMillis(1); tickUnit = 'ms'; majorTickEvery = 100;
    } else if (millisPerTick < 10) {
      tickInterval = Duration.ofMillis(10); tickUnit = 'ms'; majorTickEvery = 10;
    } else if (millisPerTick < 100) {
      tickInterval = Duration.ofMillis(100); tickUnit = 'ms'; majorTickEvery = 10;
    } else if (millisPerTick < 1000) {
      tickInterval = Duration.ofSeconds(1); tickUnit = 's'; majorTickEvery = 10;
    } else if (millisPerTick < 10000) {
      tickInterval = Duration.ofSeconds(10); tickUnit = 's'; majorTickEvery = 6;
    } else if (millisPerTick < 60000) {
      tickInterval = Duration.ofMinutes(1); tickUnit = 'min'; majorTickEvery = 10;
    } else {
      tickInterval = Duration.ofMinutes(10); tickUnit = 'min'; majorTickEvery = 6;
    }
    return { tickInterval, tickUnit, majorTickEvery };
  }

  private generateTicks(rangeZoom: TimeRangeDuration, displayWidth: number, tickInterval: Duration, tickUnit: string, majorTickEvery: number): TimescaleTick[] {
    const ticks: TimescaleTick[] = [];
    let currentTime = rangeZoom.start;
    let tickCount = 0;

    while (currentTime.isBefore(rangeZoom.finish) || currentTime.equals(rangeZoom.finish)) {
      const position = this.calcPositionX(currentTime, rangeZoom, displayWidth);
      const isMajor = tickCount !== 0 && (tickCount % majorTickEvery === 0 || tickCount % 5 === 0);

      let label = '';
      if (isMajor) {
        if (tickUnit === 'ms') label = `${currentTime.toInstant().toEpochMilli() % 1000}ms`;
        else if (tickUnit === 's') label = `${currentTime.second()}s`;
        else if (tickUnit === 'min') label = `${currentTime.minute()}:${currentTime.second().toString().padStart(2, '0')}`;
      }

      ticks.push({ id: tickCount, position, isMajor, label, ts: currentTime });
      currentTime = currentTime.plus(tickInterval);
      tickCount++;
      if (tickCount > displayWidth) break;
    }
    return ticks;
  }

  private generateLogTicks(
    listLogLine: readonly LogLine[],
    rangeZoom: TimeRangeDuration,
    displayWidth: number,
    selectedId: number | null,
    highlightedId: number | null): LogTick[] {
    const ticks: LogTick[] = [];

    for (let i = 0; i < listLogLine.length; i++) {
      const logLine = listLogLine[i];
      const ts = getLogLineTimestampValue(logLine);
      if (!ts) continue;

      const positionX = this.calcPositionX(ts, rangeZoom, displayWidth);
      ticks.push({
        id: logLine.id,
        positionX: positionX - 1,
        positionY: 5 + (logLine.id % 25) * 2,
        width: 3,
        color: LogLineColors[logLine.id % LogLineColors.length],
        isHighlighted: logLine.id === highlightedId,
        isSelected: logLine.id === selectedId,
        logLine: logLine,
        timestamp: ts
      });
    }
    return ticks;
  }

  // Template helper methods
  toPositionX(value: ZonedDateTime | null): number {
    const state = this.state.getValue();
    if (!value || state.displayWidth === 0) return 15;
    return this.calcPositionX(value, state.rangeZoom, state.displayWidth);
  }

  toTimeString(value: ZonedDateTime | null): string {
    if (!value) return '';
    try { return value.format(dateTimeFormatterHHmmssSSS); }
    catch { return ''; }
  }

  getVisibleRangeRect(state: TimeScale2ViewModel): { x: number; width: number } | null {

    const visible = this.visibleRange();
    if (!visible || state.listLogTick.length === 0) return null;

    const x1 = this.calcPositionX(visible.start ?? state.rangeZoom.start, state.rangeZoom, state.displayWidth);
    const x2 = this.calcPositionX(visible.finish ?? state.rangeZoom.finish, state.rangeZoom, state.displayWidth);

    return { x: x1, width: x2 - x1 };
  }

  // Event handlers
  onLogTickClick(logTick: LogTick): void {
    this.logLineClick.emit(logTick.logLine);
  }

  onLogTickHover(logTick: LogTick | null): void {
    this.logLineHover.emit(logTick?.logLine ?? null);
  }

  private toTimeFromPositionX(positionX: number): ZonedDateTime | null {
    const state = this.state.getValue();
    if (state.displayWidth === 0) return null;

    const width = state.displayWidth - 30;
    const relativeX = positionX - 15;
    const ratio = Math.max(0, Math.min(1, relativeX / width));
    const offsetMillis = state.rangeZoom.duration.toMillis() * ratio;
    return state.rangeZoom.start.plusNanos(offsetMillis * 1000000);
  }

  onMouseDown(event: MouseEvent, mode: 'start' | 'finish'): void {
    const clientX = event.clientX - this.containerElement.nativeElement.getBoundingClientRect().left;
    this.dragState = { mode, startClientX: clientX, startPositionX: clientX };
    event.preventDefault();
    event.stopPropagation();
  }

  onMouseMove(event: MouseEvent, _mode: 'start' | 'finish' | 'inner'): void {
    if (this.dragState.mode === '') return;

    const clientX = event.clientX - this.containerElement.nativeElement.getBoundingClientRect().left;
    const state = this.state.getValue();
    //debugger;
    if (this.dragState.mode === 'start') {
      this.state.setValue({ ...state, startFilterPositionX: Math.max(15, clientX) });
    } else if (this.dragState.mode === 'finish') {
      const newFinish = Math.min(state.displayWidth - 15, clientX);
      this.state.setValue({ ...state, finishFilterPositionX: newFinish, finishFilterWidth: state.displayWidth - 15 - newFinish });
    }
    event.preventDefault();
  }

  onMouseUp(event: MouseEvent, _mode: 'start' | 'finish' | 'inner'): void {
    if (this.dragState.mode === '') return;

    const clientX = event.clientX - this.containerElement.nativeElement.getBoundingClientRect().left;
    const ts = this.toTimeFromPositionX(clientX);
    if (ts) {
      const currentFilter = this.rangeFilter.getValue();
      if (this.dragState.mode === 'start') {
        const nextRange = { start: ts, finish: currentFilter.finish };
        this.logTimeDataService.setRangeFilter(nextRange);
      } else if (this.dragState.mode === 'finish') {
        const nextRange = { start: currentFilter.start, finish: ts };
        this.logTimeDataService.setRangeFilter(nextRange);
      }
    }
    this.dragState = { mode: '', startClientX: 0, startPositionX: 0 };
    event.preventDefault();
  }

  onMouseDblClick(mode: 'start' | 'finish'): void {
    const rangeZoom = this.dataZoom.getValue().range;
    const currentFilter = this.rangeFilter.getValue();
    let nextRange: TimeRangeOrNull;
    if (mode === 'start') {
      nextRange = { start: rangeZoom.start, finish: currentFilter.finish };
    } else {
      nextRange = { start: currentFilter.start, finish: rangeZoom.finish };
    }
    this.logTimeDataService.setRangeFilter(nextRange);
  }

  // Public method to trigger refresh
  refresh(): void {
    const clientWidth = this.containerElement.nativeElement.clientWidth;
    this.displayWidth.setValue(clientWidth);
  }

  onZoomIn() {
    const rangeFilter = this.rangeFilter.getValue();
    let nextRange: TimeRangeOrNull;
    nextRange = { start: rangeFilter.start, finish: rangeFilter.finish };
    this.logTimeDataService.setRangeZoom(nextRange);
  }
  onZoomOut(mode: 'increment' | 'complete') {
    if (mode === 'increment') {
      const rangeFilter = this.rangeFilter.getValue();
      const rangeZoom = this.rangeZoom.getValue();
      let nextRange = { start: rangeZoom.start.minus(rangeZoom.duration), finish: rangeZoom.finish.plus(rangeZoom.duration) };
      this.depDataService.scoped(() => {
        this.logTimeDataService.setRangeZoom(nextRange);
        this.logTimeDataService.setRangeFilter(rangeFilter);
      }, { name: 'onZoomOut' });
      return;
    }
    if (mode === 'complete') {
      const rangeFilter = this.rangeFilter.getValue();
      const rangeComplete = this.logTimeDataService.dataComplete.getValue().range;
      let nextRange = rangeComplete;
      this.logTimeDataService.setRangeZoom(nextRange);
      this.logTimeDataService.setRangeFilter(rangeFilter);
      return;
    }
  }

}

