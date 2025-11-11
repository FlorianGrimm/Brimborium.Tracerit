import { Component, inject } from '@angular/core';
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
export class LogViewComponent {
  readonly FileStack = FileStack;
  readonly ChevronLeft = ChevronLeft;
  readonly ChevronRight = ChevronRight;
  readonly Funnel = Funnel;
  readonly FunnelPlus = FunnelPlus;
  readonly FunnelX = FunnelX;

  readonly subscription = new Subscription();
  readonly dataService = inject(DataService);
  readonly httpClientService = inject(HttpClientService);
  readonly logTimeDataService = inject(LogTimeDataService);

  readonly listAllHeader$ = new BehaviorSubject<PropertyHeader[]>([]);
  readonly listCurrentHeader$ = new BehaviorSubject<PropertyHeader[]>([]);

  readonly listLogLine$ = new BehaviorSubject<LogLine[]>([]);
  readonly listLogLineFiltered$ = new BehaviorSubject<LogLine[]>([]);
  readonly filter$ = new BehaviorSubject<number>(1);

  readonly currentLogLineId$ = new BehaviorSubject<number | null>(null);
  readonly currentLogLine$ = new BehaviorSubject<LogLine | null>(null);
  readonly currentLogTimestamp$ = new BehaviorSubject<(ZonedDateTime | null)>(null);

  readonly contextLogLineId$ = new BehaviorSubject<number | null>(null);
  readonly contextLogLine$ = new BehaviorSubject<LogLine | null>(null);

  readonly error$ = new BehaviorSubject<null | string>(null);

  constructor() {
    this.subscription.add(
      this.dataService.listLogLine$.subscribe(this.listLogLine$));

    this.subscription.add(
      this.logTimeDataService.listLogLineFilteredCondition$.subscribe(this.listLogLineFiltered$));

    this.subscription.add(
      this.dataService.listAllHeader$.subscribe({
        next: (value) => {
          this.listAllHeader$.next(value);
          this.listCurrentHeader$.next(getVisualHeader(value));
        }
      }));

    this.subscription.add(
      combineLatest({
        filter: this.filter$,
        listLogLine: this.listLogLine$,
        listCurrentHeader: this.listCurrentHeader$,
        startZoom: this.logTimeDataService.startZoom$,
        startFilter: this.logTimeDataService.startFilter$,
        finishFilter: this.logTimeDataService.finishFilter$,
        finishZoom: this.logTimeDataService.finishZoom$
      }).pipe(
        delay(0)
      ).subscribe({
        next: (value) => {
          const resultFilteredCondition = filterListLogLine(value.listLogLine, value.listCurrentHeader);
          const resultFilteredTime = resultFilteredCondition.filter(item => {
            const ts = getLogLineTimestampValue(item);
            if (ts === null) { return false; }
            return (value.startFilter.compareTo(ts) <= 0) && (ts.compareTo(value.finishFilter) <= 0);
          });
          this.logTimeDataService.listLogLineFilteredCondition$.next(resultFilteredCondition);
        }
      })
    );

    this.subscription.add(
      combineLatest({
        listLogLineFilteredCondition: this.logTimeDataService.listLogLineFilteredCondition$,
        startZoom: this.logTimeDataService.startZoom$,
        startFilter: this.logTimeDataService.startFilter$,
        finishFilter: this.logTimeDataService.finishFilter$,
        finishZoom: this.logTimeDataService.finishZoom$
      }).pipe(
        delay(0)
      ).subscribe({
        next: (value) => {
          const resultFilteredTime = value.listLogLineFilteredCondition.filter(
            (item) => {
              const ts = getLogLineTimestampValue(item);
              if (ts === null) { return false; }
              return (value.startFilter.compareTo(ts) <= 0) && (ts.compareTo(value.finishFilter) <= 0);
            });
          this.logTimeDataService.listLogLineFilteredTime$.next(resultFilteredTime);
        }
      })
    );

    this.subscription.add(this.logTimeDataService.currentLogLineId$.subscribe(this.currentLogLineId$));
    this.subscription.add(this.logTimeDataService.currentLogLine$.subscribe(this.currentLogLine$));
    this.subscription.add(this.logTimeDataService.currentLogTimestamp$.subscribe(this.currentLogTimestamp$));
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
