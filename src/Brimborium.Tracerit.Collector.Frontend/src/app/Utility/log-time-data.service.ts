import { Injectable } from '@angular/core';
import { BehaviorSubject, combineLatest, Subscription } from 'rxjs';
import { getLogLineTimestampValue, LogLine } from '../Api';
import { Duration, ZonedDateTime, ZoneId } from '@js-joda/core';

const epoch = ZonedDateTime.of(1970, 1, 1, 0, 0, 0, 0, ZoneId.UTC);

@Injectable({
  providedIn: 'root',
})
export class LogTimeDataService {
  // input
  public listLogLine$ = new BehaviorSubject<LogLine[]>([]);
  // input
  public listLogLineFiltered$ = new BehaviorSubject<LogLine[]>([]);

  // input
  public currentLogLineId$ = new BehaviorSubject<number | null>(null);
  // depended listLogLineFiltered$ currentLogLineId$
  public currentLogLine$ = new BehaviorSubject<LogLine | null>(null);
  public currentLogTimestamp$ = new BehaviorSubject<(ZonedDateTime | null)>(null);

  // input
  public listLogLineIdHighlighted$ = new BehaviorSubject<Set<string>>(new Set<string>());

  // depended listLogLineFiltered$
  public startComplete$ = new BehaviorSubject<ZonedDateTime>(epoch);
  // depended listLogLineFiltered$
  public finishComplete$ = new BehaviorSubject<ZonedDateTime>(epoch);

  // input
  public modeZoom$ = new BehaviorSubject<'complete'>('complete');
  // depended modeZoom$ listLogLineFiltered$
  public startZoom$ = new BehaviorSubject<ZonedDateTime>(epoch);
  // depended modeZoom$ listLogLineFiltered$
  public finishZoom$ = new BehaviorSubject<ZonedDateTime>(epoch);
  // depended startZoom$ finishZoom$
  public durationZoom$ = new BehaviorSubject<Duration>(Duration.ofSeconds(0));

  public startFilter$ = new BehaviorSubject<ZonedDateTime>(epoch);
  public finishFilter$ = new BehaviorSubject<ZonedDateTime>(epoch);

  public startHighlight$ = new BehaviorSubject<ZonedDateTime>(epoch);
  public finishHighlight$ = new BehaviorSubject<ZonedDateTime>(epoch);

  public startCurrent$ = new BehaviorSubject<ZonedDateTime | null>(null);
  public finishCurrent$ = new BehaviorSubject<ZonedDateTime | null>(null);

  subscription = new Subscription();

  constructor() {
    this.subscription.add(
      this.listLogLineFiltered$
        .subscribe({
          next: (value) => {
            let first: ZonedDateTime
            if (value.length < 2) {
              this.startComplete$.next(epoch);
              this.finishComplete$.next(epoch);
            } else {
              const first = getLogLineTimestampValue(value[0]) ?? ZonedDateTime.now();
              const last = getLogLineTimestampValue(value[value.length - 1]) ?? first.plusSeconds(1);
              this.startComplete$.next(first);
              this.finishComplete$.next(last);
            }
          }
        })
    );

    this.subscription.add(
      combineLatest({
        modeZoom: this.modeZoom$,
        startComplete: this.startComplete$,
        finishComplete: this.finishComplete$,
      }).subscribe({
        next: (value) => {
          if ('complete' === value.modeZoom) {
            this.startZoom$.next(value.startComplete);
            this.finishZoom$.next(value.finishComplete);
          }
        }
      })
    );

    this.subscription.add(
      combineLatest({
        modeZoom: this.modeZoom$,
        startZoom: this.startZoom$,
        finishZoom: this.finishZoom$,
      }).subscribe({
        next: (value) => {
          const duration = Duration.between(value.startZoom, value.finishZoom);
          this.durationZoom$.next(duration);
        }
      })
    );

    this.subscription.add(
      combineLatest({
        listLogLineFiltered: this.listLogLineFiltered$,
        currentLogLineId: this.currentLogLineId$,
      }).subscribe({
        next: (value) => {
          const currentLogLine = ((null === value.currentLogLineId) || (undefined === value.currentLogLineId))
            ? undefined
            : value.listLogLineFiltered.find(item => item.id === value.currentLogLineId);

          console.log("LogTimeDataService.currentLogLineId-currentLogLine", currentLogLine);

          if (undefined === currentLogLine) {
            this.currentLogLine$.next(null);
            this.currentLogTimestamp$.next(null);
          } else {
            this.currentLogLine$.next(currentLogLine);
            const ts=getLogLineTimestampValue(currentLogLine);
            this.currentLogTimestamp$.next(ts);
            this.startCurrent$.next(ts);
          }
        }
      })
    );

  }
}
