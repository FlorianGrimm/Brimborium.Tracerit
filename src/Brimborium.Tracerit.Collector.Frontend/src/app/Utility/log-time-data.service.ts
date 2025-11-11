import { Injectable } from '@angular/core';
import { BehaviorSubject, combineLatest, debounceTime, delay, distinctUntilChanged, Subscription } from 'rxjs';
import { getLogLineTimestampValue, LogLine } from '../Api';
import { Duration, ZonedDateTime, ZoneId } from '@js-joda/core';

const epoch0 = ZonedDateTime.of(1970, 1, 1, 0, 0, 0, 0, ZoneId.UTC);
const epoch1 = ZonedDateTime.of(1970, 1, 1, 1, 1, 1, 1, ZoneId.UTC);

export type TimeRange = {
  start: ZonedDateTime;
  finish: ZonedDateTime;
};

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

  public rangeComplete$ = new BehaviorSubject<TimeRange>({
    start: epoch0,
    finish: epoch1,
  });

  // input
  public modeZoom$ = new BehaviorSubject<'complete'>('complete');
  // depended modeZoom$ listLogLineFiltered$
  public startZoom$ = new BehaviorSubject<ZonedDateTime>(epoch0);
  // depended modeZoom$ listLogLineFiltered$
  public finishZoom$ = new BehaviorSubject<ZonedDateTime>(epoch1);
  public rangeZoom$ = new BehaviorSubject<TimeRange>({
    start: epoch0,
    finish: epoch1,
  });

  // depended startZoom$ finishZoom$
  public durationZoom$ = new BehaviorSubject<Duration>(Duration.ofSeconds(0));

  public startFilter$ = new BehaviorSubject<ZonedDateTime>(epoch0);
  public finishFilter$ = new BehaviorSubject<ZonedDateTime>(epoch1);

  public startCurrent$ = new BehaviorSubject<ZonedDateTime | null>(null);
  public finishCurrent$ = new BehaviorSubject<ZonedDateTime | null>(null);

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
        delay(0)
        //debounceTime(10)
        // distinctUntilChanged((a, b) => {
        //   return a.modeZoom === b.modeZoom
        //     && a.startComplete === b.startComplete
        //     && a.finishComplete === b.finishComplete;
        // })
      ).subscribe({
        next: (value) => {
          if ('complete' === value.modeZoom) {
            console.log("LogTimeDataService.modeZoom-complete-startZoom",
              {
                modeZoom: value.modeZoom,
                startComplete: value.startComplete.toString(),
                finishComplete: value.finishComplete.toString(),
              });
            if (this.startZoom$.getValue().compareTo(value.startComplete) !== 0) {
              this.startZoom$.next(value.startComplete);
            }
            if (this.finishZoom$.getValue().compareTo(value.finishComplete) !== 0) {
              this.finishZoom$.next(value.finishComplete);
            }
          }
        }
      })
    );

    this.subscription.add(
      combineLatest({
        modeZoom: this.modeZoom$,
        startZoom: this.startZoom$,
        finishZoom: this.finishZoom$,
      }).pipe(
        delay(0)
        // distinctUntilChanged((a, b) => {
        //   return a.modeZoom === b.modeZoom
        //     && a.startZoom === b.startZoom
        //     && a.finishZoom === b.finishZoom;
        // })
      ).subscribe({
        next: (value) => {
          const duration = Duration.between(value.startZoom, value.finishZoom);
          console.log("LogTimeDataService.modeZoom-complete-durationZoom",
            {
              modeZoom: value.modeZoom,
              startZoom: value.startZoom.toString(),
              finishZoom: value.finishZoom.toString(),
              duration: duration.toString(),
            });
          if (this.durationZoom$.getValue().compareTo(duration) !== 0) {
            this.durationZoom$.next(duration);
          }
        }
      })
    );

    this.subscription.add(
      combineLatest({
        listLogLine: this.listLogLine$,
        currentLogLineId: this.currentLogLineId$,
      }).pipe(
        debounceTime(10)
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
            if (this.currentLogTimestamp$.getValue()?.compareTo(ts) !== 0) {
              this.currentLogTimestamp$.next(ts);
            }
            if (this.startCurrent$.getValue()?.compareTo(ts) !== 0) {
              this.startCurrent$.next(ts);
            }

          }
        }
      })
    );
  }
}
