import { inject, Injectable } from '@angular/core';
import { Subscription } from 'rxjs';
import { FilterAstNode, filterListLogLine, getLogLineTimestampValue, LogLine, PropertyHeader } from '../Api';
import { Duration, ZonedDateTime } from '@js-joda/core';
import { calcZoomRange, createTimeRangeDuration, createTimeRangeOrNull, emptyHeaderAndLogLine, emptyLogLineTimeRangeDuration, epoch0, epoch01RangeDuration, epoch1, equalsTimeRangeDuration, getEffectiveRange, getTimeRangeDurationToDebugString, getTimeRangeToDebugString, HeaderAndLogLine, LogLineTimeRangeDuration, setTimeRangeDurationIfChanged, setTimeRangeIfChanged, setTimeRangeOrNullIfChanged, TimeRange, TimeRangeDuration, TimeRangeOrNull } from './time-range';
import { DataService } from './data-service';
import { DepDataService } from './dep-data.service';
import { generateFilterFunction } from './filter-ast-node';
import { getVisualHeader } from './propertyHeaderUtility';

export type ModeZoom = 'complete' | 'zoom' | 'filter';

@Injectable({
  providedIn: 'root',
})
export class LogTimeDataService {
  public readonly subscription = new Subscription();
  public readonly depDataService = inject(DepDataService);
  public readonly depThis = this.depDataService.wrap(this);
  public readonly dataService = inject(DataService);

  public readonly listAllHeader = this.depThis.createProperty({
    name: 'LogTimeDataService_listAllHeader',
    initialValue: [] as PropertyHeader[],
    
  }).withSource(
    {
      sourceDependency: {
        listAllHeader: this.dataService.listAllHeader.dependencyPublic()
      },
      sourceTransform:
        (d) => d.listAllHeader,
      
    }
  );

  public readonly useCurrentStream = this.depThis.createProperty({
    name: 'LogTimeDataService_useCurrentStream',
    initialValue: false,
    
  });
  //readonly useCurrentStream$ = this.useCurrentStream.asObserable();

  // input
  public readonly listLogLineCurrentStream = this.depThis.createProperty<LogLine[]>({
    name: 'LogTimeDataService_listLogLineCurrentStream',
    initialValue: [],
    
  });

  public readonly listLogLineFiles = this.depThis.createProperty<LogLine[]>({
    name: 'LogTimeDataService_listLogLineFilesSubscripe2',
    initialValue: [],
    
  }).withSourceIdentity(
    this.dataService.listLogLineSource.dependencyPublic());

  public readonly listLogLineAll = this.depThis.createProperty({
    name: 'LogTimeDataService_listLogLineAll',
    initialValue: [] as LogLine[],
    
  }).withSourceIdentity(
    this.dataService.listLogLineSource.dependencyPublic());

  public readonly listHeaderAndLogLineAll = this.depThis.createProperty<HeaderAndLogLine>({
    name: 'LogTimeDataService_listHeaderAndLogLineAll',
    initialValue: emptyHeaderAndLogLine,
    
  }).withSourceIdentity(
    this.dataService.listHeaderAndLogLineSource.dependencyPublic());

  public readonly dataComplete = this.depThis.createProperty<LogLineTimeRangeDuration>({
    name: 'LogTimeDataService_dataComplete',
    initialValue: emptyLogLineTimeRangeDuration,
    sideEffect: {
      fn: (value) => {
        if (Object.is(value, emptyLogLineTimeRangeDuration)) { return; }

        const rangeComplete = value.range;
        const rangeZoom = this.rangeZoom.getValue();
        const nextRangeZoom = calcZoomRange(rangeComplete, rangeZoom);
        setTimeRangeDurationIfChanged(this.rangeZoom, nextRangeZoom);
      }
    },
    
  }).withSource({
    sourceDependency: {
      src: this.listHeaderAndLogLineAll.dependencyInner()
    },
    sourceTransform: ({ src }) => {
      if (0 === src.listLogLine.length) {
        return emptyLogLineTimeRangeDuration;
      } else {
        const range = calcStartFinish(src.listLogLine);
        const listVisualHeader = getVisualHeader(src.listAllHeader);
        const result: LogLineTimeRangeDuration = {
          listAllHeader: src.listAllHeader, listVisualHeader,
          listLogLine: src.listLogLine, range: range, filter: null
        };
        return result;
      }
    },
    
  });

  public readonly rangeComplete = this.depThis.createProperty<TimeRangeDuration>({
    name: 'LogTimeDataService_rangeComplete',
    initialValue: epoch01RangeDuration,
    
  }).withSource(
    {
      sourceDependency: {
        src: this.dataComplete.dependencyInner()
      },
      sourceTransform: ({ src }) => { return src.range; },
      
    }
  );

  // input
  public readonly modeZoom = this.depThis.createProperty({
    name: 'LogTimeDataService_modeZoom',
    initialValue: 'complete' as ModeZoom,
    
  });
  //readonly modeZoom$ = this.modeZoom.asObserable();

  public readonly rangeZoom = this.depThis.createProperty<TimeRangeDuration>({
    name: 'LogTimeDataService_rangeZoom',
    initialValue: epoch01RangeDuration,
    transform: (value) => {
      return calcRange([this.rangeComplete.getValue(), value]);
    },
    
  });

  public readonly dataZoom = this.depThis.createProperty<LogLineTimeRangeDuration>({
    name: 'LogTimeDataService_dataZoom',
    initialValue: emptyLogLineTimeRangeDuration,
    sideEffect: {
      fn: (value) => {
        if (value == emptyLogLineTimeRangeDuration) { return; }
        const rangeZoom = value.range;
        const rangeFilter = this.rangeFilter.getValue();
        const nextRangeFilter = calcZoomRange(rangeZoom, rangeFilter);
        setTimeRangeDurationIfChanged(this.rangeFilter, nextRangeFilter);
      }
    },
    
  }).withSource({
    sourceDependency: {
      dataComplete: this.dataComplete.dependencyInner(),
      modeZoom: this.modeZoom.dependencyInner(),
      rangeZoom: this.rangeZoom.dependencyInner(),
    },
    sourceTransform: ({ dataComplete, modeZoom, rangeZoom }) => {
      if (Object.is(dataComplete, emptyLogLineTimeRangeDuration)) { return emptyLogLineTimeRangeDuration; }

      const { listAllHeader, listVisualHeader, listLogLine, range: rangeComplete } = dataComplete;
      //if (d.modeZoom === 'complete') TODO
      const nextRangeZoom = getEffectiveRange([rangeComplete, rangeZoom]);
      let nextListLogLine = listLogLine;
      if (!equalsTimeRangeDuration(rangeComplete, nextRangeZoom)) {
        nextListLogLine = listLogLine.filter(item => {
          const ts = getLogLineTimestampValue(item);
          if (ts === null) { return false; }
          return (nextRangeZoom.start.compareTo(ts) <= 0)
            && (ts.compareTo(nextRangeZoom.finish) <= 0);
        });
      }
      const result: LogLineTimeRangeDuration = { listAllHeader, listVisualHeader, listLogLine: nextListLogLine, range: nextRangeZoom, filter: null };
      return result;
    },
    
  });

  public readonly rangeFilter = this.depThis.createProperty<TimeRangeDuration>({
    name: 'LogTimeDataService_rangeFilter',
    initialValue: epoch01RangeDuration,
    transform: (value) => {
      return calcRange([this.rangeComplete.getValue(), this.rangeZoom.getValue(), value]);
    },
    
  });

  public readonly dataTimeFiltered = this.depThis.createProperty<LogLineTimeRangeDuration>({
    name: 'LogTimeDataService_dataTimeFiltered',
    initialValue: emptyLogLineTimeRangeDuration,
    sideEffect: {
      fn: (value) => {
        if (Object.is(value, emptyLogLineTimeRangeDuration)) { return; }

        const rangeZoom = value.range;
        const rangeFilter = this.rangeFilter.getValue();
        const nextRangeFilter = calcZoomRange(rangeZoom, rangeFilter);
        setTimeRangeDurationIfChanged(this.rangeFilter, nextRangeFilter);
      }
    },
    
  }).withSource({
    sourceDependency: {
      dataZoom: this.dataZoom.dependencyInner(),
      rangeFilter: this.rangeFilter.dependencyInner(),
    },
    sourceTransform: ({ dataZoom, rangeFilter }) => {
      if (Object.is(dataZoom, emptyLogLineTimeRangeDuration)) { return emptyLogLineTimeRangeDuration; }
      const { listAllHeader, listVisualHeader, listLogLine, range: rangeZoom } = dataZoom;

      const nextRangeFilter = getEffectiveRange([rangeZoom, rangeFilter]);
      let nextListLogLine = listLogLine;
      if (!equalsTimeRangeDuration(rangeZoom, nextRangeFilter)) {
        nextListLogLine = listLogLine.filter(item => {
          const ts = getLogLineTimestampValue(item);
          if (ts === null) { return false; }
          return (nextRangeFilter.start.compareTo(ts) <= 0)
            && (ts.compareTo(nextRangeFilter.finish) <= 0);
        });
      }
      const result: LogLineTimeRangeDuration = { listAllHeader, listVisualHeader, listLogLine: nextListLogLine, range: nextRangeFilter, filter: null };
      return result;
    },
    
  });

  // filter
  // input
  // TODO
  public readonly listFilterCondition = this.depThis.createProperty({
    name: 'LogTimeDataService_listFilterCondition',
    initialValue: [] as PropertyHeader[],
    sideEffect: {
      fn: (value) => {
        // TODO
        //const next = integrate(value, this.filterAst.getValue());
        //this.filterAst.set(next);
      }
    },
    
  });

  public readonly filterAst = this.depThis.createProperty({
    name: 'LogTimeDataService_filterAst',
    initialValue: null as FilterAstNode | null,
    
  });

  public readonly dataFilteredCondition = this.depThis.createProperty<LogLineTimeRangeDuration>({
    name: 'LogTimeDataService_dataFilteredCondition',
    initialValue: emptyLogLineTimeRangeDuration,
    
  }).withSource({
    sourceDependency: {
      dataTimeFiltered: this.dataTimeFiltered.dependencyInner(),
      filterAst: this.filterAst.dependencyInner(),
    },
    sourceTransform: ({ dataTimeFiltered, filterAst }) => {
      if (Object.is(dataTimeFiltered, emptyLogLineTimeRangeDuration)) { return emptyLogLineTimeRangeDuration; }
      const { listAllHeader, listVisualHeader, listLogLine, range } = dataTimeFiltered;
      const filterFn = generateFilterFunction(filterAst);
      const nextListLogLine = listLogLine.filter(filterFn);
      const result: LogLineTimeRangeDuration = { listAllHeader, listVisualHeader, listLogLine: nextListLogLine, range, filter: filterAst };
      return result;
    },
    
  });

  // calculated output depended on listLogLine$ and listFilterCondition$
  public readonly listLogLineFilteredCondition = this.depThis.createProperty({
    name: 'LogTimeDataService_listLogLineFilteredCondition',
    initialValue: [] as LogLine[],
    
  }).withSource({
    sourceDependency: {
      listLogLineAll: this.listLogLineAll.dependencyInner(),
      listFilterCondition: this.listFilterCondition.dependencyInner(),
    },
    sourceTransform: (d) => {
      const result = filterListLogLine(
        d.listLogLineAll,
        d.listFilterCondition);
      return result;
    },
    
  }
  );
  public readonly $listLogLineFilteredCondition = this.listLogLineFilteredCondition.asSignal();

  public readonly listLogLineTimeZoomed = this.depThis.createProperty({
    name: 'LogTimeDataService_listLogLineTimeZoomed',
    initialValue: [] as LogLine[],
    
  }).withSource(
    {
      sourceDependency:
      {
        listLogLine: this.listLogLineAll.dependencyInner(),
        rangeZoom: this.rangeZoom.dependencyInner(),
      },
      sourceTransform:
        (d) => {
          const filterStart = (d.rangeZoom.start ?? epoch0).isEqual(epoch0);
          const filterFinish = (d.rangeZoom.finish ?? epoch1).isEqual(epoch1);

          if (filterStart && filterFinish) {
            return d.listLogLine;
          }

          const result = d.listLogLine.filter(item => {
            const ts = getLogLineTimestampValue(item);
            if (ts === null) { return false; }
            return (filterStart ? true : (d.rangeZoom.start.compareTo(ts) <= 0))
              && (filterFinish ? true : (ts.compareTo(d.rangeZoom.finish) <= 0));
          });
          return result;
        },
      
    }
  );

  // calculated output depended on listLogLineFilteredCondition$ and rangeFilter$
  public readonly listLogLineFilteredTime = this.depThis.createProperty({
    name: 'LogTimeDataService_listLogLineFilteredTime',
    initialValue: [] as LogLine[],
    
  }).withSource(
    {
      sourceDependency:
      {
        listLogLineTimeZoomed: this.listLogLineTimeZoomed.dependencyInner(),
        rangeFilter: this.rangeFilter.dependencyInner(),
      },
      sourceTransform:
        (d) => {
          let result: LogLine[] = [];
          const filterStart = (d.rangeFilter.start ?? epoch0).isEqual(epoch0);
          const filterFinish = (d.rangeFilter.finish ?? epoch1).isEqual(epoch1);

          if (filterStart && filterFinish) {
            return result = d.listLogLineTimeZoomed;
          }

          // TODO
          result = (d.listLogLineTimeZoomed as LogLine[]).filter(item => {
            const ts = getLogLineTimestampValue(item);
            if (ts === null) { return false; }
            return (filterStart ? true : (d.rangeFilter.start.compareTo(ts) <= 0))
              && (filterFinish ? true : (ts.compareTo(d.rangeFilter.finish) <= 0));
          });
          return result;
        },
      
    }
  );

  // input
  public readonly currentLogLineId = this.depThis.createProperty({
    name: 'LogTimeDataService_currentLogLineId',
    initialValue: null as (number | null),
    
  });

  // depended listLogLineFiltered$ currentLogLineId$
  public readonly currentLogLine = this.depThis.createProperty<LogLine | null>({
    name: 'LogTimeDataService_currentLogLine',
    initialValue: null as (LogLine | null),
    
  }).withSource(
    {
      sourceDependency:
      {
        dataTimeFiltered: this.dataTimeFiltered.dependencyInner(),
        currentLogLineId: this.currentLogLineId.dependencyInner(),
      },
      sourceTransform:
        ({ dataTimeFiltered, currentLogLineId }) => {
          if (currentLogLineId === null) { return null; }
          const result = dataTimeFiltered.listLogLine.find(item => item.id === currentLogLineId) ?? null;
          return result;
        },
      
    });

  public readonly currentLogTimestamp = this.depThis.createProperty<ZonedDateTime | null>({
    name: 'LogTimeDataService_currentLogTimestamp',
    initialValue: null,
    
  }).withSource(
    {
      sourceDependency: {
        currentLogLine: this.currentLogLine.dependencyInner(),
      },
      sourceTransform: ({ currentLogLine }) => {
        const result = getLogLineTimestampValue(currentLogLine);
        return result;
      },
      
    }
  );

  // input
  public readonly listLogLineIdHighlighted = this.depThis.createProperty({
    name: 'LogTimeDataService_listLogLineIdHighlighted',
    initialValue: new Set<string>(),
    
  });

  public readonly rangeCurrentSelected = this.depThis.createProperty<TimeRangeOrNull>({
    name: 'LogTimeDataService_rangeCurrentSelected',
    initialValue: Object.freeze({ start: null, finish: null }),
    
  }).withSource({
    sourceDependency: {
      currentLogLine: this.currentLogLine.dependencyInner(),
    },
    sourceTransform: ({ currentLogLine }) => {
      const ts = getLogLineTimestampValue(currentLogLine);
      return createTimeRangeOrNull(ts, ts);
    },
    
  });

  constructor() {
    this.depThis.executePropertyInitializer();
  }

  setRangeComplete(value: TimeRangeDuration) {
    const rangeComplete = createTimeRangeDuration(value.start, value.finish);
    this.rangeComplete.setValue(value);

    const rangeZoom = this.rangeZoom.getValue();
    const nextRangeZoom = getEffectiveRange([rangeComplete, rangeZoom]);
    setTimeRangeDurationIfChanged(this.rangeZoom, nextRangeZoom);

    const rangeFilter = this.rangeFilter.getValue();
    const nextRangeFilter = getEffectiveRange([rangeComplete, rangeZoom, rangeFilter]);
    setTimeRangeDurationIfChanged(this.rangeFilter, nextRangeFilter);
  }

  setRangeZoom(value: TimeRangeOrNull) {
    const valueNormalized = {
      start: value.start ?? epoch0,
      finish: value.finish ?? epoch1
    };
    const rangeComplete = this.rangeComplete.getValue();
    const nextRangeZoom = getEffectiveRange([rangeComplete, valueNormalized]);
    setTimeRangeDurationIfChanged(this.rangeZoom, nextRangeZoom);

    const rangeFilter = this.rangeFilter.getValue();
    const nextRangeFilter = getEffectiveRange([rangeComplete, nextRangeZoom, rangeFilter]);
    setTimeRangeDurationIfChanged(this.rangeFilter, nextRangeFilter);
  }

  setRangeFilter(value: TimeRangeOrNull) {
    const valueNormalized = {
      start: value.start ?? epoch0,
      finish: value.finish ?? epoch1
    };
    const rangeComplete = this.dataComplete.getValue().range;
    const rangeZoom = this.dataZoom.getValue().range;
    const nextRangeFilter = getEffectiveRange([rangeComplete, rangeZoom, valueNormalized]);
    setTimeRangeDurationIfChanged(this.rangeFilter, nextRangeFilter);
  }

}

function calcStartFinish(value: LogLine[]) {
  if (0 === value.length) {
    return createTimeRangeDuration(epoch0, epoch1);
  } else {

    let start: ZonedDateTime | null = null;
    let finish: ZonedDateTime | null = null;

    for (let idx = 0; idx < value.length; idx++) {
      const item = value[idx];
      const ts = getLogLineTimestampValue(item);
      if (ts === null) { continue; }
      if (ts === epoch0) { continue; }
      start = ts;
      break;
    }
    for (let idx = value.length - 1; 0 <= idx; idx--) {
      const item = value[idx];
      const ts = getLogLineTimestampValue(item);
      if (ts === null) { continue; }
      if (ts === epoch1) { continue; }

      finish = ts;
      break;
    }
    return createTimeRangeDuration(start ?? epoch0, finish ?? epoch1);
  }

}

function calcRange(lst: TimeRangeDuration[]) {
  const result = getEffectiveRange(lst);
  if (result.duration.isNegative()) {
    console.error("calcRange duration is negative", { start: result.start.toString(), finish: result.finish.toString() });
    //   return lst[0];
    // } else {
  }
  return result;
}