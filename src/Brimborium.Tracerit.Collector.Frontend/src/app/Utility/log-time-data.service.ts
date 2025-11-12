import { inject, Inject, Injectable } from '@angular/core';
import { BehaviorSubject, combineLatest, debounceTime, delay, distinctUntilChanged, map, pipe, Subscription } from 'rxjs';
import { filterListLogLine, getLogLineTimestampValue, LogLine, PropertyHeader } from '../Api';
import { Duration, ZonedDateTime, ZoneId } from '@js-joda/core';
import { epoch0, epoch1, setTimeRangeDurationIfChanged, setTimeRangeIfChanged, setTimeRangeOrNullIfChanged, setTimeRangeOrNullStartIfChanged, TimeRange, TimeRangeDuration, TimeRangeOrNull } from './time-range';
import { debounceToggle } from './debounceToggle';
import { BehaviorRingSubject } from './BehaviorRingSubject';
import { MasterRingSubject } from "./MasterRingSubject";
import { MasterRingService } from './master-ring.service';
import { AppRingOrder } from '../app-ring-order';
import { combineLatestRingSubject } from './CombineLatestRingSubject';
import { DataService } from './data-service';

@Injectable({
  providedIn: 'root',
})
export class LogTimeDataService {
  readonly subscription = new Subscription();
  readonly ring$ = inject(MasterRingService).dependendRing('LogTimeDataService-ring$', this.subscription);
  readonly dataService = inject(DataService);

  // input
  readonly listLogLine$ = new BehaviorRingSubject<LogLine[]>([], 
    AppRingOrder.LogTimeDataService_listLogLine, 'LogTimeDataService_listLogLine', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });


  readonly listAllHeader$ = new BehaviorRingSubject<PropertyHeader[]>([], 
    AppRingOrder.LogTimeDataService_listAllHeader, 'LogTimeDataService_listAllHeader', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });
      

  // filter
  // input
  readonly listFilterCondition$ = new BehaviorRingSubject<PropertyHeader[]>([], 
    AppRingOrder.LogTimeDataService_listFilterCondition, 'LogTimeDataService_listFilterCondition', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });

  // calculated output depended on listLogLine$ and listFilterCondition$
  readonly listLogLineFilteredCondition$ = new BehaviorRingSubject<LogLine[]>([], 
    AppRingOrder.LogTimeDataService_listLogLineFilteredCondition,  'LogTimeDataService_listLogLineFilteredCondition', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });

  // input
  readonly listLogLineFilteredTime$ = new BehaviorRingSubject<LogLine[]>([], 
    AppRingOrder.LogTimeDataService_listLogLineFilteredTime,  'LogTimeDataService_listLogLineFilteredTime', this.subscription, this.ring$, undefined,    
    (name, message, value) => { console.log(name, message, value?.length); });

  // input
  readonly currentLogLineId$ = new BehaviorRingSubject<number | null>(null, 
    AppRingOrder.LogTimeDataService_currentLogLineId, 'currentLogLineId$', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value); });
  // depended listLogLineFiltered$ currentLogLineId$
  readonly currentLogLine$ = new BehaviorRingSubject<LogLine | null>(null, 
    AppRingOrder.LogTimeDataService_currentLogLine, 'currentLogLine$', this.subscription, this.ring$, undefined, BehaviorRingSubject.defaultLog);

  readonly currentLogTimestamp$ = new BehaviorRingSubject<(ZonedDateTime | null)>(null, 
    AppRingOrder.LogTimeDataService_currentLogTimestamp, 'currentLogTimestamp$', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.toString()); });

  // input
  readonly listLogLineIdHighlighted$ = new BehaviorRingSubject<Set<string>>(new Set<string>(), 1, 'listLogLineIdHighlighted$', this.subscription, this.ring$, undefined, BehaviorRingSubject.defaultLog);

  // depended listLogLineFiltered$
  readonly rangeComplete$ = new BehaviorRingSubject<TimeRange>(
    Object.freeze({
      start: epoch0,
      finish: epoch1,
    }),
    AppRingOrder.LogTimeDataService_rangeComplete, 'rangeComplete$', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, { start: value?.start?.toString(), finish: value?.finish?.toString() }); });

  // input
  readonly modeZoom$ = new BehaviorRingSubject<'complete'>('complete', 1, 'modeZoom$', this.subscription, this.ring$, undefined,
    (name, message, value) => {
      console.log(name, message, value);
    });

  // depended modeZoom$ listLogLineFiltered$
  //public startZoom$ = new BehaviorRingSubject<ZonedDateTime>(epoch0);
  // depended modeZoom$ listLogLineFiltered$
  //public finishZoom$ = new BehaviorRingSubject<ZonedDateTime>(epoch1);
  // depended startZoom$ finishZoom$
  //public durationZoom$ = new BehaviorRingSubject<Duration>(Duration.ofSeconds(0));

  readonly rangeZoom$ = new BehaviorRingSubject<TimeRangeDuration>(Object.freeze({
    start: epoch0,
    finish: epoch1,
    duration: Duration.between(epoch0, epoch1)
  }),
    AppRingOrder.LogTimeDataService_rangeZoom, 'LogTimeDataService_rangeZoom',
    this.subscription, this.ring$, undefined,
    (name, message, value) => {
      console.log(name, message, { start: value?.start?.toString(), finish: value?.finish?.toString(), duration: value?.duration?.toString() });
    }
  );

  readonly rangeFilter$ = new BehaviorRingSubject<TimeRange>(Object.freeze({
    start: epoch0,
    finish: epoch1,
  }),
    AppRingOrder.LogTimeDataService_rangeFilter, 'LogTimeDataService_rangeFilter', this.subscription, this.ring$, undefined,
    (name, message, value) => {
      console.log(name, message, { start: value?.start?.toString(), finish: value?.finish?.toString() });
    }
  );

  readonly rangeCurrent$ = new BehaviorRingSubject<TimeRangeOrNull>(Object.freeze({ start: null, finish: null }),
    AppRingOrder.LogTimeDataService_rangeCurrent, 'LogTimeDataService_rangeCurrent',
    this.subscription, this.ring$, undefined,
    (name, message, value) => {
      console.log(name, message, { start: value?.start?.toString(), finish: value?.finish?.toString() });
    }
  );

  constructor() {

     this.subscription.add(
      this.dataService.listLogLine$.pipeAndSubscribe(this.listLogLine$, (subject) => subject));

    this.subscription.add(
      combineLatestRingSubject(
        {
          filter: this.listFilterCondition$,
          listLogLine: this.listLogLine$,
          listHeader: this.listAllHeader$,
        }, 
        AppRingOrder.LogTimeDataService_listLogLineFilteredCondition_calculate,
        'DataService-FilteredCondition', this.ring$
      ).pipeAndSubscribe(
        this.listLogLineFilteredCondition$,
        (subject) => subject.pipe(
          map(value => {
            return filterListLogLine(value.listLogLine, value.listHeader);
          })
        )
      ));

    this.listLogLine$.pipeAndSubscribe(
      this.rangeComplete$,
      (subject) => subject.pipe(
        map(value => {
          if (0 === value.length) {
            return ({
              start: epoch0,
              finish: epoch1,
            });
          } else {
            const first = getLogLineTimestampValue(value[0]);
            const last = getLogLineTimestampValue(value[value.length - 1]);
            if (first === null || last === null) { return undefined; }
            return ({
              start: first,
              finish: last,
            });
          }
        })
      ),
      (value, target) => setTimeRangeIfChanged(target, value)
    );

    // this.subscription.add(
    //   this.listLogLine$
    //     .subscribe({
    //       next: (value) => {
    //         if (0 === value.length) {
    //           setTimeRangeIfChanged(this.rangeComplete$, {
    //             start: epoch0,
    //             finish: epoch1,
    //           });
    //         } else {
    //           const rangeComplete = this.rangeComplete$.getValue();
    //           const rangeCompleteIsEpoch01 = rangeComplete.start.isEqual(epoch0) && rangeComplete.finish.isEqual(epoch1);
    //           if (rangeCompleteIsEpoch01) {
    //             const first = getLogLineTimestampValue(value[0]) ?? ZonedDateTime.now();
    //             const last = getLogLineTimestampValue(value[value.length - 1]) ?? first.plusSeconds(1);
    //             setTimeRangeIfChanged(this.rangeComplete$, {
    //               start: first,
    //               finish: last,
    //             });
    //           }
    //         }
    //       }
    //     })
    // );

    this.subscription.add(
      combineLatest({
        modeZoom: this.modeZoom$,
        rangeComplete: this.rangeComplete$
      }).pipe(
        distinctUntilChanged((a, b) => {
          return a.modeZoom === b.modeZoom
            && a.rangeComplete.start.isEqual(b.rangeComplete.start)
            && a.rangeComplete.finish.isEqual(b.rangeComplete.finish);
        }),
        debounceToggle(this.ring$.pipe(map(value => 0 < value)))
      ).subscribe({
        next: (value) => {
          if ('complete' === value.modeZoom) {
            console.log("LogTimeDataService.modeZoom-complete-startZoom",
              {
                modeZoom: value.modeZoom,
                startComplete: value.rangeComplete.start.toString(),
                finishComplete: value.rangeComplete.finish.toString(),
              });
            setTimeRangeDurationIfChanged(this.rangeZoom$, {
              start: value.rangeComplete.start,
              finish: value.rangeComplete.finish,
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
        debounceToggle(this.ring$.pipe(map(value => 0 < value)))
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
            setTimeRangeOrNullIfChanged(this.rangeCurrent$, { start: ts, finish: null });
          }
        }
      })
    );
  }
}
