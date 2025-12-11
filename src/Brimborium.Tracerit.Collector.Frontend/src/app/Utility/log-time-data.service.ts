import { inject, Injectable } from '@angular/core';
import { combineLatest, distinctUntilChanged, filter, map, Subscription, switchMap, tap } from 'rxjs';
import { filterListLogLine, getLogLineTimestampValue, LogLine, PropertyHeader } from '../Api';
import { Duration, ZonedDateTime } from '@js-joda/core';
import { epoch0, epoch1, getEffectiveRange, setTimeRangeDurationIfChanged, setTimeRangeIfChanged, setTimeRangeOrNullIfChanged, TimeRange, TimeRangeDuration, TimeRangeOrNull } from './time-range';
import { debounceToggle } from './debounceToggle';
import { BehaviorRingSubject } from './BehaviorRingSubject';
import { MasterRingService } from './master-ring.service';
import { combineLatestRingSubject } from './CombineLatestRingSubject';
import { DataService } from './data-service';
import { createObserableSubscripe } from './ObservableSubscripe';
import { combineLatestSubject } from './CombineLatestSubject';

@Injectable({
  providedIn: 'root',
})
export class LogTimeDataService {
  readonly subscription = new Subscription();
  readonly ring$ = inject(MasterRingService).dependendRing('LogTimeDataService-ring$', this.subscription);
  readonly dataService = inject(DataService);

  readonly useCurrentStream$ = new BehaviorRingSubject<boolean>(false, 1, 'LogTimeDataService_useCurrentStream$', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value); });

  // input
  readonly listLogLineCurrentStream$ = new BehaviorRingSubject<LogLine[]>([],
    0, 'LogTimeDataService_listLogLineCurrentStream', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });

  readonly listLogLineFiles$ = new BehaviorRingSubject<LogLine[]>([],
    0, 'LogTimeDataService_listLogLineFiles', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });

  readonly listLogLineFilesSubscripe = createObserableSubscripe({
    obs: this.dataService.listLogLineSource$.pipe(
      tap({
        next: (value) => {
          this.listLogLineFiles$.next(value);
        }
      })
    ),
    subscribtion: this.subscription,
    immediate: true
  });

  readonly listLogLineInputSources$ = combineLatestSubject({
    dictObservable: {
      useCurrentStream: this.useCurrentStream$,
      listLogLineCurrentStream: this.listLogLineCurrentStream$,
      listLogLineFiles: this.listLogLineFiles$,
    },
    name: 'LogTimeDataService_listLogLineInputSources$'
  });

  readonly listLogLineAll$ = new BehaviorRingSubject<LogLine[]>([],
    0, 'LogTimeDataService_listLogLineAll', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });

  readonly listLogLineSubscripe = createObserableSubscripe({
    obs: this.useCurrentStream$.pipe(
      switchMap(value => value ? this.listLogLineCurrentStream$ : this.listLogLineFiles$)
    ).pipe(
      tap({
        next: (value) => {
          this.listLogLineAll$.next(value);
        }
      })
    ),
    subscribtion: this.subscription,
    immediate: true
  });

  readonly listAllHeader$ = new BehaviorRingSubject<PropertyHeader[]>([],
    0, 'LogTimeDataService_listAllHeader', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });

  // filter
  // input
  readonly listFilterCondition$ = new BehaviorRingSubject<PropertyHeader[]>([],
    0, 'LogTimeDataService_listFilterCondition', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });

  // calculated output depended on listLogLine$ and listFilterCondition$
  readonly listLogLineFilteredCondition$ = new BehaviorRingSubject<LogLine[]>([],
    0, 'LogTimeDataService_listLogLineFilteredCondition', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });

  readonly listLogLineTimeZoomed$ = new BehaviorRingSubject<LogLine[]>([],
    0, 'LogTimeDataService_listLogLineTimeZoomed', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });
    
  // calculated output depended on listLogLineFilteredCondition$ and rangeFilter$
  readonly listLogLineFilteredTime$ = new BehaviorRingSubject<LogLine[]>([],
    0, 'LogTimeDataService_listLogLineFilteredTime', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });

  // input
  readonly currentLogLineId$ = new BehaviorRingSubject<number | null>(null,
    0, 'currentLogLineId$', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value); });
  // depended listLogLineFiltered$ currentLogLineId$
  readonly currentLogLine$ = new BehaviorRingSubject<LogLine | null>(null,
    0, 'LogTimeDataService_currentLogLine$', this.subscription, this.ring$, undefined, BehaviorRingSubject.defaultLog);

  readonly currentLogTimestamp$ = new BehaviorRingSubject<(ZonedDateTime | null)>(null,
    0, 'LogTimeDataService_currentLogTimestamp$', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.toString()); });

  // input
  readonly listLogLineIdHighlighted$ = new BehaviorRingSubject<Set<string>>(new Set<string>(), 1, 'listLogLineIdHighlighted$', this.subscription, this.ring$, undefined, BehaviorRingSubject.defaultLog);

  // depended listLogLineFiltered$
  readonly rangeComplete$ = new BehaviorRingSubject<TimeRange>(
    Object.freeze({
      start: epoch0,
      finish: epoch1,
    }),
    0, 'LogTimeDataService_rangeComplete$', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, { start: value?.start?.toString(), finish: value?.finish?.toString() }); });

  // input
  readonly modeZoom$ = new BehaviorRingSubject<'complete'>('complete', 1, 'modeZoom$', this.subscription, this.ring$, undefined,
    (name, message, value) => {
      console.log(name, message, value);
    });

  // depended modeZoom$ listLogLineFiltered$

  readonly rangeZoom$ = new BehaviorRingSubject<TimeRangeDuration>(Object.freeze({
    start: epoch0,
    finish: epoch1,
    duration: Duration.between(epoch0, epoch1)
  }),
    0, 'LogTimeDataService_rangeZoom',
    this.subscription, this.ring$, undefined,
    (name, message, value) => {
      console.log(name, message, { start: value?.start?.toString(), finish: value?.finish?.toString(), duration: value?.duration?.toString() });
    }
  );

  readonly rangeFilter$ = new BehaviorRingSubject<TimeRange>(Object.freeze({
    start: epoch0,
    finish: epoch1,
  }),
    0, 'LogTimeDataService_rangeFilter', this.subscription, this.ring$, undefined,
    (name, message, value) => {
      console.log(name, message, { start: value?.start?.toString(), finish: value?.finish?.toString() });
    }
  );

  readonly rangeCurrentSelected$ = new BehaviorRingSubject<TimeRangeOrNull>(Object.freeze({ start: null, finish: null }),
    0, 'LogTimeDataService_rangeCurrentSelected',
    this.subscription, this.ring$, undefined,
    (name, message, value) => {
      console.log(name, message, { start: value?.start?.toString(), finish: value?.finish?.toString() });
    }
  );

  constructor() {
      //switchMap(value => value ? this.listLogLineCurrentStream$ : this.listLogLineFiles$)
    this.subscription.add(
      this.listLogLineAll$.subscribe({
        next: (listLogLine) => {
          const rangeComplete = calcStartFinish(listLogLine)
          setTimeRangeIfChanged(this.rangeComplete$, rangeComplete);
          setTimeRangeDurationIfChanged(this.rangeZoom$, rangeComplete);
          setTimeRangeDurationIfChanged(this.rangeZoom$, rangeComplete);
          /*
          TODO
          this.useCurrentStream$.getValue()
          const nextRangeZoom = getEffectiveRange([rangeComplete, this.rangeZoom$.getValue()]);
          setTimeRangeDurationIfChanged(this.rangeZoom$, nextRangeZoom);

          const nextRangeFilter = getEffectiveRange([nextRangeZoom, this.rangeFilter$.getValue()]);
          setTimeRangeIfChanged(this.rangeFilter$, nextRangeFilter);
          console.log("rangeComplete", rangeComplete.start.toString(), rangeComplete.finish.toString());
          console.log("rangeZoom", nextRangeZoom.start.toString(), nextRangeZoom.finish.toString());
          console.log("rangeFilter", nextRangeFilter.start.toString(), nextRangeFilter.finish.toString());
          */
        }
      }));

    this.subscription.add(
      combineLatest({
        listLogLine: this.listLogLineAll$,
        rangeZoom: this.rangeZoom$
      }).pipe(
        map(value => {
          const filterStart = (value.rangeZoom.start ?? epoch0).isEqual(epoch0) ;
          const filterFinish = (value.rangeZoom.finish ?? epoch1).isEqual(epoch1);

          if (filterStart && filterFinish) {
            return value.listLogLine;
          }

          return value.listLogLine.filter(item => {
            const ts = getLogLineTimestampValue(item);
            if (ts === null) { return false; }            
            return (filterStart ? true : (value.rangeZoom.start.compareTo(ts) <= 0) )
              && (filterFinish ? true : (ts.compareTo(value.rangeZoom.finish) <= 0) );
          });
        })
      ).subscribe({
        next: (value) => {
          this.listLogLineTimeZoomed$.next(value);
        }
      })
    );
    this.subscription.add(
      combineLatest({
        listLogLine: this.listLogLineTimeZoomed$,
        rangeFilter: this.rangeFilter$
      }).pipe(
        map(value => {
          const filterStart = (value.rangeFilter.start ?? epoch0).isEqual(epoch0) ;
          const filterFinish = (value.rangeFilter.finish ?? epoch1).isEqual(epoch1);

          if (filterStart && filterFinish) {
            return value.listLogLine;
          }
          
          return value.listLogLine.filter(item => {
            const ts = getLogLineTimestampValue(item);
            if (ts === null) { return false; }            
            return (filterStart ? true : (value.rangeFilter.start.compareTo(ts) <= 0) )
              && (filterFinish ? true : (ts.compareTo(value.rangeFilter.finish) <= 0) );
          });
        })
      ).subscribe({
        next: (value) => {
          this.listLogLineFilteredTime$.next(value);
        }
      })
    );

    this.subscription.add(
      combineLatest({
        listFilterCondition: this.listFilterCondition$,
        listLogLineFilteredTime: this.listLogLineFilteredTime$
      }).subscribe({
        next: (value) => {
          const result = filterListLogLine(
            value.listLogLineFilteredTime, 
            value.listFilterCondition);
          this.listLogLineFilteredCondition$.next(result);
        }
      })
    );

    // this.subscription.add(
    //   combineLatest({
    //     modeZoom: this.modeZoom$,
    //     rangeComplete: this.rangeComplete$
    //   }).pipe(
    //     distinctUntilChanged((a, b) => {
    //       return a.modeZoom === b.modeZoom
    //         && a.rangeComplete.start.isEqual(b.rangeComplete.start)
    //         && a.rangeComplete.finish.isEqual(b.rangeComplete.finish);
    //     }),
    //     debounceToggle(this.ring$.pipe(map(value => 0 < value)))
    //   ).subscribe({
    //     next: (value) => {
    //       if ('complete' === value.modeZoom) {
    //         console.log("LogTimeDataService.modeZoom-complete-startZoom",
    //           {
    //             modeZoom: value.modeZoom,
    //             startComplete: value.rangeComplete.start.toString(),
    //             finishComplete: value.rangeComplete.finish.toString(),
    //           });
    //         setTimeRangeDurationIfChanged(this.rangeZoom$, {
    //           start: value.rangeComplete.start,
    //           finish: value.rangeComplete.finish,
    //         });
    //       }
    //     }
    //   })
    // );

    this.subscription.add(
      combineLatest({
        listLogLine: this.listLogLineAll$,
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
            setTimeRangeOrNullIfChanged(this.rangeCurrentSelected$, { start: ts, finish: null });
          }
        }
      })
    );
  }

  setRangeFilter(range: TimeRangeOrNull) {
    const rangeNormalized = {
      start: range.start ?? epoch0,
      finish: range.finish ?? epoch1
    };
    const rangeComplete = this.rangeComplete$.getValue();
    const nextRange = getEffectiveRange([rangeComplete, rangeNormalized]);
    /*
    console.log("setRangeFilter-rangeComplete", rangeComplete.start.toString(), rangeComplete.finish.toString());
    console.log("setRangeFilter-rangeNormalized", rangeNormalized.start.toString(), rangeNormalized.finish.toString());
    console.log("setRangeFilter-nextRange", nextRange.start.toString(), nextRange.finish.toString());
    */
    this.rangeFilter$.next(nextRange);
  }

}

function calcStartFinish(value: LogLine[]) {
  if (0 === value.length) {
    return <TimeRange>({
      start: epoch0,
      finish: epoch1,
    });
  } else {

    let start: ZonedDateTime | null = null;
    let finish: ZonedDateTime | null = null;

    for (const item of value) {
      const ts = getLogLineTimestampValue(item);
      if (ts === null) { continue; }
      if (start === null || ts.isBefore(start)) {
        start = ts;
      }
      if (finish === null || ts.isAfter(finish)) {
        finish = ts;
      }
    }
    return <TimeRange>({
      start: start ?? epoch0,
      finish: finish ?? epoch1,
    });
  }

}

