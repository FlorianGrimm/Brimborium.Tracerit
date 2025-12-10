import { AsyncPipe } from '@angular/common';
import {
  AfterViewInit, ChangeDetectionStrategy, Component, ElementRef,
  HostListener, inject, input, OnDestroy, output, ViewChild
} from '@angular/core';
import { DateTimeFormatter, Duration, ZonedDateTime, ZoneId } from '@js-joda/core';
import { BehaviorSubject, combineLatest, delay, filter, Subscription } from 'rxjs';
import { toObservable } from '@angular/core/rxjs-interop';
import { getLogLineTimestampValue, LogLine } from '../../Api';
import { BehaviorRingSubject } from '../../Utility/BehaviorRingSubject';
import { MasterRingService } from '../../Utility/master-ring.service';
import { TimeRangeDuration, TimeRangeOrNull, setTimeRangeFinishIfChanged, setTimeRangeOrNullIfChanged, setTimeRangeStartIfChanged } from '../../Utility/time-range';

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

export type TimescaleTick = {
  id: number;
  position: number;
  isMajor: boolean;
  label: string;
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

export type VisibleRange = {
  startIndex: number;
  endIndex: number;
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

  // Inputs
  readonly listLogLine = input<LogLine[]>([]);
  readonly rangeZoom = input<TimeRangeDuration>({ start: epoch0, finish: epoch1, duration: Duration.between(epoch0, epoch1) });
  readonly rangeFilter = input<TimeRangeOrNull>({ start: null, finish: null });
  readonly selectedLogLineId = input<number | null>(null);
  readonly highlightedLogLineId = input<number | null>(null);
  readonly visibleRange = input<VisibleRange | null>(null);

  readonly rangeZoom$ = toObservable(this.rangeZoom);
  readonly rangeFilter$ = toObservable(this.rangeFilter);
  readonly listLogLine$ = toObservable(this.listLogLine);
  readonly selectedLogLineId$ = toObservable(this.selectedLogLineId);
  readonly highlightedLogLineId$ = toObservable(this.highlightedLogLineId);
  readonly visibleRange$ = toObservable(this.visibleRange);


  // Outputs
  readonly rangeFilterChange = output<TimeRangeOrNull>();
  readonly logLineClick = output<LogLine>();
  readonly logLineHover = output<LogLine | null>();

  // Internal state
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
    this.subscription.add(
      combineLatest({
        displayWidth: this.displayWidth$.pipe(filter(w => w > 0)),
        rangeZoom:            this.rangeZoom$,
        rangeFilter:          this.rangeFilter$,
        listLogLine:          this.listLogLine$,
        selectedLogLineId:    this.selectedLogLineId$,
        highlightedLogLineId: this.highlightedLogLineId$,
        visibleRange:         this.visibleRange$
      }).subscribe({
        next: ({ displayWidth, rangeZoom, rangeFilter, listLogLine, selectedLogLineId, highlightedLogLineId, visibleRange }) => {

          const { tickInterval, tickUnit, majorTickEvery } = this.calculateTicksBaseInfo(displayWidth, rangeZoom);

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

          const listTick = this.generateTicks(rangeZoom, displayWidth, tickInterval, tickUnit, majorTickEvery);
          const listLogTick = this.generateLogTicks(listLogLine, rangeZoom, displayWidth, selectedLogLineId, highlightedLogLineId);

          const selectedLogTick = listLogTick.find(t => t.id === selectedLogLineId) ?? null;

          const state: TimeScale2ViewModel = {
            displayWidth,
            viewBox: `0 0 ${displayWidth} 100`,
            rangeZoom,
            rangeFilter,
            startFilterPositionX,
            finishFilterPositionX,
            finishFilterWidth,
            tickInterval,
            tickUnit,
            majorTickEvery,
            listTick: listTick,
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

      ticks.push({ id: tickCount, position, isMajor, label });
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
        positionY: 5 + (i % 25) * 2,
        width: 3,
        color: LogLineColors[i % LogLineColors.length],
        isHighlighted: logLine.id === highlightedId,
        isSelected: logLine.id === selectedId,
        logLine:logLine,
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

    const startTick = state.listLogTick[Math.min(visible.startIndex, state.listLogTick.length - 1)];
    const endTick = state.listLogTick[Math.min(visible.endIndex, state.listLogTick.length - 1)];
    if (!startTick || !endTick) return null;

    return { x: startTick.positionX, width: Math.max(10, endTick.positionX - startTick.positionX + endTick.width) };
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
      const currentFilter = this.rangeFilter();
      if (this.dragState.mode === 'start') {
        this.rangeFilterChange.emit({ start: ts, finish: currentFilter.finish });
      } else if (this.dragState.mode === 'finish') {
        this.rangeFilterChange.emit({ start: currentFilter.start, finish: ts });
      }
    }
    this.dragState = { mode: '', startClientX: 0, startPositionX: 0 };
    event.preventDefault();
  }

  onMouseDblClick(mode: 'start' | 'finish'): void {
    const rangeZoom = this.rangeZoom();
    const currentFilter = this.rangeFilter();
    if (mode === 'start') {
      this.rangeFilterChange.emit({ start: rangeZoom.start, finish: currentFilter.finish });
    } else {
      this.rangeFilterChange.emit({ start: currentFilter.start, finish: rangeZoom.finish });
    }
  }

  // Public method to trigger refresh
  refresh(): void {
    this.displayWidth$.next(this.containerElement.nativeElement.clientWidth);
  }
}

