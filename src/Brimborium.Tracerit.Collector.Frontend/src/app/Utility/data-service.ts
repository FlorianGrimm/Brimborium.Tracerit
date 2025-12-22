import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, distinctUntilChanged, filter, Subscription } from 'rxjs';
import { getLogLineTraceId, LogFileInformationList, LogLine, PropertyHeader, TraceInformation, TypeValue } from '../Api';
import { DepDataService } from './dep-data.service';
import { emptyHeaderAndLogLine, HeaderAndLogLine } from './time-range';
import { getVisualHeader } from './propertyHeaderUtility';

export type HeaderUpdate = { header: Partial<{ id: string; name?: string }>; update: Partial<PropertyHeader>; };

@Injectable({
  providedIn: 'root'
})
export class DataService {

  public readonly subscription = new Subscription();
  public readonly depDataService = inject(DepDataService);
  public readonly depThis = this.depDataService.wrap(this);
  public readonly currentStreamName = (new Date()).valueOf().toString();

  public mapHeaderById = new Map<string, PropertyHeader>();
  public mapHeaderByName = new Map<string, PropertyHeader>();
  private listAllHeaderBuffer: PropertyHeader[] = [];

  public readonly listAllHeader = this.depThis.createProperty<ReadonlyArray<PropertyHeader>>({
    name: 'listAllHeader',
    initialValue: [],
  });

  public readonly listAllHeaderSortedByName = this.depThis.createProperty<ReadonlyArray<PropertyHeader>>({
    name: 'listAllHeaderSortedByName',
    initialValue: [],
    compare: (a, b) => {
      if (a.length !== b.length) { return false; }
      for (let i = 0; i < a.length; i++) {
        if (!Object.is(a[i], b[i])) { return false; }
      }
      return true;
    },
  }).withSource({
    sourceDependency: {
      listAllHeader: this.listAllHeader.dependencyInner()
    },
    sourceTransform: ({ listAllHeader }) => {
      const result = listAllHeader.slice();
      result.sort((a, b) => a.name.localeCompare(b.name));
      return Object.freeze(result);
    },
  });

  public readonly listVisualHeader = this.depThis.createProperty<ReadonlyArray<PropertyHeader>>({
    name: 'listVisualHeader',
    initialValue: [],
  }).withSource({
    sourceDependency: {
      listAllHeader: this.listAllHeader.dependencyInner()
    },
    sourceTransform: ({ listAllHeader }) => {
      const result = getVisualHeader(listAllHeader);
      return result;
    },
  });

  public readonly useCurrentStream = this.depThis.createProperty({
    name: 'useCurrentStream',
    initialValue: true,
    compare: (a, b) => (a === b),
    sideEffect: {
      fn: (value) => {
        if (value) {
          this.listLogLineSource.setValue([]);

          const listAllHeader = this.listAllHeader.getValue();
          const listVisualHeader = this.listVisualHeader.getValue();
          this.listHeaderAndLogLineSource.setValue({ listAllHeader, listVisualHeader, listLogLine: [] });
        }
      }
    },

  });

  public readonly listFile = this.depThis.createProperty({
    name: 'listFile',
    initialValue: [] as LogFileInformationList,

  });

  public readonly currentFile = this.depThis.createProperty({
    name: 'currentFile',
    initialValue: undefined as (string | undefined),

  });

  public readonly listSelectedFileName = this.depThis.createProperty({
    name: 'listSelectedFileName',
    initialValue: [] as string[],

  });

  public readonly listLogLineSource = this.depThis.createProperty({
    name: 'listLogLineSource',
    initialValue: [] as LogLine[],

  });

  public readonly listHeaderAndLogLineSource = this.depThis.createProperty<HeaderAndLogLine>({
    name: 'listHeaderAndLogLineSource',
    initialValue: emptyHeaderAndLogLine,
  }).withSource({
    sourceDependency: {
      listAllHeader: this.listAllHeader.dependencyInner(),
      listVisualHeader: this.listVisualHeader.dependencyInner(),
      listLogLineSource: this.listLogLineSource.dependencyInner(),
    },
    sourceTransform: ({ listAllHeader, listVisualHeader, listLogLineSource }) => {
      return { listAllHeader, listVisualHeader, listLogLine: listLogLineSource };
    },
  });

  public readonly mapLogLineByName = new Map<string, BehaviorSubject<LogLine[]>>();

  mapTraceInformation: Map<string, TraceInformation> = new Map<string, TraceInformation>();

  constructor() {
    this.addDefaultMapName();
    this.depThis.executePropertyInitializer();
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
    this.depDataService.scoped(() => {
      
      const currentListAllHeader = this.listAllHeader.getValue();
      if (currentListAllHeader.length !== this.listAllHeaderBuffer.length) {
        const listAllHeader = this.listAllHeaderBuffer.slice();
        this.listAllHeader.setValue(listAllHeader);
      }
      this.listLogLineSource.setValue(data);
    });
    // this.listHeaderAndLogLineSource.setValue({
    //   listAllHeader,
    //   listVisualHeader,
    //   listLogLine: data,
    // });
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
        this.addHeader(prop.name, prop.typeValue);
      }
    }
    // if (this.listAllHeader.getValue().length !== this.listAllHeaderBuffer.length) {
    //   this.listAllHeader.setValue([...this.listAllHeaderBuffer]);
    // }
    if (oldLength !== this.listAllHeaderBuffer.length) {
      const nextListAllHeader = Object.freeze(this.listAllHeaderBuffer.slice());
      this.listAllHeader.setValue(nextListAllHeader);
    }
  }

  private addDefaultMapName() {
    this.addHeader("id", "int", false);

    this.addHeader("timestamp", "dt", true, 0, 0, -1);
    this.addHeader("logLevel", "lvl", true, 1, 1, -1);
    this.addHeader("resource", "str", true, 2, 2, -1);
    this.addHeader("source", "str", true, 3, 3, -1);
    this.addHeader("scope", "str", true, 4, 4, -1);
    this.addHeader("message", "str", true, 5, -1, 0);

    this.addHeader("{OriginalFormat}", "str", false, -1, -1, 1000);

    this.addHeader("value", "str", false, -1, -1, 100);
    this.addHeader("event.id", "str", false, -1, -1, 100);
    this.addHeader("event.name", "str", false, -1, -1, 100);
    this.addHeader("activity.traceId", "str", false, -1, -1, 100);
    this.addHeader("activity.parentTraceId", "str", false, -1, -1, 100);
    this.addHeader("activity.parentTraceId.2", "str", false, -1, -1, 100);
    this.addHeader("activity.parentTraceId.3", "str", false, -1, -1, 100);
    this.addHeader("activity.spanId", "str", false, -1, -1, 100);
    this.addHeader("activity.parentSpanId", "str", false, -1, -1, 100);
    this.addHeader("activity.parentSpanId.2", "str", false, -1, -1, 100);
    this.addHeader("activity.parentSpanId.3", "str", false, -1, -1, 100);
    this.addHeader("activity.traceFlags", "str", false, -1, -1, 100);
    this.addHeader("exception.typeName", "str", false, -1, -1, 100);
    this.addHeader("exception.message", "str", false, -1, -1, 100);
    this.addHeader("exception.hResult", "str", false, -1, -1, 100);
    this.addHeader("exception.VerboseMessage", "str", false, -1, -1, 100);
    this.addHeader("activity.OperationName", "str", false, -1, -1, 100);
    this.addHeader("activity.DisplayName", "str", false, -1, -1, 100);
    this.addHeader("activity.StartTimeUtc", "dt", false, -1, -1, 100);
    this.addHeader("activity.StopTimeUtc", "dt", false, -1, -1, 100);
    this.addHeader("{OriginalFormat}", "str", false, -1, -1, 100);
  }

  private addHeader(
    name: string,
    typeValue: TypeValue,
    show: boolean = false,
    visualHeaderIndex: number = -1,
    visualDetailHeaderIndex: number = -1,
    visualDetailBodyIndex: number = -1) {
    let result = this.mapHeaderByName.get(name);
    if (result != null) { return result; }
    const index = 1 + this.mapHeaderByName.size;
    const width = ("id" === name) ? 100 : 1;
    result = Object.freeze<PropertyHeader>({
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
    });
    this.mapHeaderByName.set(result.name, result);
    this.mapHeaderById.set(result.id, result);

    this.listAllHeaderBuffer.push(result);
    /*
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
    */
    return result;
  }


  updateHeaders(updates: HeaderUpdate[]) {
    let changed = false;
    for (const { header, update } of updates) {
      // find the header
      let currentHeader: PropertyHeader | undefined = undefined;
      if ("string" === typeof header.id) {
        currentHeader = this.mapHeaderById.get(header.id);
      } else if ("string" === typeof header.name) {
        currentHeader = this.mapHeaderByName.get(header.name);
      } else {
        continue;
      }
      if (currentHeader == null) {
        throw new Error('header is null');
      }

      // do not update id and name
      if (update.id != null) {
        delete update.id;
      }
      if (update.name != null) {
        delete update.name;
      }

      // check if equal
      for (const key of Object.keys(update)) {
        const currentValue = (currentHeader as any)[key];
        const nextValue = (update as any)[key];
        if (currentValue === nextValue) {
          delete (update as any)[key];
        }
      }
      if (Object.keys(update).length === 0) { continue; }

      // apply the updates
      const nextHeader = { ...currentHeader, ...update };
      this.mapHeaderById.set(currentHeader.id, nextHeader);
      this.mapHeaderByName.set(currentHeader.name, nextHeader);
      const indexListAllHeaderBuffer = this.listAllHeaderBuffer.findIndex((h) => (h.id === currentHeader.id));
      if (indexListAllHeaderBuffer < 0) {
        this.listAllHeaderBuffer.push(nextHeader);
      } else {
        this.listAllHeaderBuffer[indexListAllHeaderBuffer] = nextHeader;
      }
      changed = true;
    }

    if (changed) {
      const nextListAllHeader = Object.freeze(this.listAllHeaderBuffer.slice());
      this.listAllHeader.setValue(nextListAllHeader);
    }
  }
}
