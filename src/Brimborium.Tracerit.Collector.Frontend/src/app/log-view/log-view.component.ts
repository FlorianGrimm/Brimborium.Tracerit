import { Component, inject, OnDestroy } from '@angular/core';
import { BehaviorSubject, combineLatest, delay, filter, map, Subscription, tap } from 'rxjs';
import { DataService } from '../Utility/data-service';
import { HttpClientService } from '../Utility/http-client-service';
import { filterListLogLine, getLogLineTimestampValue, LogLine, PropertyHeader } from '../Api';
import { AsyncPipe, JsonPipe, KeyValuePipe } from '@angular/common';
import { getVisualHeader } from '../Utility/propertyHeaderUtility';
import { LucideAngularModule, FileStack, ChevronLeft, ChevronRight, Funnel, FunnelPlus, FunnelX } from 'lucide-angular';
import { RouterLink } from '@angular/router';
import { Duration, ZonedDateTime } from '@js-joda/core';
import { TimeRulerComponent } from "../time-ruler/time-ruler.component";
import { LogTimeDataService } from '../Utility/log-time-data.service';
import { BehaviorRingSubject } from '../Utility/BehaviorRingSubject';
import { MasterRingService } from '../Utility/master-ring.service';
import { combineLatestSubject } from '../Utility/CombineLatestSubject';
import { createObserableSubject } from '../Utility/ObserableSubject';
import { createObserableSubscripe } from '../Utility/ObservableSubscripe';
import { CdkDrag, CdkDragDrop, CdkDropList, moveItemInArray } from '@angular/cdk/drag-drop';
import { epoch0, epoch1 } from '../Utility/time-range';

@Component({
  selector: 'app-log-view',
  imports: [
    AsyncPipe,
    CdkDropList,
    CdkDrag,
    RouterLink,
    LucideAngularModule,
    TimeRulerComponent
  ],
  templateUrl: './log-view.component.html',
  styleUrl: './log-view.component.scss',
})
export class LogViewComponent implements OnDestroy {
  resizeScreenX: number = 0;
  resizeClientWidth: number = 0;
  resizeHeaderId: string = '';
  overlay: HTMLDivElement | undefined = undefined;
  private boundResizeMouseMove = this.onResizeMouseMove.bind(this);
  private boundResizeMouseUp = this.onResizeMouseUp.bind(this);

  mouseDownResize(e: MouseEvent, headerId: string) {
    //e.preventDefault();
    const allHeader = this.listAllHeader$.getValue();
    const header = allHeader.find((item) => (headerId === item.id));
    if (undefined === header) { return; }

    this.resizeHeaderId = headerId;
    this.resizeScreenX = e.screenX;
    const elementHeader = window.document.getElementById("header-" + headerId);
    if (elementHeader == null) { return; }
    this.resizeClientWidth = elementHeader.clientWidth;

    // Add document-level listeners to track mouse outside the element
    const overlay = document.createElement('div');
    this.overlay = overlay;
    overlay.style.position = 'absolute';
    overlay.style.top = '0';
    overlay.style.left = '0';
    overlay.style.width = '100%';
    overlay.style.height = '100%';
    overlay.style.zIndex = '500';
    overlay.addEventListener('mousemove', this.boundResizeMouseMove);
    overlay.addEventListener('mouseup', this.boundResizeMouseUp);
    overlay.style.cursor = 'col-resize';
    overlay.style.userSelect = 'none';
    window.document.body.appendChild(overlay);
    console.log("mouseDownResize", headerId, e);
  }

  private onResizeMouseMove(e: MouseEvent) {
    if (0 === this.resizeScreenX || !this.resizeHeaderId) { return; }
    const allHeader = this.listAllHeader$.getValue();
    const header = allHeader.find((item) => (this.resizeHeaderId === item.id));
    if (undefined === header) { return; }
    const diff = e.screenX - this.resizeScreenX;
    const newWidth = Math.max(50, this.resizeClientWidth + diff); // Minimum width of 50px
    header.headerCellStyle = { 'width': `${newWidth}px` };
    header.dataCellStyle = { 'width': `${newWidth}px` };
    console.log("mouseMoveResize", this.resizeHeaderId, diff, e);
  }

  private onResizeMouseUp(e: MouseEvent) {
    if (this.resizeHeaderId) {
      const allHeader = this.listAllHeader$.getValue();
      const header = allHeader.find((item) => (this.resizeHeaderId === item.id));
      if (header) {
        const diff = e.screenX - this.resizeScreenX;
        const newWidth = Math.max(50, this.resizeClientWidth + diff);
        header.headerCellStyle = { 'width': `${newWidth}px` };
        header.dataCellStyle = { 'width': `${newWidth}px` };
        console.log("mouseUpResize", this.resizeHeaderId, diff, e);
      }
    }

    // Clean up
    const overlay = this.overlay;
    this.overlay = undefined;
    if (overlay != null) {
      window.document.body.removeChild(overlay);
      overlay.removeEventListener('mousemove', this.boundResizeMouseMove);
      overlay.removeEventListener('mouseup', this.boundResizeMouseUp);
      overlay.style.cursor = '';
      overlay.style.userSelect = '';
    }
    this.resizeScreenX = 0;
    this.resizeClientWidth = 0;
    this.resizeHeaderId = '';
  }

  // Keep these for backward compatibility, but they're no longer needed
  mouseMoveResize(_e: MouseEvent, _headerId: string) { }
  mouseUpResize(_e: MouseEvent, _headerId: string) { }
  readonly FileStack = FileStack;
  readonly ChevronLeft = ChevronLeft;
  readonly ChevronRight = ChevronRight;
  readonly Funnel = Funnel;
  readonly FunnelPlus = FunnelPlus;
  readonly FunnelX = FunnelX;

  readonly subscription = new Subscription();
  readonly ring$ = inject(MasterRingService).dependendRing('LogTimeDataService-ring$', this.subscription);
  readonly dataService = inject(DataService);
  readonly httpClientService = inject(HttpClientService);
  readonly logTimeDataService = inject(LogTimeDataService);

  readonly listAllHeader$ = new BehaviorRingSubject<PropertyHeader[]>([],
    0, 'LogViewComponent_listAllHeader', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });
  readonly listCurrentHeader$ = new BehaviorRingSubject<PropertyHeader[]>([],
    0, 'LogViewComponent_listCurrentHeader', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });
  readonly listAllHeaderSubscripe = createObserableSubscripe({
    obs: this.dataService.listAllHeader$.pipe(
      tap({
        next: (value) => {
          const nextValue = value.slice();
          this.listAllHeader$.next(nextValue);
          this.listCurrentHeader$.next(getVisualHeader(nextValue));
        }
      })
    ),
    subscribtion: this.subscription,
    immediate: true
  });

  readonly listLogLine$ = new BehaviorRingSubject<LogLine[]>([],
    0, 'LogViewComponent_listLogLine', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });
  readonly listLogLineSubscripe = createObserableSubscripe({
    obs: this.dataService.listLogLine$.pipe(
      tap({
        next: (value) => {
          this.listLogLine$.next(value.slice());
        }
      })
    ),
    subscribtion: this.subscription,
    immediate: true
  });

  readonly listLogLineFilteredCondition$ = new BehaviorRingSubject<LogLine[]>([],
    0, 'LogViewComponent_listLogLineFilteredCondition', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });

  readonly listLogLineFilteredConditionSubscripe = createObserableSubscripe({
    obs: this.logTimeDataService.listLogLineFilteredCondition$.pipe(
      tap({
        next: (value) => {
          this.listLogLineFilteredCondition$.next(value.slice());
        }
      })
    ),
    subscribtion: this.subscription,
    immediate: true
  });

  readonly listLogLineFilteredTime$ = new BehaviorRingSubject<LogLine[]>([],
    0, 'LogViewComponent_listLogLineFilteredTime', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });
  readonly listLogLineFilteredTimeSubscripe = createObserableSubscripe({
    obs:
      combineLatest({
        listLogLineFilteredCondition: this.listLogLineFilteredCondition$,
        rangeFilter: this.logTimeDataService.rangeFilter$,
      }).pipe(
        tap({
          next: (value) => {
            const testStart = (epoch0.compareTo(value.rangeFilter.start) !== 0);
            const testFinish = (epoch1.compareTo(value.rangeFilter.finish) !== 0);
            const resultFilteredTime = value.listLogLineFilteredCondition.filter(
              (item) => {
                const ts = getLogLineTimestampValue(item);
                if (ts === null) { return false; }
                return (testStart ? (value.rangeFilter.start.compareTo(ts) <= 0) : true)
                  && (testFinish ? (ts.compareTo(value.rangeFilter.finish) <= 0) : true);
              });
            // console.log("filter start", value.rangeFilter.start.toString(), "finish", value.rangeFilter.finish.toString(), resultFilteredTime.length);
            this.listLogLineFilteredTime$.next(resultFilteredTime);
          }
        })
      ),
    subscribtion: this.subscription,
    immediate: true
  });

  readonly filter$ = new BehaviorRingSubject<number>(1, 0, 'LogViewComponent_filter$', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value); });

  readonly currentLogLineId$ = new BehaviorRingSubject<number | null>(null,
    0, 'LogViewComponent_currentLogLineId', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value); });
  readonly currentLogLine$ = new BehaviorRingSubject<LogLine | null>(null,
    0, 'LogViewComponent_currentLogLine', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value); });
  readonly currentLogTimestamp$ = new BehaviorRingSubject<(ZonedDateTime | null)>(null,
    0, 'LogViewComponent_currentLogTimestamp', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value); });

  readonly contextLogLineId$ = new BehaviorRingSubject<number | null>(null,
    0, 'LogViewComponent_contextLogLineId', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value); });
  readonly contextLogLine$ = new BehaviorRingSubject<LogLine | null>(null,
    0, 'LogViewComponent_contextLogLine', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value); });

  readonly error$ = new BehaviorRingSubject<null | string>(null,
    0, 'LogViewComponent_error$', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value); });

  readonly triggerFilter$ = combineLatestSubject(
    {
      dictObservable: {
        filter: this.filter$,
        listCurrentHeader: this.listCurrentHeader$
      },
      name: 'LogViewComponent_triggerFilter$'
    }
  );

  copyToLogTimeDataService = createObserableSubscripe({
    obs: this.triggerFilter$.combineLatest().pipe(
      map(value => value.listCurrentHeader.slice()),
      tap({
        next: (value) => {
          this.logTimeDataService.listFilterCondition$.next(value);
        }
      })
    ),
    subscribtion: this.subscription,
    immediate: true
  });

  constructor() {

    this.subscription.add(
      this.dataService.listAllHeader$.subscribe({
        next: (value) => {
          const nextValue = value.slice();
          this.listAllHeader$.next(nextValue);
          this.listCurrentHeader$.next(getVisualHeader(nextValue));
        }
      }));

    /*
    this.subscription.add(
      combineLatest({
        listLogLineFilteredCondition: this.listLogLineFilteredCondition$,
        rangeFilter: this.logTimeDataService.rangeFilter$,
      }).subscribe({
        next: (value) => {
          const testStart = (epoch0.compareTo(value.rangeFilter.start) !== 0);
          const testFinish = (epoch0.compareTo(value.rangeFilter.finish) !== 0);
          const resultFilteredTime = value.listLogLineFilteredCondition.filter(
            (item) => {
              const ts = getLogLineTimestampValue(item);
              if (ts === null) { return false; }
              return (testStart ? (value.rangeFilter.start.compareTo(ts) <= 0) : true)
                && (testFinish ? (ts.compareTo(value.rangeFilter.finish) <= 0) : true);
            });
          console.log("filter start", value.rangeFilter.start.toString(), "finish", value.rangeFilter.finish.toString(), resultFilteredTime.length);
          this.listLogLineFilteredTime$.next(resultFilteredTime);
        }
      })
    );
    */

    this.subscription.add(this.logTimeDataService.currentLogLineId$.subscribe(this.currentLogLineId$));
    this.subscription.add(this.logTimeDataService.currentLogLine$.subscribe(this.currentLogLine$));
    this.subscription.add(this.logTimeDataService.currentLogTimestamp$.subscribe(this.currentLogTimestamp$));
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  getContent(logLine: LogLine, header: PropertyHeader): string {
    const property = logLine.data.get(header.name);
    if (undefined === property) { return ""; }
    switch (property.typeValue) {
      case 'null': return 'null';
      case 'str': return property.value;
      case 'enum': return property.value;
      case 'uuid': return property.value;
      case 'lvl': return property.value;
      case 'int': return property.value.toString();
      case 'dbl': return property.value.toString();
      case 'bool': return property.value.toString();
      case 'dt': return property.value.toString().replace('T', '\r\n');
      case 'dto': return property.value.toString().replace('T', '\r\n');
      case 'dur': return property.value.toString();
      default: return (property as any).value?.toString() ?? "";
    }
    //return property.value?.toString() ?? "";
  }

  setCurrentLogLine(logLineId: number, $event: MouseEvent) {
    const listLogLine = this.listLogLine$.getValue();
    const logLine = listLogLine.find((item) => (logLineId === item.id));
    console.log("LogViewComponent.setCurrentLogLine-logLine", logLine);

    this.logTimeDataService.currentLogLineId$.next(logLine?.id ?? null);
    $event.stopPropagation();
    return false;
  }

  setContextLogLine(logLineId: number, $event: PointerEvent) {
    console.log("setContextLogLine", logLineId, $event.button);
    const listLogLine = this.listLogLine$.getValue();
    const logLine = listLogLine.find((item) => (logLineId === item.id));
    if ((logLine === undefined)
      || (this.contextLogLineId$.getValue() === logLine.id)) {
      this.contextLogLineId$.next(null);
      this.contextLogLine$.next(null);
    } else {
      this.contextLogLineId$.next(logLine.id);
      this.contextLogLine$.next(logLine);
    }
    $event.stopPropagation();
    return false;
  }

  getCurrentDiff(logLine: LogLine): string {
    const currentLogTimestamp = this.currentLogTimestamp$.getValue();
    if (undefined === currentLogTimestamp || null === currentLogTimestamp) { return ""; }

    const timestamp = getLogLineTimestampValue(logLine);
    if (undefined === timestamp || null === timestamp) { return ""; }

    const dur = Duration.between(currentLogTimestamp, timestamp);
    {
      const mili = dur.toMillis();
      if (-2000 <= mili && mili <= 2000) { return mili.toFixed(2) + ' ms'; }
    }
    {
      const seconds = dur.seconds();
      if (-100 <= seconds && seconds <= 100) { return seconds.toFixed(2) + ' sec'; }
      const minutes = seconds / 60;
      if (-240 <= minutes && minutes <= 240) { return minutes.toFixed(2) + ' min'; }
      const hours = seconds / 3600;
      if (-24 <= hours && hours <= 24) { return hours.toFixed(2) + ' hrs'; }
    }
    return dur.toString();
  }

  addFilter(logLineId: number, headerId: string) {
    const listLogLine = this.listLogLine$.getValue();
    const listCurrentHeader = this.listCurrentHeader$.getValue();

    const logLine = listLogLine.find((item) => (logLineId === item.id));
    const header = listCurrentHeader.find((item) => (headerId === item.id));
    if ((undefined === logLine) || (undefined === header)) { return false; }

    const filter = logLine.data.get(header.name);
    if (undefined === filter) {
      header.filter = undefined;
    } else {
      header.filter = { ...filter };
    }

    this.filter$.next(1 + this.filter$.getValue());
    return false;
  }

  setFilterStrValue(header: PropertyHeader, value: string) {
    if (undefined === header.filter) {
      header.filter = {
        name: header.name,
        typeValue: "str",
        value: value
      };
    } else if ("str" === header.filter.typeValue) {
      header.filter.value = value;
    }
    this.filter$.next(1 + this.filter$.getValue());
    return false;
  }

  removeFilter(headerId: string) {
    const listCurrentHeader = this.listCurrentHeader$.getValue();
    const header = listCurrentHeader.find((item) => (headerId === item.id));
    if (undefined === header) { return false; }

    header.filter = undefined;

    this.filter$.next(1 + this.filter$.getValue());
    return false;
  }

  getHeaderStyle(propertyHeader: PropertyHeader) {
    const dataCellStyle = (propertyHeader.dataCellStyle == null)
      ? 'data-header'
      : `data-header ${propertyHeader.dataCellStyle || ''}`;
    return dataCellStyle;
  }

  dropHeader($event: CdkDragDrop<any, any, any>) {
    const listCurrentHeader = this.listCurrentHeader$.getValue();
    const { currentIndex, previousIndex } = $event;
    if (currentIndex === previousIndex) { return; }
    const lowerIndex = Math.min(currentIndex, previousIndex);
    const higherIndex = Math.max(currentIndex, previousIndex);
    moveItemInArray(listCurrentHeader, previousIndex, currentIndex);
    for (let idx = lowerIndex; idx <= higherIndex; idx++) {
      listCurrentHeader[idx].visualHeaderIndex = idx;
    }
    const listAllHeader = this.listAllHeader$.getValue();
    this.listCurrentHeader$.next(getVisualHeader(listAllHeader));
  }

  toggleColumn(headerId: string, headerName: string) {
    const listAllHeader = this.listAllHeader$.getValue();

    const listMatchingHeader = (headerId === "")
      ? listAllHeader.filter((header) => (headerName === header.name))
      : listAllHeader.filter((header) => (headerId === header.id) && (headerName === header.name));

    for (const header of listMatchingHeader) {
      header.show = !header.show;
    }
    this.listCurrentHeader$.next(getVisualHeader(listAllHeader));

    return false;
  }


}
