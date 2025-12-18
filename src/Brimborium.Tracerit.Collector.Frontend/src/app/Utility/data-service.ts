import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, distinctUntilChanged, filter, Subscription } from 'rxjs';
import { getLogLineTraceId, LogFileInformationList, LogLine, PropertyHeader, TraceInformation, TypeValue } from '../Api';
import { DepDataService } from './dep-data.service';
import { emptyHeaderAndLogLine, HeaderAndLogLine } from './time-range';
import { getVisualHeader } from './propertyHeaderUtility';

@Injectable({
  providedIn: 'root'
})
export class DataService {
  public readonly subscription = new Subscription();
  public readonly depDataService = inject(DepDataService);
  public readonly currentStreamName = (new Date()).valueOf().toString();

  public mapName = new Map<string, PropertyHeader>();
  private listAllHeaderBuffer: PropertyHeader[] = [];

  public readonly listAllHeader = this.depDataService.createProperty({
    name: 'DataService_listAllHeader',
    initialValue: [] as PropertyHeader[],
    subscription: this.subscription,
  });

  public readonly useCurrentStream = this.depDataService.createProperty({
    name: 'DataService_useCurrentStream',
    initialValue: true,
    compare: (a, b) => (a === b),
    sideEffect: {
      fn: (value) => {
        if (value) {
          this.listLogLineSource.setValue([]);

          const listAllHeader = this.listAllHeader.getValue();
          const listVisualHeader = getVisualHeader(listAllHeader);
          this.listHeaderAndLogLineSource.setValue({ listAllHeader, listVisualHeader, listLogLine: [] });
        }
      }
    },
    subscription: this.subscription,
  });

  public readonly listFile = this.depDataService.createProperty({
    name: 'DataService_listFile',
    initialValue: [] as LogFileInformationList,
    subscription: this.subscription,
  });

  public readonly currentFile = this.depDataService.createProperty({
    name: 'DataService_currentFile',
    initialValue: undefined as (string | undefined),
    subscription: this.subscription,
  });

  public readonly listSelectedFileName = this.depDataService.createProperty({
    name: 'DataService_listSelectedFileName',
    initialValue: [] as string[],
    subscription: this.subscription,
  });

  public readonly listLogLineSource = this.depDataService.createProperty({
    name: 'DataService_listLogLineSource',
    initialValue: [] as LogLine[],
    subscription: this.subscription,
  });

  public readonly listHeaderAndLogLineSource = this.depDataService.createProperty<HeaderAndLogLine>({
    name: 'DataService_listHeaderAndLogLineSource',
    initialValue: emptyHeaderAndLogLine,
    subscription: this.subscription,
  });

  public readonly mapLogLineByName = new Map<string, BehaviorSubject<LogLine[]>>();

  mapTraceInformation: Map<string, TraceInformation> = new Map<string, TraceInformation>();

  constructor() {
    this.addDefaultMapName();
  }

  public setListFile(value: LogFileInformationList) {
    value.sort((a, b) => {
      const cmp = b.creationTimeUtc.compareTo(a.creationTimeUtc);
      if (0 !== cmp) { return cmp; }
      return a.name.localeCompare(b.name);
    });
    this.listFile.setValue(value);
    return value;
  }

  public setCurrentFile(name: (string | undefined)): (string | undefined) {
    this.currentFile.setValue(name);
    if (name === undefined) {
      this.listSelectedFileName.setValue([]);
    } else {
      this.listSelectedFileName.setValue([name]);
    }
    return name;
  }

  setListLogLineByName(name: string, data: LogLine[]) {
    this.extractHeader(data);
    let contentSubject = this.mapLogLineByName.get(name);
    if (undefined === contentSubject) {
      contentSubject = new BehaviorSubject<LogLine[]>(data);
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
      this.listLogLineSource.setValue(data);

      const listAllHeader = this.listAllHeaderBuffer.slice();
      const listVisualHeader = getVisualHeader(listAllHeader);
      this.listHeaderAndLogLineSource.setValue({
        listAllHeader,
        listVisualHeader,
        listLogLine: data,
      });
    } else {
      const currentData = this.listLogLineSource.getValue();
      // TODO: adjust the rangeZoom/rangeFilter if appended to the end and the last timestamp of rangeComplete and rangeZoom were equal
      // if (0<currentData.length){
      //   const lastId = currentData[currentData.length - 1].id;
      // }
      const currentDataLimited = (restMaxLength < currentData.length)
        ? currentData.slice(currentData.length - restMaxLength)
        : currentData;
      const nextData = currentDataLimited.concat(data);
      this.connectTraces(data, currentDataLimited);

      this.listLogLineSource.setValue(nextData);

      const listAllHeader = this.listAllHeaderBuffer.slice();
      const listVisualHeader = getVisualHeader(listAllHeader);
      this.listHeaderAndLogLineSource.setValue({
        listAllHeader,
        listVisualHeader,
        listLogLine: nextData,
      });
    }
  }

  /** the data comes from a file (from the webserver) */
  setListLogLine(data: LogLine[]) {
    this.extractHeader(data);
    this.connectTraces(data, undefined);
    this.listLogLineSource.setValue(data);

    const listAllHeader = this.listAllHeaderBuffer.slice();
    const listVisualHeader = getVisualHeader(listAllHeader);
    this.listHeaderAndLogLineSource.setValue({
      listAllHeader,
      listVisualHeader,
      listLogLine: data,
    });
  }

  connectTraces(nextData: LogLine[], previousData: LogLine[] | undefined) {
    const mapTraceInformation = new Map<string, TraceInformation>();
    this.mapTraceInformation = mapTraceInformation;
    if (previousData != null) {
      for (const logLine of nextData) {
        let traceInformation = logLine.traceInformation;
        if (traceInformation != null) {
          const traceId = traceInformation.traceId;
          traceInformation.listLogLineId = [logLine.id];
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

  // TODO
  extractHeader(data: LogLine[]) {
    const oldLength = this.listAllHeaderBuffer.length;
    for (const line of data) {
      for (const prop of line.data.values()) {
        this.addMapName(prop.name, prop.typeValue);
      }
    }
    // if (this.listAllHeader.getValue().length !== this.listAllHeaderBuffer.length) {
    //   this.listAllHeader.setValue([...this.listAllHeaderBuffer]);
    // }
    if (oldLength !== this.listAllHeaderBuffer.length) {
      this.listAllHeader.setValue(this.listAllHeaderBuffer.slice());
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

    this.addMapName("value", "str", false, -1, -1, 100);
    this.addMapName("event.id", "str", false, -1, -1, 100);
    this.addMapName("event.name", "str", false, -1, -1, 100);
    this.addMapName("activity.traceId", "str", false, -1, -1, 100);
    this.addMapName("activity.parentTraceId", "str", false, -1, -1, 100);
    this.addMapName("activity.parentTraceId.2", "str", false, -1, -1, 100);
    this.addMapName("activity.parentTraceId.3", "str", false, -1, -1, 100);
    this.addMapName("activity.spanId", "str", false, -1, -1, 100);
    this.addMapName("activity.parentSpanId", "str", false, -1, -1, 100);
    this.addMapName("activity.parentSpanId.2", "str", false, -1, -1, 100);
    this.addMapName("activity.parentSpanId.3", "str", false, -1, -1, 100);
    this.addMapName("activity.traceFlags", "str", false, -1, -1, 100);
    this.addMapName("exception.typeName", "str", false, -1, -1, 100);
    this.addMapName("exception.message", "str", false, -1, -1, 100);
    this.addMapName("exception.hResult", "str", false, -1, -1, 100);
    this.addMapName("exception.VerboseMessage", "str", false, -1, -1, 100);
    this.addMapName("activity.OperationName", "str", false, -1, -1, 100);
    this.addMapName("activity.DisplayName", "str", false, -1, -1, 100);
    this.addMapName("activity.StartTimeUtc", "dt", false, -1, -1, 100);
    this.addMapName("activity.StopTimeUtc", "dt", false, -1, -1, 100);
    this.addMapName("{OriginalFormat}", "str", false, -1, -1, 100);
  }

  addMapName(
    name: string,
    typeValue: TypeValue,
    show: boolean = false,
    visualHeaderIndex: number = -1,
    visualDetailHeaderIndex: number = -1,
    visualDetailBodyIndex: number = -1) {
    let result = this.mapName.get(name);
    if (result != null) { return result; }
    const index = 1 + this.mapName.size;
    const width = ("id" === name) ? 100 : 1;
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
    this.listAllHeaderBuffer.push(result);
    this.listAllHeaderBuffer.sort((a, b) => {
      let cmp = 0;
      if (0 <= a.visualDetailHeaderIndex && 0 <= b.visualDetailHeaderIndex) {
        cmp = a.visualDetailHeaderIndex - b.visualDetailHeaderIndex;
      }
      if (cmp != 0) { return cmp; }

      if (0 <= a.visualDetailBodyIndex && 0 <= b.visualDetailBodyIndex) {
        cmp = a.visualDetailBodyIndex - b.visualDetailBodyIndex;
      }
      if (cmp != 0) { return cmp; }

      cmp = a.name.localeCompare(b.name)
      return cmp;
    });
    return result;
  }
}
