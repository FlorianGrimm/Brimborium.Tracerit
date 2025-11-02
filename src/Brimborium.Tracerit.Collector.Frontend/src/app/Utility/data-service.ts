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
  listLogLine$ = new BehaviorSubject<LogLine[]>([]);

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
    return name;
  }

  setListLogLine(data: LogLine[]) {
    // console.log("listLogLine$", data);
    const listNameLength = this.listAllHeader.length
    for (const line of data) {
      for (const prop of line.data.values()) {
        this.addMapName(prop.name, prop.typeValue);
      }
    }
    if (listNameLength !== this.listAllHeader.length) {
      this.listAllHeader$.next(this.listAllHeader);
    }
    this.listLogLine$.next(data);
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
