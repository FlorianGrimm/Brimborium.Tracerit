import { AsyncPipe } from '@angular/common';
import { AfterViewInit, Component, ElementRef, HostListener, inject, Input, OnDestroy, output, Output, ViewChild } from '@angular/core';
import { DateTimeFormatter, Duration, ZonedDateTime, ZoneId } from '@js-joda/core';
import { BehaviorSubject, combineLatest, delay, distinctUntilChanged, map, Subscription } from 'rxjs';
import { LogTimeDataService } from '../Utility/log-time-data.service';

const epoch = ZonedDateTime.of(1970, 1, 1, 0, 0, 0, 0, ZoneId.UTC);

type TimeRulerViewModel = {
  displayWidth: number;
  viewBox: string;

  startZoom: ZonedDateTime;
  finishZoom: ZonedDateTime;
  durationZoom: Duration;

  startCurrent: ZonedDateTime | null;
  finishCurrent: ZonedDateTime | null;

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

  // startZoom$ = new BehaviorSubject<ZonedDateTime>(epoch);
  // finishZoom$ = new BehaviorSubject<ZonedDateTime>(epoch);
  // durationZoom$ = new BehaviorSubject<Duration>(Duration.ofSeconds(0));

  readonly state$ = new BehaviorSubject<TimeRulerViewModel>({
    displayWidth: 0,
    viewBox: '',
    startZoom: epoch,
    finishZoom: epoch,
    durationZoom: Duration.ofSeconds(0),
    startCurrent: null,
    finishCurrent: null,
  });

  // @Input()
  // finishComplete: ZonedDateTime | null = null;
  // finishCompleteChanged = output<ZonedDateTime | null>();

  // @Input()
  // startCurrent: ZonedDateTime | null = null;
  // startCurrentChanged = output<ZonedDateTime | null>();

  // @Input()
  // finishCurrent: ZonedDateTime | null = null;
  // finishCurrentChanged = output<ZonedDateTime | null>();

  // @Input()
  // startFilter: ZonedDateTime | null = null;
  // startFilterChanged = output<ZonedDateTime | null>();

  // @Input()
  // finishFilter: ZonedDateTime | null = null;
  // finishFilterChanged = output<ZonedDateTime | null>();


  // @Input()
  // startHighlight: ZonedDateTime | null = null;
  // startHighlightChanged = output<ZonedDateTime | null>();

  // @Input()
  // finishHighlight: ZonedDateTime | null = null;
  // finishHighlightChanged = output<ZonedDateTime | null>();

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
      }).pipe(
        delay(0)
      ).subscribe({
        next: (value) => {
          const result: TimeRulerViewModel = {
            displayWidth: value.displayWidth,
            viewBox: `0 0 ${value.displayWidth ?? 100} 100`,
            startZoom: value.startZoom,
            finishZoom: value.finishZoom,
            durationZoom: value.durationZoom,
            startCurrent: value.startCurrent,
            finishCurrent: value.finishCurrent,
          };
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
      const duration = Duration.between(state.startZoom, state.finishZoom);
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

}
