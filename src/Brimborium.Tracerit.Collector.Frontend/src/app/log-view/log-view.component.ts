import { Component, inject } from '@angular/core';
import { BehaviorSubject, combineLatest, filter, map, Subscription } from 'rxjs';
import { DataService } from '../Utility/data-service';
import { HttpClientService } from '../Utility/http-client-service';
import { filterListLogLine, getLogLineTimestampValue, LogLine, PropertyHeader } from '../Api';
import { AsyncPipe, JsonPipe, KeyValuePipe } from '@angular/common';
import { getVisualHeader } from '../Utility/propertyHeaderUtility';
import { LucideAngularModule, FileStack, ChevronLeft, ChevronRight, Funnel, FunnelPlus, FunnelX } from 'lucide-angular';
import { RouterLink } from '@angular/router';
import { Duration, ZonedDateTime } from '@js-joda/core';

@Component({
  selector: 'app-log-view',
  imports: [
    AsyncPipe,
    RouterLink,
    LucideAngularModule],
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

  readonly listAllHeader$ = new BehaviorSubject<PropertyHeader[]>([]);
  readonly listCurrentHeader$ = new BehaviorSubject<PropertyHeader[]>([]);

  readonly listLogLine$ = new BehaviorSubject<LogLine[]>([]);
  readonly listLogLineDisplay$ = new BehaviorSubject<LogLine[]>([]);
  readonly filter$ = new BehaviorSubject<number>(1);

  readonly currentLogLineId$ = new BehaviorSubject<number | undefined>(undefined);
  readonly currentLogLine$ = new BehaviorSubject<LogLine | undefined>(undefined);
  readonly currentLogTimestamp$ = new BehaviorSubject<(ZonedDateTime | undefined)>(undefined);

  readonly contextLogLineId$ = new BehaviorSubject<number | undefined>(undefined);
  readonly contextLogLine$ = new BehaviorSubject<LogLine | undefined>(undefined);

  readonly error$ = new BehaviorSubject<undefined | string>(undefined);

  constructor() {
    this.subscription.add(
      this.dataService.listLogLine$.subscribe(this.listLogLine$));

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
        listCurrentHeader: this.listCurrentHeader$
      }).subscribe({
        next: (value) => {
          console.log("filter")
          const result = filterListLogLine(value.listLogLine, value.listCurrentHeader);
          this.listLogLineDisplay$.next(result);
        }
      })
    );
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
    this.currentLogLineId$.next(logLine?.id);
    this.currentLogLine$.next(logLine);
    this.currentLogTimestamp$.next(getLogLineTimestampValue(logLine));
    $event.stopPropagation();
    return false;
  }

  setContextLogLine(logLineId: number, $event: PointerEvent) {
    console.log("setContextLogLine", logLineId, $event.button);
    const listLogLine = this.listLogLine$.getValue();
    const logLine = listLogLine.find((item) => (logLineId === item.id));
    if (this.contextLogLineId$.getValue() === logLine?.id) {
      this.contextLogLineId$.next(undefined);
      this.contextLogLine$.next(undefined);
    } else {
      this.contextLogLineId$.next(logLine?.id);
      this.contextLogLine$.next(logLine);
    }
    $event.stopPropagation();
    return false;
  }

  getCurrentDiff(logLine: LogLine): string {
    const currentLogTimestamp = this.currentLogTimestamp$.getValue();
    if (undefined === currentLogTimestamp) { return ""; }

    const timestamp = getLogLineTimestampValue(logLine);
    if (undefined === timestamp) { return ""; }

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
    } else if ("str" === header.filter.typeValue){
      header.filter.value=value;
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
