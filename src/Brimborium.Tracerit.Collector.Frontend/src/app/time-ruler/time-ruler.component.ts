import { AsyncPipe } from '@angular/common';
import { AfterViewInit, Component, ElementRef, HostListener, inject, Input, OnDestroy, output, Output, ViewChild } from '@angular/core';
import { DateTimeFormatter, Duration, ZonedDateTime, ZoneId } from '@js-joda/core';
import { BehaviorSubject, combineLatest, delay, distinctUntilChanged, map, Subscription } from 'rxjs';
import { LogTimeDataService } from '../Utility/log-time-data.service';
import { getLogLineTimestampValue, LogLine } from '../Api';

const epoch = ZonedDateTime.of(1970, 1, 1, 0, 0, 0, 0, ZoneId.UTC);

type TimeRulerViewModel = {
  displayWidth: number;
  viewBox: string;
  startZoom: ZonedDateTime;
  finishZoom: ZonedDateTime;
  durationZoom: Duration;
  startCurrent: ZonedDateTime | null;
  finishCurrent: ZonedDateTime | null;
  tickInterval: Duration;
  tickUnit: string;
  majorTickEvery: number;
  listTick: TimescaleTick[];
  listLogTick: LogTick[];
};

type TimescaleTick = {
  position: number;
  isMajor: boolean;
  label: string;
};
type LogTick = {
  id: number;
  positionX1: number;
  positionX2: number;
  positionY: number;
  color:string;
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
})
export class TimeRulerComponent implements AfterViewInit, OnDestroy {
  subscription = new Subscription();
  subscriptionState = new Subscription();
  readonly logTimeDataService = inject(LogTimeDataService);

  readonly state$ = new BehaviorSubject<TimeRulerViewModel>({
    displayWidth: 0,
    viewBox: '',
    startZoom: epoch,
    finishZoom: epoch,
    durationZoom: Duration.ofSeconds(0),
    startCurrent: null,
    finishCurrent: null,
    tickInterval: Duration.ofMillis(100),
    tickUnit: 'ms',
    majorTickEvery: 10,
    listTick: [],
    listLogTick:[]
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
      combineLatest({
        displayWidth: this.displayWidth$.pipe(
          distinctUntilChanged(),
          delay(0)
        ),
        startZoom: this.logTimeDataService.startZoom$,
        finishZoom: this.logTimeDataService.finishZoom$,
        durationZoom: this.logTimeDataService.durationZoom$,
        startCurrent: this.logTimeDataService.startCurrent$,
        finishCurrent: this.logTimeDataService.finishCurrent$,
        listLogLineFiltered: this.logTimeDataService.listLogLineFiltered$,
      }).pipe(
        delay(0)
      ).subscribe({
        next: (value) => {

          let { tickInterval, tickUnit, majorTickEvery }: { tickInterval: Duration; tickUnit: string; majorTickEvery: number; } = this.calculateTicksBaseInfo(value);

          const result: TimeRulerViewModel = {
            displayWidth: value.displayWidth,
            viewBox: `0 0 ${value.displayWidth ?? 100} 100`,
            startZoom: value.startZoom,
            finishZoom: value.finishZoom,
            durationZoom: value.durationZoom,
            startCurrent: value.startCurrent,
            finishCurrent: value.finishCurrent,
            tickInterval:tickInterval,
            tickUnit:tickUnit,
            majorTickEvery:majorTickEvery,
            listTick: [],
            listLogTick:[]
          };
          this.generateTicks(result);
          this.generateLogTicks(value.listLogLineFiltered, result)
          console.log("state", result);
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


  private calculateTicksBaseInfo(value: { displayWidth: number; startZoom: ZonedDateTime; finishZoom: ZonedDateTime; durationZoom: Duration; startCurrent: ZonedDateTime | null; finishCurrent: ZonedDateTime | null; listLogLineFiltered: LogLine[]; }) {
    let duration = value.durationZoom;
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

  generateTicks(state: TimeRulerViewModel) {
    const ticks: Array<TimescaleTick> = state.listTick;
    ticks.splice(0, ticks.length);

    let currentTime = state.startZoom;
    let tickCount = 0;

    while (currentTime.isBefore(state.finishZoom) || currentTime.equals(state.finishZoom)) {
      const position = this.toPositionX(currentTime);
      const isMajor
        = (tickCount !== 0)
          &&((tickCount % state.majorTickEvery === 0)
            || (tickCount % 5=== 0)
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

      ticks.push({ position, isMajor, label });

      currentTime = currentTime.plus(state.tickInterval);
      tickCount++;

      // Safety break to prevent infinite loops
      if (tickCount > 1000) break;
    }
  }

  generateLogTicks(listLogLineFiltered: LogLine[], result: TimeRulerViewModel) {
    result.listLogTick.splice(0, result.listLogTick.length);
    let oldPosition = 15;
    let index=0;
    const colors=["red","green","blue","yellow","orange","purple","pink","brown","gray","black"];
    for (const logLine of listLogLineFiltered) {
      const ts=logLine.data.get("timestamp")?.value as (ZonedDateTime | undefined | null);
      if (ts === undefined || ts === null) { continue; }
      const position = this.toPositionX(ts);
      index++;;
      result.listLogTick.push({
        id: logLine.id,
        positionX1: oldPosition,
        positionX2: position,
        positionY: (index % 30)*2,
        color:colors[index % colors.length],
        logLine:logLine });
        oldPosition = position;
    }
  }
}
