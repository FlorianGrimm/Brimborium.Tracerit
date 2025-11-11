import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { LogFileInformationList, LogLine, PropertyHeader, TypeValue } from '../Api';


@Injectable({
  providedIn: 'root'
})
export class DataService {
  mapName = new Map<string, PropertyHeader>();
  listAllHeader: PropertyHeader[] = [];
  listAllHeader$ = new BehaviorSubject<PropertyHeader[]>([]);
  listFile$ = new BehaviorSubject<LogFileInformationList>([]);
  currentFile$ = new BehaviorSubject<string | undefined>(undefined);
  listSelectedFileName$ = new BehaviorSubject<string[]>([]);

  listLogLine$ = new BehaviorSubject<LogLine[]>([]);
  mapLogLineByName = new Map<string, BehaviorSubject<LogLine[]>>();

  constructor() {
    this.addDefaultMapName();
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
    let contentSubject=this.mapLogLineByName.get(name);
    if (undefined === contentSubject){
      contentSubject=new BehaviorSubject<LogLine[]>(data);
      this.mapLogLineByName.set(name, contentSubject);
    } else {
      contentSubject.next(data);
    }
  }

  clearMapLogLineByNameOthers(listSelectedName: string[]) {
    const listToClear:string[]=[];
    for(let name of this.mapLogLineByName.keys()){
      if (listSelectedName.includes(name)){
        // OK
      } else {
        // clear
        listToClear.push(name);
      }
    }
    for(let name of listToClear){
      this.mapLogLineByName.delete(name);
    }
  }


  setListLogLine(data: LogLine[]) {
    //this.extractHeader(data);
    this.listLogLine$.next(data);
  }

  extractHeader(data: LogLine[]) {
    for (const line of data) {
      for (const prop of line.data.values()) {
        this.addMapName(prop.name, prop.typeValue);
      }
    }
    if (this.listAllHeader$.getValue().length !== this.listAllHeader.length) {
      this.listAllHeader$.next(this.listAllHeader);
    }
  }

  addDefaultMapName() {

    // this.addMapName("timestamp");
    // this.addMapName("source");
    // this.addMapName("scope");
    // this.addMapName("message");
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
    // this.addMapName("logLevel");
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

  addMapName(name: string, typeValue: TypeValue, show: boolean = true) {
    let result = this.mapName.get(name);
    if (result === undefined) {
      if (!this.mapName.has(name)) {
        const index = 1 + this.mapName.size;
        result = {
          id: `${name}-${typeValue}`,
          name: name,
          typeValue: typeValue,
          index: index,
          visualIndex: index,
          show: show
        }
        this.mapName.set(name, result);
        this.listAllHeader.push(result);
      }
    }
    return result;
  }
}
