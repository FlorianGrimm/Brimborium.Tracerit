import { AsyncPipe } from '@angular/common';
import {
  AfterViewInit, ChangeDetectionStrategy, Component, ElementRef,
  HostListener, inject, input, OnDestroy, output, signal, ViewChild
} from '@angular/core';
import { DateTimeFormatter, Duration, ZonedDateTime, ZoneId } from '@js-joda/core';
import { BehaviorSubject, combineLatest, delay, filter, Subscription } from 'rxjs';
import { toObservable } from '@angular/core/rxjs-interop';
import { getLogLineTimestampValue, LogLine } from '../../Api';
import { BehaviorRingSubject } from '../../Utility/BehaviorRingSubject';
import { MasterRingService } from '../../Utility/master-ring.service';
import { TimeRange, TimeRangeDuration, TimeRangeOrNull, getEffectiveRange, setTimeRangeDurationIfChanged, setTimeRangeFinishIfChanged, setTimeRangeIfChanged, setTimeRangeOrNullIfChanged, setTimeRangeStartIfChanged } from '../../Utility/time-range';
import { LogTimeDataService } from '../../Utility/log-time-data.service';
import { tick } from '@angular/core/testing';

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
  imports: [AsyncPipe],
  templateUrl: './time-scale-2.component.html',
  styleUrl: './time-scale-2.component.scss'
})
export class TimeScale2Component implements AfterViewInit, OnDestroy {
  private subscription = new Subscription();
  private readonly ring$ = inject(MasterRingService).dependendRing('TimeScale2-ring$', this.subscription);
  private readonly logTimeDataService = inject(LogTimeDataService);

  // Inputs
  readonly selectedLogLineId = input<number | null>(null);
  readonly highlightedLogLineId = input<number | null>(null);
  readonly visibleRange = input<TimeRangeOrNull | null>(null);

  readonly listLogLineAll$ = new BehaviorRingSubject<LogLine[]>([], 0, 'TimeScale2Component_listLogLineAl', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });
  readonly listLogLineTimeZoomed$ = new BehaviorRingSubject<LogLine[]>([], 0, 'TimeScale2Component_listLogLineTimeFiltered', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });
  readonly listLogLineTimeFiltered$ = new BehaviorRingSubject<LogLine[]>([], 0, 'TimeScale2Component_listLogLineTimeFiltered', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });
  readonly listLogLineFilteredCondition$ = new BehaviorRingSubject<LogLine[]>([], 0, 'TimeScale2Component_listLogLineFilteredCondition', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });
  readonly listLogLineVisual$ = new BehaviorRingSubject<LogLine[]>([], 0, 'TimeScale2Component_listLogLineVisual', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });

  readonly rangeComplete$ = new BehaviorRingSubject<TimeRangeDuration>(
    Object.freeze({
      start: epoch0,
      finish: epoch1,
      duration: Duration.between(epoch0, epoch1)
    }),
    0, 'TimeScale2Component_rangeComplete$', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, { start: value?.start?.toString(), finish: value?.finish?.toString() }); });

  readonly rangeZoom$ = new BehaviorRingSubject<TimeRangeDuration>(Object.freeze({
    start: epoch0,
    finish: epoch1,
    duration: Duration.between(epoch0, epoch1)
  }),
    0, 'TimeScale2Component_rangeZoom',
    this.subscription, this.ring$, undefined,
    (name, message, value) => {
      console.log(name, message, { start: value?.start?.toString(), finish: value?.finish?.toString(), duration: value?.duration?.toString() });
    }
  );

  readonly rangeFilter$ = new BehaviorRingSubject<TimeRangeDuration>(Object.freeze({
    start: epoch0,
    finish: epoch1,
    duration: Duration.between(epoch0, epoch1)
  }),
    0, 'TimeScale2Component_rangeFilter', this.subscription, this.ring$, undefined,
    (name, message, value) => {
      console.log(name, message, { start: value?.start?.toString(), finish: value?.finish?.toString() });
    }
  );

  readonly selectedLogLineId$ = toObservable(this.selectedLogLineId);
  readonly highlightedLogLineId$ = toObservable(this.highlightedLogLineId);
  readonly visibleRange$ = toObservable(this.visibleRange);


  // Outputs
  readonly logLineClick = output<LogLine>();
  readonly logLineHover = output<LogLine | null>();

  // Internal state
  readonly stateTicks$ = new BehaviorRingSubject<TicksViewModel>(
    this.createInitialState(),
    0, 'TimeScale2_stateTicks', this.subscription, this.ring$, undefined
  );

  readonly plotCount = signal("15,80");
  readonly selectedLogLine = signal<LogTick | null>(null);
  readonly highlightedLogLine = signal<LogTick | null>(null);

  readonly state$ = new BehaviorRingSubject<TimeScale2ViewModel>(
    this.createInitialState(),
    0, 'TimeScale2_state', this.subscription, this.ring$, undefined
  );

  readonly displayWidth$ = new BehaviorRingSubject<number>(0,
    0, 'TimeScale2_displayWidth', this.subscription, this.ring$, undefined
  );

  @ViewChild('containerElement', { static: true }) containerElement!: ElementRef<HTMLDivElement>;
  @ViewChild('svgElement', { static: false }) svgElement!: ElementRef<SVGSVGElement>;

  private dragState: { mode: 'start' | 'finish' | ''; startClientX: number; startPositionX: number; } = {
    mode: '', startClientX: 0, startPositionX: 0
  };

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
    window.requestAnimationFrame(() => {
      this.setupStateSubscription();
    });
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
    this.displayWidth$.next(width);
  }

  private setupStateSubscription(): void {
    this.subscription.add(this.logTimeDataService.listLogLineAll$.subscribe({ next: (value) => { this.listLogLineAll$.next(value ?? []); } }));
    this.subscription.add(this.logTimeDataService.listLogLineTimeZoomed$.subscribe({ next: (value) => { this.listLogLineTimeZoomed$.next(value ?? []); } }));
    this.subscription.add(this.logTimeDataService.listLogLineFilteredTime$.subscribe({ next: (value) => { this.listLogLineTimeFiltered$.next(value ?? []); } }));
    this.subscription.add(this.logTimeDataService.listLogLineFilteredCondition$.subscribe({ next: (value) => { this.listLogLineFilteredCondition$.next(value ?? []); } }));

    this.subscription.add(
      this.logTimeDataService.rangeComplete$.subscribe({
        next: (value) => {
          setTimeRangeDurationIfChanged(this.rangeComplete$, value);
        }
      }));
    this.subscription.add(
      this.logTimeDataService.rangeZoom$.subscribe({
        next: (value) => {
          setTimeRangeDurationIfChanged(this.rangeZoom$, value);
        }
      }));
    this.subscription.add(
      this.logTimeDataService.rangeFilter$.subscribe({
        next: (value) => {
          setTimeRangeDurationIfChanged(this.rangeFilter$, value);
        }
      }));
    this.subscription.add(
      this.listLogLineFilteredCondition$.subscribe({
        next: (listLogLine) => {
          const listLogLineLength = listLogLine.length;
          const maxLength = 512;
          let nextListLogLineVisual: LogLine[];
          if (listLogLineLength <= (maxLength + 1)) {
            nextListLogLineVisual = listLogLine;
          } else {
            // sample down to 500 items and keep the selected and highlighted items
            nextListLogLineVisual = Array(maxLength + 1);
            for (let pos = 0; pos <= maxLength; pos++) {
              const index = Math.floor(pos * listLogLineLength / 500);
              nextListLogLineVisual[pos] = listLogLine[index];
            }
          }
          this.listLogLineVisual$.next(nextListLogLineVisual);
        }
      })
    );

    this.subscription.add(
      combineLatest({
        listLogLine: this.listLogLineFilteredCondition$,
        selectedLogLineId: this.selectedLogLineId$,
        highlightedLogLineId: this.highlightedLogLineId$,
        displayWidth: this.displayWidth$,
        rangeZoom: this.rangeZoom$
      }).subscribe({
        next: ({ listLogLine, selectedLogLineId, highlightedLogLineId, displayWidth, rangeZoom }) => {
          const selectedLogLine = listLogLine.find(l => l.id === selectedLogLineId);
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
            this.selectedLogLine.set(tick);
          } else {
            this.selectedLogLine.set(null);
          }

          const highlightedLogLine = listLogLine.find(l => l.id === highlightedLogLineId);
          if (highlightedLogLine != null && highlightedLogLine.ts != null) {
            const positionX = this.calcPositionX(highlightedLogLine.ts, rangeZoom, displayWidth);
            const tick: LogTick = {
              id: selectedLogLineId!,
              positionX: positionX - 1,
              positionY: 5 + (highlightedLogLineId! % 25) * 2,
              width: 5,
              color: 'yellow',
              isHighlighted: selectedLogLineId == highlightedLogLineId,
              isSelected: true,
              logLine: highlightedLogLine,
              timestamp: highlightedLogLine.ts
            };
            this.highlightedLogLine.set(tick);
          } else {
            this.highlightedLogLine.set(null);
          }
        }
      }
      ));

    this.subscription.add(
      combineLatest({
        displayWidth: this.displayWidth$.pipe(filter(w => w > 0)),
        rangeZoom: this.rangeZoom$,
      }).subscribe({
        next: ({ displayWidth, rangeZoom }) => {
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

          this.stateTicks$.next(stateTicks);
        }
      })
    );

    this.subscription.add(
      combineLatest({
        listLogLine: this.listLogLineTimeZoomed$,
        displayWidth: this.displayWidth$,
        stateTicks: this.stateTicks$
      }).subscribe({
        next: ({ listLogLine, displayWidth, stateTicks }) => {

          const listTick = stateTicks.listTick;
          const partsCount = listTick.length;
          if (0 === partsCount) {
            this.plotCount.set("15,80");
          } else {
            const partsCount_1 = partsCount - 1;
            // count the log lines in the parts
            const listCountPart: number[] = Array(partsCount).fill(0);
            let idxPart = 0;
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
                if (0 <= idxPart && idxPart < partsCount) {
                  listCountPart[idxPart]++;
                }
              } catch (error) {
                console.error(error);
              }
            }

            // the maximum number of log lines in a part
            const maxCountPart = Math.max(...listCountPart);

            const visualWidth = displayWidth - 30;
            const width = visualWidth / partsCount;

            const points: string[] = Array(partsCount + 1);
            for (let idx = 0; idx < partsCount; idx++) {
              const x = 15 + visualWidth * (idx / partsCount);
              const rel = (maxCountPart === 0) ? 0 : (80 * (listCountPart[idx] / maxCountPart));
              const y = 80 - rel;
              if (Number.isNaN(y) || Number.isNaN(x)) {
                debugger;
              } else {
                points[idx] = `${x},${y}`;
              }
            }
            const pointsString = points.join(' ');
            this.plotCount.set(pointsString);
          }
        }
      })
    );

    this.subscription.add(
      combineLatest({
        displayWidth: this.displayWidth$.pipe(filter(w => w > 0)),
        rangeZoom: this.rangeZoom$,
        rangeFilter: this.rangeFilter$,
        listLogLineVisual: this.listLogLineVisual$,
        selectedLogLineId: this.selectedLogLineId$,
        highlightedLogLineId: this.highlightedLogLineId$,
        stateTicks: this.stateTicks$
      }).subscribe({
        next: ({ displayWidth, rangeZoom, rangeFilter, listLogLineVisual, selectedLogLineId, highlightedLogLineId, stateTicks }) => {

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

          this.state$.next(state);
        }
      })
    );

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

  private generateLogTicks(listLogLine: LogLine[], rangeZoom: TimeRangeDuration, displayWidth: number, selectedId: number | null, highlightedId: number | null): LogTick[] {
    //const selectedId = this.selectedLogLineId();
    //const highlightedId = this.highlightedLogLineId();
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
    const state = this.state$.getValue();
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
    const state = this.state$.getValue();
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
    const state = this.state$.getValue();

    if (this.dragState.mode === 'start') {
      this.state$.next({ ...state, startFilterPositionX: Math.max(15, clientX) });
    } else if (this.dragState.mode === 'finish') {
      const newFinish = Math.min(state.displayWidth - 15, clientX);
      this.state$.next({ ...state, finishFilterPositionX: newFinish, finishFilterWidth: state.displayWidth - 15 - newFinish });
    }
    event.preventDefault();
  }

  onMouseUp(event: MouseEvent, _mode: 'start' | 'finish' | 'inner'): void {
    if (this.dragState.mode === '') return;

    const clientX = event.clientX - this.containerElement.nativeElement.getBoundingClientRect().left;
    const ts = this.toTimeFromPositionX(clientX);
    if (ts) {
      const currentFilter = this.rangeFilter$.getValue();
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
    const rangeZoom = this.rangeZoom$.getValue();
    const currentFilter = this.rangeFilter$.getValue();
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
    this.displayWidth$.next(this.containerElement.nativeElement.clientWidth);
  }
}

