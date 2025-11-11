import { Injectable } from '@angular/core';
import { BehaviorSubject, combineLatest, debounceTime, delay, distinctUntilChanged, Subscription } from 'rxjs';
import { getLogLineTimestampValue, LogLine } from '../Api';
import { Duration, ZonedDateTime, ZoneId } from '@js-joda/core';
import { epoch0, epoch1, setTimeRangeDurationIfChanged, setTimeRangeOrNullIfChanged, setTimeRangeOrNullStartIfChanged, TimeRange, TimeRangeDuration, TimeRangeOrNull } from './time-range';
@Injectable({
  providedIn: 'root',
})
export class LogTimeDataService {
  // input
  public listLogLine$ = new BehaviorSubject<LogLine[]>([]);
  // input
  public listLogLineFilteredCondition$ = new BehaviorSubject<LogLine[]>([]);
  // input
  public listLogLineFilteredTime$ = new BehaviorSubject<LogLine[]>([]);

  // input
  public currentLogLineId$ = new BehaviorSubject<number | null>(null);
  // depended listLogLineFiltered$ currentLogLineId$
  public currentLogLine$ = new BehaviorSubject<LogLine | null>(null);
  public currentLogTimestamp$ = new BehaviorSubject<(ZonedDateTime | null)>(null);

  // input
  public listLogLineIdHighlighted$ = new BehaviorSubject<Set<string>>(new Set<string>());

  // depended listLogLineFiltered$
  public startComplete$ = new BehaviorSubject<ZonedDateTime>(epoch0);
  // depended listLogLineFiltered$
  public finishComplete$ = new BehaviorSubject<ZonedDateTime>(epoch1);

  public rangeComplete$ = new BehaviorSubject<TimeRange>(Object.freeze({
    start: epoch0,
    finish: epoch1,
  }));

  // input
  public modeZoom$ = new BehaviorSubject<'complete'>('complete');
  // depended modeZoom$ listLogLineFiltered$
  //public startZoom$ = new BehaviorSubject<ZonedDateTime>(epoch0);
  // depended modeZoom$ listLogLineFiltered$
  //public finishZoom$ = new BehaviorSubject<ZonedDateTime>(epoch1);
  // depended startZoom$ finishZoom$
  //public durationZoom$ = new BehaviorSubject<Duration>(Duration.ofSeconds(0));

  public rangeZoom$ = new BehaviorSubject<TimeRangeDuration>(Object.freeze({
    start: epoch0,
    finish: epoch1,
    duration: Duration.between(epoch0, epoch1)
  }));

  public rangeFilter$ = new BehaviorSubject<TimeRange>(Object.freeze({
    start: epoch0,
    finish: epoch1,
  }));

  public rangeCurrent$ = new BehaviorSubject<TimeRangeOrNull>(Object.freeze({ start: null, finish: null }));

  subscription = new Subscription();

  constructor() {
    this.subscription.add(
      this.listLogLineFilteredCondition$
        .subscribe({
          next: (value) => {
            if (value.length < 2) {
              this.startComplete$.next(epoch0);
              this.finishComplete$.next(epoch1);
            } else {
              const first = getLogLineTimestampValue(value[0]) ?? ZonedDateTime.now();
              const last = getLogLineTimestampValue(value[value.length - 1]) ?? first.plusSeconds(1);
              if (this.startComplete$.getValue().compareTo(first) !== 0) {
                this.startComplete$.next(first);
              }
              if (this.finishComplete$.getValue().compareTo(last) !== 0) {
                this.finishComplete$.next(last);
              }
            }
          }
        })
    );

    this.subscription.add(
      combineLatest({
        modeZoom: this.modeZoom$,
        startComplete: this.startComplete$,
        finishComplete: this.finishComplete$,
      }).pipe(
        delay(0),
        distinctUntilChanged((a, b) => {
          return a.modeZoom === b.modeZoom
            && a.startComplete.isEqual(b.startComplete)
            && a.finishComplete.isEqual( b.finishComplete);
        })
      ).subscribe({
        next: (value) => {
          if ('complete' === value.modeZoom) {
            console.log("LogTimeDataService.modeZoom-complete-startZoom",
              {
                modeZoom: value.modeZoom,
                startComplete: value.startComplete.toString(),
                finishComplete: value.finishComplete.toString(),
              });
              setTimeRangeDurationIfChanged(this.rangeZoom$, {
                start: value.startComplete,
                finish: value.finishComplete,
              });
          }
        }
      })
    );

    this.subscription.add(
      combineLatest({
        listLogLine: this.listLogLine$,
        currentLogLineId: this.currentLogLineId$,
      }).pipe(
        delay(0)
      ).subscribe({
        next: (value) => {
          const currentLogLine = ((null === value.currentLogLineId) || (undefined === value.currentLogLineId))
            ? undefined
            : value.listLogLine.find(item => item.id === value.currentLogLineId);

          console.log("LogTimeDataService.currentLogLineId-currentLogLine", currentLogLine);

          if (undefined === currentLogLine) {
            this.currentLogLine$.next(null);
            this.currentLogTimestamp$.next(null);
          } else {
            this.currentLogLine$.next(currentLogLine);
            const ts = getLogLineTimestampValue(currentLogLine);
            if (null === ts) { return; }
            if (!(this.currentLogTimestamp$.getValue()?.isEqual(ts))) {
              this.currentLogTimestamp$.next(ts);
            }
            setTimeRangeOrNullIfChanged(this.rangeCurrent$, {start: ts, finish: null});
          }
        }
      })
    );
  }
}
