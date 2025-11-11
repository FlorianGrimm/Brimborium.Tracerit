import { AsyncPipe } from '@angular/common';
import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, HostListener, inject, Input, OnDestroy, output, Output, ViewChild } from '@angular/core';
import { DateTimeFormatter, Duration, ZonedDateTime, ZoneId } from '@js-joda/core';
import { BehaviorSubject, combineLatest, debounce, debounceTime, delay, distinctUntilChanged, map, Subscription } from 'rxjs';
import { LogTimeDataService } from '../Utility/log-time-data.service';
import { getLogLineTimestampValue, LogLine } from '../Api';
import { setTimeRangeFinishIfChanged, setTimeRangeOrNullIfChanged, setTimeRangeStartIfChanged, TimeRangeDuration, TimeRangeOrNull } from '../Utility/time-range';

const epoch0 = ZonedDateTime.of(1970, 1, 1, 0, 0, 0, 0, ZoneId.UTC);
const epoch1 = ZonedDateTime.of(1970, 1, 1, 1, 1, 1, 1, ZoneId.UTC);

const LogLineColors = ["red", "green", "blue", "yellow", "orange", "purple", "pink", "brown", "gray", "black"];

type TimeRulerViewModel = {
  displayWidth: number;
  viewBox: string;

  startZoom: ZonedDateTime;
  finishZoom: ZonedDateTime;
  durationZoom: Duration;

  startCurrent: ZonedDateTime | null;
  finishCurrent: ZonedDateTime | null;

  startFilter: ZonedDateTime | null;
  finishFilter: ZonedDateTime | null;

  startFilterPositionX: number | null,
  finishFilterPositionX: number | null,
  finishFilterWidth: number | null,

  tickInterval: Duration;
  tickUnit: string;
  majorTickEvery: number;

  listLogLineFiltered: LogLine[];
  listTick: TimescaleTick[];
  listLogTick: LogTick[];
};

type TimescaleTick = {
  id: number;
  position: number;
  isMajor: boolean;
  label: string;
};
type LogTick = {
  id: number;
  positionX1: number;
  positionX2: number;
  positionY: number;
  color: string;
  logLine: LogLine;
};

const dateTimeFormatterHHmmssSSS = DateTimeFormatter.ofPattern("HH:mm:ss.SSS");

@Component({
  selector: 'app-time-ruler',
  imports: [
    AsyncPipe
  ],
  templateUrl: './time-ruler.component.html',
  styleUrl: './time-ruler.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TimeRulerComponent implements AfterViewInit, OnDestroy {
  subscription = new Subscription();
  subscriptionState = new Subscription();
  readonly logTimeDataService = inject(LogTimeDataService);

  public rangeFilter$ = new BehaviorSubject<TimeRangeOrNull>(Object.freeze({ start: null, finish: null }));

  readonly state$ = new BehaviorSubject<TimeRulerViewModel>({
    displayWidth: 0,
    viewBox: '',

    startZoom: epoch0,
    finishZoom: epoch1,
    durationZoom: Duration.between(epoch0, epoch1),

    startCurrent: null,
    finishCurrent: null,

    startFilter: null,
    finishFilter: null,

    startFilterPositionX: null,
    finishFilterPositionX: null,
    finishFilterWidth: 0,

    tickInterval: Duration.ofMillis(100),
    tickUnit: 'ms',
    majorTickEvery: 10,

    listLogLineFiltered: [],
    listTick: [],
    listLogTick: []
  });

  @ViewChild('containerElement', { static: true }) containerElement!: ElementRef<HTMLDivElement>;
  @ViewChild('svgElement', { static: true }) svgElement!: ElementRef<SVGSVGElement>;

  public readonly displayWidth$ = new BehaviorSubject<number>(800);

  constructor() {
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
    this.subscriptionState.unsubscribe();
  }

  ngAfterViewInit(): void {
    this.updateViewBox();

    this.subscriptionState.add(
      this.logTimeDataService.rangeFilter$.subscribe({
        next: (value) => {
          setTimeRangeOrNullIfChanged(this.rangeFilter$, value);
        }
      })
    );

    this.subscriptionState.add(
      combineLatest({
        listLogLineFiltered: this.logTimeDataService.listLogLineFilteredCondition$,
        currentLogLineId: this.logTimeDataService.currentLogLineId$,
        currentLogLine: this.logTimeDataService.currentLogLine$,
      }).pipe(
        delay(0)
      ).subscribe({
        next: (value) => {
          if (0 < value.listLogLineFiltered.length
            && ((null === value.currentLogLineId) || (undefined === value.currentLogLineId))
          ) {
            this.logTimeDataService.currentLogLineId$.next(value.listLogLineFiltered[0].id);
          }
        }
      })
    );
    this.subscriptionState.add(
      combineLatest({
        listLogLineFiltered: this.logTimeDataService.listLogLineFilteredCondition$,
        rangeZoom: this.logTimeDataService.rangeZoom$,
        rangeFilter: this.logTimeDataService.rangeFilter$,
      }).pipe(
        delay(0)
      ).subscribe({
        next: (value) => {
          if ((null === value.rangeFilter.start) && (null === value.rangeFilter.finish)) {
            if (1 < value.listLogLineFiltered.length) {
              const tsStart = getLogLineTimestampValue(value.listLogLineFiltered[0]);
              const tsFinish = getLogLineTimestampValue(value.listLogLineFiltered[value.listLogLineFiltered.length - 1]);
              setTimeRangeOrNullIfChanged(this.rangeFilter$, {
                start: tsStart,
                finish: tsFinish
              });
            }
          } else {
            // startZoom<=startFilter<=finishFilter<=finishZoom
            let tsStart: ZonedDateTime;
            let tsFinish: ZonedDateTime;
            if (value.rangeZoom.start.compareTo(value.rangeFilter.start) <= 0
              && value.rangeFilter.start.compareTo(value.rangeZoom.finish) <= 0
            ) {
              tsStart = value.rangeFilter.start;
            } else {
              tsStart = value.rangeZoom.start;
            }
            if (value.rangeZoom.start.compareTo(value.rangeFilter.finish) <= 0
              && value.rangeFilter.finish.compareTo(value.rangeZoom.finish) <= 0
            ) {
              tsFinish = value.rangeFilter.finish;
            } else {
              tsFinish = value.rangeZoom.finish;
            }
            setTimeRangeOrNullIfChanged(this.rangeFilter$, { start: tsStart, finish: tsFinish });
          }
        }
      })
    );

    this.subscriptionState.add(
      combineLatest({
        displayWidth: this.displayWidth$,

        rangeZoom: this.logTimeDataService.rangeZoom$,

        rangeCurrent: this.logTimeDataService.rangeCurrent$,

        rangeFilter: this.rangeFilter$,

        listLogLineFiltered: this.logTimeDataService.listLogLineFilteredCondition$,
      }).pipe(
        delay(0),
      ).subscribe({
        next: (value) => {

          let { tickInterval, tickUnit, majorTickEvery }: { tickInterval: Duration; tickUnit: string; majorTickEvery: number; } = this.calculateTicksBaseInfo(value);
          // startZoom<=startFilter<=finishFilter<=finishZoom
          let startFilter: ZonedDateTime;
          if (null !== value.rangeFilter.start
            && value.rangeZoom.start.compareTo(value.rangeFilter.start) <= 0
            && value.rangeFilter.start.compareTo(value.rangeZoom.finish) <= 0
          ) {
            startFilter = value.rangeFilter.start;
          } else {
            startFilter = value.rangeZoom.start;
          }

          let finishFilter: ZonedDateTime;
          if (null !== value.rangeFilter.finish
            && value.rangeZoom.start.compareTo(value.rangeFilter.finish) <= 0
            && value.rangeFilter.finish.compareTo(value.rangeZoom.finish) <= 0
          ) {
            finishFilter = value.rangeFilter.finish;
          } else {
            finishFilter = value.rangeZoom.finish;
          }

          const startFilterPositionX = this.toPositionX(startFilter);
          const finishFilterPositionX = this.toPositionX(finishFilter);
          const finishFilterWidth = value.displayWidth - finishFilterPositionX;

          const result: TimeRulerViewModel = {
            displayWidth: value.displayWidth,
            viewBox: `0 0 ${value.displayWidth ?? 100} 100`,

            startZoom: value.rangeZoom.start,
            finishZoom: value.rangeZoom.finish,
            durationZoom: value.rangeZoom.duration,

            startCurrent: value.rangeCurrent.start,
            finishCurrent: value.rangeCurrent.finish,

            startFilter: startFilter,
            finishFilter: finishFilter,

            startFilterPositionX: startFilterPositionX,
            finishFilterPositionX: finishFilterPositionX,
            finishFilterWidth: finishFilterWidth,

            tickInterval: tickInterval,
            tickUnit: tickUnit,
            majorTickEvery: majorTickEvery,

            listLogLineFiltered: value.listLogLineFiltered,
            listTick: [],
            listLogTick: []
          };
          console.log("state", {
            startZoom: result.startZoom?.toString(),
            finishZoom: result.finishZoom?.toString(),
            startFilter: result.startFilter?.toString(),
            finishFilter: result.finishFilter?.toString(),
            startFilterPositionX: result.startFilterPositionX,
            finishFilterPositionX: result.finishFilterPositionX
          });
          /*
          this.generateTicks(result);
          this.generateLogTicks(value.listLogLineFiltered, result)
          */
          this.state$.next(result);
        }
      })
    );
  }

  @HostListener('window:resize')
  onResize() {
    this.updateViewBox();
  }

  private updateViewBox(): void {
    const containerElement = this.containerElement.nativeElement;
    const containerWidth = containerElement.clientWidth;
    this.displayWidth$.next(containerWidth);
  }

  toPositionX(value: ZonedDateTime | null) {
    const state = this.state$.getValue();
    if (null === state.startZoom
      || null === state.finishZoom
      || null === value
      || 0 === state.displayWidth
    ) {
      return 15;
    } else {

      const duration = state.durationZoom;
      const durationMillis = duration.toMillis();
      const durationCurrent = Duration.between(state.startZoom, value);
      const durationCurrentMillis = durationCurrent.toMillis();
      const displayWidth = this.displayWidth$.getValue() - 30;
      if (durationMillis <= 0) {
        return 15;
      } else {
        const result = 15 + ((durationCurrentMillis / durationMillis) * displayWidth);
        return result;
      }
    }
  }

  toTimeString(value: ZonedDateTime | null) {
    if (value === null) { return ""; }
    try {
      return value.format(dateTimeFormatterHHmmssSSS);
    } catch (error) {
      console.error(error);
      return "";
    }
  }


  private calculateTicksBaseInfo(
    value: {
      displayWidth: number;
      rangeZoom: TimeRangeDuration;
    }) {
    let duration = value.rangeZoom.duration;
    let displayWidth = value.displayWidth - 30;
    // Calculate timeline ruler scale and unit
    let tickInterval: Duration;
    let tickUnit: string;
    let majorTickEvery: number;
    const durationMillis = duration.toMillis();
    const pixelsPerTick = 10; // Minimum pixels between ticks
    const desiredTickCount = displayWidth / pixelsPerTick;
    const millisPerTick = durationMillis / desiredTickCount;

    if (millisPerTick < 1) {
      tickInterval = Duration.ofMillis(1);
      tickUnit = 'ms';
      majorTickEvery = 100;
    } else if (millisPerTick < 10) {
      tickInterval = Duration.ofMillis(10);
      tickUnit = 'ms';
      majorTickEvery = 10;
    } else if (millisPerTick < 100) {
      tickInterval = Duration.ofMillis(100);
      tickUnit = 'ms';
      majorTickEvery = 10;
    } else if (millisPerTick < 1000) {
      tickInterval = Duration.ofSeconds(1);
      tickUnit = 's';
      majorTickEvery = 10;
    } else if (millisPerTick < 10000) {
      tickInterval = Duration.ofSeconds(10);
      tickUnit = 's';
      majorTickEvery = 6;
    } else if (millisPerTick < 60000) {
      tickInterval = Duration.ofMinutes(1);
      tickUnit = 'min';
      majorTickEvery = 10;
    } else {
      tickInterval = Duration.ofMinutes(10);
      tickUnit = 'min';
      majorTickEvery = 6;
    }
    return { tickInterval, tickUnit, majorTickEvery };
  }

  getListTick(state: TimeRulerViewModel): TimescaleTick[]{
    if(state.listTick.length === 0){
      this.generateTicks(state);
    }
    return state.listTick;
  }

  generateTicks(state: TimeRulerViewModel) {
    const ticks: Array<TimescaleTick> = state.listTick;
    ticks.splice(0, ticks.length);

    let currentTime = state.startZoom;
    let tickCount = 0;
    let index = 0;

    while (currentTime.isBefore(state.finishZoom) || currentTime.equals(state.finishZoom)) {
      const position = this.toPositionX(currentTime);
      const isMajor
        = (tickCount !== 0)
        && ((tickCount % state.majorTickEvery === 0)
          || (tickCount % 5 === 0)
        );

      let label = '';
      if (isMajor) {
        if (state.tickUnit === 'ms') {
          label = `${currentTime.toInstant().toEpochMilli() % 1000}ms`;
        } else if (state.tickUnit === 's') {
          label = `${currentTime.second()}s`;
        } else if (state.tickUnit === 'min') {
          label = `${currentTime.minute()}:${currentTime.second().toString().padStart(2, '0')}`;
        }
      }

      ticks.push({ id: index++, position, isMajor, label });

      currentTime = currentTime.plus(state.tickInterval);
      tickCount++;

      // Safety break to prevent infinite loops
      if (state.displayWidth < tickCount) {
        break;
      }
    }
  }

  getListLogTick(state: TimeRulerViewModel): LogTick[] {
    if (state.listLogTick.length === 0) {
      this.generateLogTicks(state.listLogLineFiltered, state);
    }
    return state.listLogTick;
  }

  generateLogTicks(listLogLineFiltered: LogLine[], result: TimeRulerViewModel) {
    result.listLogTick.splice(0, result.listLogTick.length);
    let oldPosition = 15;
    let index = 0;

    for (const logLine of listLogLineFiltered) {
      const ts = logLine.data.get("timestamp")?.value as (ZonedDateTime | undefined | null);
      if (ts === undefined || ts === null) { continue; }
      const position = this.toPositionX(ts);
      index++;;
      result.listLogTick.push({
        id: logLine.id,
        positionX1: oldPosition,
        positionX2: position,
        positionY: (index % 30) * 2,
        color: LogLineColors[index % LogLineColors.length],
        logLine: logLine
      });
      oldPosition = position;
    }
  }

  private toTimeFromPositionX(positionX: number): ZonedDateTime | null {
    const state = this.state$.getValue();
    if (!state.startZoom || !state.finishZoom || state.displayWidth === 0) {
      return null;
    }

    const displayWidth = state.displayWidth - 30;
    const relativeX = positionX - 15;
    const ratio = Math.max(0, Math.min(1, relativeX / displayWidth));

    const durationMillis = state.durationZoom.toMillis();
    const offsetMillis = durationMillis * ratio;

    return state.startZoom.plusNanos(offsetMillis * 1000000);
  }

  dragState: {
    mode: 'start' | 'finish' | '';
    startClientX: number;
    startPositionX: number;
  } = {
      mode: '',
      startClientX: 0,
      startPositionX: 0,
    }

  onMouseDown($event: MouseEvent, mode: 'start' | 'finish') {
    const clientX = clientXToPositionX($event.clientX);
    const state = this.state$.getValue();
    console.log("clientX", clientX, "startFilterPositionX", state.startFilterPositionX);
    if (mode === 'start') {
      if (0 <= clientX && clientX < (state.startFilterPositionX ?? 0)) {
        this.dragState = {
          mode: 'start',
          startClientX: clientX,
          startPositionX: (state.startFilterPositionX ?? 0),
        }
      };
      console.log(this.dragState);
    } else if (mode === 'finish') {
      if ((state.finishFilterPositionX ?? 0) <= clientX && clientX < (state.displayWidth ?? 0)) {
        this.dragState = {
          mode: 'finish',
          startClientX: clientX,
          startPositionX: (state.startFilterPositionX ?? 0),
        }
      };
      console.log(this.dragState);
    }
    $event.preventDefault();
    $event.stopPropagation();
  }
  onMouseMove($event: MouseEvent, mode: 'start' | 'finish' | 'inner') {
    if (this.dragState.mode === mode
      || this.dragState.mode !== '' && mode === 'inner'
    ) {
      const clientX = clientXToPositionX($event.clientX);
      const ts = this.toTimeFromPositionX(clientX);
      if (ts === null) { return; }
      console.log($event.clientX, clientX, this.dragState, ts.toString());
      const currentState = this.state$.getValue();
      if (this.dragState.mode === 'start') {
        this.state$.next({
          ...currentState,
          startFilterPositionX: clientX
        });
      } else if (this.dragState.mode === 'finish') {
        this.state$.next({
          ...currentState,
          finishFilterPositionX: clientX,
          finishFilterWidth: currentState.displayWidth - clientX
        });
      }
      $event.preventDefault();
      $event.stopPropagation();
    }
  }
  onMouseUp($event: MouseEvent, mode: 'start' | 'finish' | 'inner') {
    if ((this.dragState.mode === mode)
      || (this.dragState.mode !== '' && mode === 'inner')) {
      const clientX = clientXToPositionX($event.clientX);
      const ts = this.toTimeFromPositionX(clientX);
      if (ts === null) { return; }
      console.log("onMouseUp", this.dragState.mode, ts.toString());
      if (this.dragState.mode === 'start') {
        setTimeRangeStartIfChanged(this.logTimeDataService.rangeFilter$, ts);
      } else if (this.dragState.mode === 'finish') {
        setTimeRangeFinishIfChanged(this.logTimeDataService.rangeFilter$, ts);
      }
      this.dragState = { mode: '', startClientX: 0, startPositionX: 0 };
      $event.preventDefault();
      $event.stopPropagation();
    }
  }
  onMouseDblClick(mode: 'start' | 'finish') {
    if (this.dragState.mode === '') {
      if (mode === 'start') {
        const nextStart = this.logTimeDataService.startComplete$.getValue();
        setTimeRangeStartIfChanged(this.logTimeDataService.rangeFilter$, nextStart);
      } else if (mode === 'finish') {
        const nextFinish = this.logTimeDataService.finishComplete$.getValue();
        setTimeRangeFinishIfChanged(this.logTimeDataService.rangeFilter$, nextFinish);
      }
    }
  }

}
function clientXToPositionX(clientX: number) {
  return clientX - 48;
}
