import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, distinctUntilChanged, filter, Subscription } from 'rxjs';
import { getLogLineTraceId, LogFileInformationList, LogLine, PropertyHeader, TraceInformation, TypeValue } from '../Api';
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
  
  mapTraceInformation: Map<string, TraceInformation>=new Map<string, TraceInformation>();

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
    value.sort((a, b) => {
      const cmp = b.creationTimeUtc.compareTo(a.creationTimeUtc);
      if (0 !== cmp) { return cmp; }
      return a.name.localeCompare(b.name);
    });
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

  /** the data comes from the current stream (from the webserver) */
  addListLogLine(data: LogLine[]) {
    if (0 === data.length) { return; }

    this.extractHeader(data);
    const restMaxLength = 4096 - data.length;
    if (restMaxLength < 0) {
      this.listLogLineSource$.next(data);
    } else {
      const currentData = this.listLogLineSource$.getValue();
      // TODO: adjust the rangeZoom/rangeFilter if appended to the end and the last timestamp of rangeComplete and rangeZoom were equal
      // if (0<currentData.length){
      //   const lastId = currentData[currentData.length - 1].id;
      // }
      const currentDataLimited = (restMaxLength < currentData.length)
        ? currentData.slice(currentData.length - restMaxLength)
        : currentData;
      const nextData = currentDataLimited.concat(data);
      this.connectTraces(data,currentDataLimited);
      this.listLogLineSource$.next(nextData);
    }
  }

  /** the data comes from a file (from the webserver) */
  setListLogLine(data: LogLine[]) {
    this.extractHeader(data);
    this.connectTraces(data, undefined);
    this.listLogLineSource$.next(data);
  }
  connectTraces(nextData: LogLine[],previousData: LogLine[]|undefined) {
    const mapTraceInformation = new Map<string, TraceInformation>();
    this.mapTraceInformation=mapTraceInformation;
    if(previousData != null) { 
      for (const logLine of nextData) {
        let traceInformation = logLine.traceInformation;
        if (traceInformation != null) { 
          const traceId = traceInformation.traceId;
          traceInformation.listLogLineId=[logLine.id];
          mapTraceInformation.set(traceId, traceInformation);
        } else {
          let traceId = getLogLineTraceId(logLine);
          if (traceId == null || traceId.length === 0) { continue; }
          let traceInformation = mapTraceInformation.get(traceId);
          if (traceInformation == null) { 
            traceInformation = {
              traceId: traceId,
              spanId: "",
              parentSpanId: "",
              listLogLineId: []
            };
            logLine.traceInformation = traceInformation;
          } else {
            traceInformation.listLogLineId.push(logLine.id);
          }
          mapTraceInformation.set(traceId, traceInformation);
        }
      }      
    }
    {
      for (const logLine of nextData) {
        let traceId = getLogLineTraceId(logLine);
          if (traceId == null || traceId.length === 0) { continue; }
          let traceInformation = mapTraceInformation.get(traceId);
          if (traceInformation == null) { 
            traceInformation = {
              traceId: traceId,
              spanId: "",
              parentSpanId: "",
              listLogLineId: []
            };
            logLine.traceInformation = traceInformation;
          } else {
            traceInformation.listLogLineId.push(logLine.id);
          }
          mapTraceInformation.set(traceId, traceInformation);
      }
    }
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
    this.addMapName("timestamp", "dt", true, 0, 0, -1);
    this.addMapName("logLevel", "lvl", true, 1, 1, -1);
    this.addMapName("resource", "str", true, 2, 2, -1);
    this.addMapName("source", "str", true, 3, 3, -1);
    this.addMapName("scope", "str", true, 4, 4, -1);
    this.addMapName("message", "str", true, 5, -1, 0);

    this.addMapName("{OriginalFormat}", "str", false, -1, -1, 1000);

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

  addMapName(name: string, typeValue: TypeValue, 
    show: boolean = false, 
    visualHeaderIndex: number = -1,
    visualDetailHeaderIndex: number = -1, 
    visualDetailBodyIndex: number = -1) {
    let result = this.mapName.get(name);
    if (result === undefined) {
      if (!this.mapName.has(name)) {
        const index = 1 + this.mapName.size;
        const width = ("id" === name) ? 100 : 99;
        result = {
          id: `${name}-${typeValue}`,
          name: name,
          typeValue: typeValue,
          index: index,
          visualHeaderIndex: visualHeaderIndex,
          visualDetailHeaderIndex: visualDetailHeaderIndex,
          visualDetailBodyIndex: visualDetailBodyIndex,
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
