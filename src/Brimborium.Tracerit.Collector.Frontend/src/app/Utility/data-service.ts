import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, distinctUntilChanged, filter, Subscription } from 'rxjs';
import { LogFileInformationList, LogLine, PropertyHeader, TypeValue } from '../Api';
import { BehaviorRingSubject } from './BehaviorRingSubject';
import { MasterRingService } from './master-ring.service';

@Injectable({
  providedIn: 'root'
})
export class DataService {
  readonly subscription = new Subscription();
  readonly ring$ = inject(MasterRingService).dependendRing('DataService-ring$', this.subscription);
  readonly currentStreamName = (new Date()).valueOf().toString();

  mapName = new Map<string, PropertyHeader>();
  listAllHeader: PropertyHeader[] = [];

  readonly listAllHeader$ = new BehaviorRingSubject<PropertyHeader[]>([],
    0, 'DataService_listAllHeader', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });

  readonly useCurrentStream$ = new BehaviorRingSubject<boolean>(true, 1, 'DataService_useCurrentStream$', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value); });

  readonly listFile$ = new BehaviorRingSubject<LogFileInformationList>([],
    0, 'DataService_listFile', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });

  readonly currentFile$ = new BehaviorRingSubject<string | undefined>(undefined,
    0, 'DataService_currentFile', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value); });

  readonly listSelectedFileName$ = new BehaviorRingSubject<string[]>([],
    0, 'DataService_listSelectedFileName', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });

  // listLogLine 
  readonly listLogLineSource$ = new BehaviorRingSubject<LogLine[]>([],
    0, 'DataService_listLogLineSource', this.subscription, this.ring$, undefined,
    (name, message, value) => { console.log(name, message, value?.length); });

  readonly mapLogLineByName = new Map<string, BehaviorSubject<LogLine[]>>();

  constructor() {
    this.addDefaultMapName();

    this.useCurrentStream$.pipe(
      distinctUntilChanged(),
      filter(value => value)
    ).subscribe({
      next: (value) => {
        this.listLogLineSource$.next([]);
      }
    });
  }

  public setListFile(value: LogFileInformationList) {
    value.sort((a, b) => a.creationTimeUtc.compareTo(b.creationTimeUtc));
    this.listFile$.next(value);
    return value;
  }

  public setCurrentFile(name: (string | undefined)): (string | undefined) {
    this.currentFile$.next(name);
    if (name === undefined) {
      this.listSelectedFileName$.next([]);
    } else {
      this.listSelectedFileName$.next([name]);
    }
    return name;
  }

  setListLogLineByName(name: string, data: LogLine[]) {
    this.extractHeader(data);
    let contentSubject = this.mapLogLineByName.get(name);
    if (undefined === contentSubject) {
      contentSubject = new BehaviorRingSubject<LogLine[]>(
        data,
        1, `listLogLine$-${name}`, this.subscription, this.ring$, undefined,
        (name, message, value) => { console.log(name, message, value?.length); }
      );
      this.mapLogLineByName.set(name, contentSubject);
    } else {
      contentSubject.next(data);
    }
  }

  clearMapLogLineByNameOthers(listSelectedName: string[]) {
    const listToClear: string[] = [];
    for (let name of this.mapLogLineByName.keys()) {
      if (listSelectedName.includes(name)) {
        // OK
      } else {
        // clear
        listToClear.push(name);
      }
    }
    for (let name of listToClear) {
      this.mapLogLineByName.delete(name);
    }
  }

  addListLogLine(data: LogLine[]) {
    if (0 === data.length) { return; }

    this.extractHeader(data);
    const restMaxLength = 1024 - data.length;
    if (restMaxLength < 0) {
      this.listLogLineSource$.next(data);
    } else {
      const currentData = this.listLogLineSource$.getValue();
      const currentDataLimited = (restMaxLength < currentData.length)
        ? currentData.slice(currentData.length - restMaxLength)
        : currentData;
      const nextData = currentDataLimited.concat(data);
      this.listLogLineSource$.next(nextData);
    }
  }

  setListLogLine(data: LogLine[]) {
    this.extractHeader(data);
    this.listLogLineSource$.next(data);
  }

  extractHeader(data: LogLine[]) {
    for (const line of data) {
      for (const prop of line.data.values()) {
        this.addMapName(prop.name, prop.typeValue);
      }
    }
    if (this.listAllHeader$.getValue().length !== this.listAllHeader.length) {
      this.listAllHeader$.next([...this.listAllHeader]);
    }
  }

  addDefaultMapName() {
    this.addMapName("id", "int", false);
    this.addMapName("timestamp", "dt", true);
    this.addMapName("logLevel", "lvl", true);
    this.addMapName("source", "str", true);
    this.addMapName("scope", "str", true);
    this.addMapName("message", "str", true);
    // this.addMapName("value");
    // this.addMapName("event.id");
    // this.addMapName("event.name");
    // this.addMapName("activity.traceId");
    // this.addMapName("activity.parentTraceId");
    // this.addMapName("activity.parentTraceId.2");
    // this.addMapName("activity.parentTraceId.3");
    // this.addMapName("activity.spanId");
    // this.addMapName("activity.parentSpanId");
    // this.addMapName("activity.parentSpanId.2");
    // this.addMapName("activity.parentSpanId.3");
    // this.addMapName("activity.traceFlags");
    // this.addMapName("exception.typeName");
    // this.addMapName("exception.message");
    // this.addMapName("exception.hResult");
    // this.addMapName("exception.VerboseMessage");
    // this.addMapName("activity.OperationName");
    // this.addMapName("activity.DisplayName");
    // this.addMapName("activity.StartTimeUtc");
    // this.addMapName("activity.StopTimeUtc");
    // this.addMapName("{OriginalFormat}");
  }

  addMapName(name: string, typeValue: TypeValue, show: boolean = false) {
    let result = this.mapName.get(name);
    if (result === undefined) {
      if (!this.mapName.has(name)) {
        const index = 1 + this.mapName.size;
        const width = ("id"===name) ? 100:99;
        result = {
          id: `${name}-${typeValue}`,
          name: name,
          typeValue: typeValue,
          index: index,
          visualIndex: index,
          show: show,
          width: width,
          headerCellStyle: { width: `${width}px` },
          dataCellStyle: { width: `${width}px` }
        }
        this.mapName.set(name, result);
        this.listAllHeader.push(result);
      }
    }
    return result;
  }

  reloadIfNecessary() {
    if (this.useCurrentStream$.getValue()) {
      this.loadCurrentStream();
    }
  }

  loadCurrentStream() {

  }
}
