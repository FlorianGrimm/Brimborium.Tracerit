import { inject, Injectable } from '@angular/core';
import { combineLatest, distinctUntilChanged, filter, map, Subscription, switchMap, tap } from 'rxjs';
import { filterListLogLine, getLogLineTimestampValue, LogLine, PropertyHeader } from '../Api';
import { Duration, ZonedDateTime } from '@js-joda/core';
import { epoch0, epoch1, setTimeRangeDurationIfChanged, setTimeRangeIfChanged, setTimeRangeOrNullIfChanged, TimeRange, TimeRangeDuration, TimeRangeOrNull } from './time-range';
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
    obs: this.dataService.listLogLine$.pipe(
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

  readonly listLogLine$ = new BehaviorRingSubject<LogLine[]>([],
    0, 'LogTimeDataService_listLogLine', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });

  readonly listLogLineSubscripe = createObserableSubscripe({
    obs: this.useCurrentStream$.pipe(
      switchMap(value => value ? this.listLogLineCurrentStream$ : this.listLogLineFiles$)
    ).pipe(
      tap({
        next: (value) => {
          this.listLogLine$.next(value);
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

    this.subscription.add(
      this.dataService.listLogLine$.subscribe({
        next: (value) => {
          this.listLogLine$.next(value);
        }
      }));


    combineLatestRingSubject(
      {
        filter: this.listFilterCondition$,
        listLogLine: this.listLogLine$,
        // listHeader: this.listAllHeader$,
      },
      0,
      'DataService-FilteredCondition', this.ring$
    ).pipeAndSubscribe(
      this.subscription,
      this.listLogLineFilteredCondition$,
      (obs) => obs.pipe(
        map(value => {
          return filterListLogLine(value.listLogLine, value.filter);
        })
      )
    );

    this.subscription.add(
      this.listLogLine$
        .pipe(
          map(value => {
            if (0 === value.length) {
              return <TimeRange>({
                start: epoch0,
                finish: epoch1,
              });
            } else {
              const first = getLogLineTimestampValue(value[0]);
              const last = getLogLineTimestampValue(value[value.length - 1]);
              if (first === null || last === null) { return undefined; }
              return <TimeRange>({
                start: first,
                finish: last,
              });
            }
          })
        ).subscribe({
          next: (value) => {
            if (undefined === value) { return; }
            setTimeRangeIfChanged(this.rangeComplete$, value);
          }
        }));

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
            setTimeRangeOrNullIfChanged(this.rangeCurrentSelected$, { start: ts, finish: null });
          }
        }
      })
    );

    this.subscription.add(
      combineLatest({
        listLogLineFilteredCondition: this.listLogLineFilteredCondition$,
        rangeFilter: this.rangeFilter$,
      }).pipe(
        debounceToggle(this.ring$.pipe(map(value => 0 < value)))
      ).subscribe({
        next: (value) => {
          const testStart = (epoch0.compareTo(value.rangeFilter.start) !== 0);
          const testFinish = (epoch1.compareTo(value.rangeFilter.finish) !== 0);

          const result = value.listLogLineFilteredCondition.filter(
            (item) => {
              const ts = getLogLineTimestampValue(item);
              if (ts === null) { return false; }
              return (testStart ? (value.rangeFilter.start.compareTo(ts) <= 0) : true)
                && (testFinish ? (ts.compareTo(value.rangeFilter.finish) <= 0) : true);
            });
          this.listLogLineFilteredTime$.next(result);
        }
      })
    );
  }
}
