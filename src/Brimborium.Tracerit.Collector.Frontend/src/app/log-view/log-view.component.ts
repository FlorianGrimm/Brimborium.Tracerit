import { Component, inject, OnDestroy } from '@angular/core';
import { BehaviorSubject, combineLatest, delay, filter, map, Subscription } from 'rxjs';
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
import { MasterRingSubject } from "../Utility/MasterRingSubject";
import { MasterRingService } from '../Utility/master-ring.service';
import { AppRingOrder } from '../app-ring-order';

@Component({
  selector: 'app-log-view',
  imports: [
    AsyncPipe,
    RouterLink,
    LucideAngularModule,
    TimeRulerComponent
  ],
  templateUrl: './log-view.component.html',
  styleUrl: './log-view.component.scss',
})
export class LogViewComponent implements OnDestroy {
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
    AppRingOrder.LogViewComponent_listAllHeader, 'LogViewComponent_listAllHeader', this.subscription, this.ring$, undefined, 
    (name, message, value) => { console.log(name, message, value?.length); });
  readonly listCurrentHeader$ = new BehaviorRingSubject<PropertyHeader[]>([], 
    AppRingOrder.LogViewComponent_listCurrentHeader, 'LogViewComponent_listCurrentHeader', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });

  readonly listLogLine$ = new BehaviorRingSubject<LogLine[]>([], 
    AppRingOrder.LogViewComponent_listLogLine, 'LogViewComponent_listLogLine', this.subscription, this.ring$, undefined, 
    (name, message, value) => { console.log(name, message, value?.length); });
  readonly listLogLineFilteredCondition$ = new BehaviorRingSubject<LogLine[]>([], 
    AppRingOrder.LogViewComponent_listLogLineFilteredCondition, 'LogViewComponent_listLogLineFilteredCondition', this.subscription, this.ring$, undefined, 
    (name, message, value) => { console.log(name, message, value?.length); });
  readonly filter$ = new BehaviorRingSubject<number>(1, 1, 'filter$', this.subscription, this.ring$, undefined, BehaviorRingSubject.defaultLog);

  readonly currentLogLineId$ = new BehaviorRingSubject<number | null>(null, 
    AppRingOrder.LogViewComponent_currentLogLineId, 'LogViewComponent_currentLogLineId', this.subscription, this.ring$, undefined, BehaviorRingSubject.defaultLog);
  readonly currentLogLine$ = new BehaviorRingSubject<LogLine | null>(null, 
    AppRingOrder.LogViewComponent_currentLogLine, 'LogViewComponent_currentLogLine', this.subscription, this.ring$, undefined, BehaviorRingSubject.defaultLog);
  readonly currentLogTimestamp$ = new BehaviorRingSubject<(ZonedDateTime | null)>(null, 
    AppRingOrder.LogViewComponent_currentLogTimestamp, 'LogViewComponent_currentLogTimestamp', this.subscription, this.ring$, undefined, BehaviorRingSubject.defaultLog);

  readonly contextLogLineId$ = new BehaviorRingSubject<number | null>(null, 
    AppRingOrder.LogViewComponent_contextLogLineId, 'LogViewComponent_contextLogLineId', this.subscription, this.ring$, undefined, BehaviorRingSubject.defaultLog);
  readonly contextLogLine$ = new BehaviorRingSubject<LogLine | null>(null, 
    AppRingOrder.LogViewComponent_contextLogLine, 'LogViewComponent_contextLogLine', this.subscription, this.ring$, undefined, BehaviorRingSubject.defaultLog);

  readonly error$ = new BehaviorRingSubject<null | string>(null, 1, 'error$');

  constructor() {
    this.subscription.add(
      this.dataService.listLogLine$.pipeAndSubscribe(this.listLogLine$, (subject) => subject));

    this.subscription.add(
      this.logTimeDataService.listLogLineFilteredCondition$.pipeAndSubscribe(this.listLogLineFilteredCondition$, (subject) => subject));

    this.subscription.add(
      this.dataService.listAllHeader$.subscribe({
        next: (value) => {
          this.listAllHeader$.next(value);
          this.listCurrentHeader$.next(getVisualHeader(value));
        }
      }));

    // this.subscription.add(
    //   combineLatest({
    //     listLogLineFilteredCondition: this.logTimeDataService.listLogLineFilteredCondition$,
    //     rangeFilter: this.logTimeDataService.rangeFilter$,
    //   }).pipe(
    //     delay(0)
    //   ).subscribe({
    //     next: (value) => {
    //       /*
    //       const resultFilteredTime = value.listLogLineFilteredCondition.filter(
    //         (item) => {
    //           const ts = getLogLineTimestampValue(item);
    //           if (ts === null) { return false; }
    //           return (value.rangeFilter.start.compareTo(ts) <= 0) && (ts.compareTo(value.rangeFilter.finish) <= 0);
    //         });
    //       */
    //       this.logTimeDataService.listLogLineFilteredTime$.next(value.listLogLineFilteredCondition);
    //     }
    //   })
    // );

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

    const dur = Duration.between(timestamp, currentLogTimestamp);
    return dur.toString();
  }

  addFilter(logLineId: number, headerId: string) {
    const listLogLine = this.listLogLine$.getValue();
    const listCurrentHeader = this.listCurrentHeader$.getValue();

    const logLine = listLogLine.find((item) => (logLineId === item.id));
    const header = listCurrentHeader.find((item) => (headerId === item.id));
    if ((undefined === logLine) || (undefined === header)) { return false; }

    header.filter = logLine.data.get(header.name);

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

}
