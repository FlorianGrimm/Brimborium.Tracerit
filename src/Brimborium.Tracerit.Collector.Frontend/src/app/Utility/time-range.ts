import { BehaviorSubject } from 'rxjs';
import { Duration, ZonedDateTime, ZoneId } from '@js-joda/core';
import { DepDataProperty } from './dep-data.service';
import type { FilterAstNode, LogLine, PropertyHeader } from '@app/Api';

export const epoch0 = ZonedDateTime.of(1970, 1, 1, 0, 0, 0, 0, ZoneId.UTC);
export const epoch1 = ZonedDateTime.of(1970, 1, 1, 1, 1, 1, 1, ZoneId.UTC);
export const epoch01Range: TimeRange = Object.freeze({
  start: epoch0,
  finish: epoch1,
});

export const epoch01RangeDuration: TimeRangeDuration = Object.freeze({
  start: epoch0,
  finish: epoch1,
  duration: Duration.between(epoch0, epoch1)
});


export type TimeRange = {
  start: ZonedDateTime;
  finish: ZonedDateTime;
};

export type TimeRangeDuration = {
  start: ZonedDateTime;
  finish: ZonedDateTime;
  duration: Duration;
};

export type TimeRangeOrNull = {
  start: ZonedDateTime | null;
  finish: ZonedDateTime | null;
};

export const emptyTimeRangeOrNull :TimeRangeOrNull = Object.freeze({
  start:  null,
  finish:  null,
});

export type HeaderAndLogLine = {
  listAllHeader: readonly PropertyHeader[];
  listVisualHeader: readonly PropertyHeader[];
  listLogLine: readonly LogLine[];
}

export const emptyHeaderAndLogLine: HeaderAndLogLine = Object.freeze({
  listAllHeader: [],
  listVisualHeader: [],
  listLogLine: [],
});

export type LogLineTimeRangeDuration = {
  listAllHeader: readonly PropertyHeader[],
  listVisualHeader:  readonly PropertyHeader[];
  listLogLine: readonly LogLine[];
  range: TimeRangeDuration;
  filter: FilterAstNode | null;
}
export const emptyLogLineTimeRangeDuration: LogLineTimeRangeDuration = Object.freeze({
  listAllHeader: [],
  listVisualHeader: [],
  listLogLine: [],
  range: epoch01RangeDuration,
  filter: null,
});

// TimeRange
export function createTimeRange(start: ZonedDateTime, finish: ZonedDateTime): TimeRange {
  if (start.compareTo(finish) > 0) {
    debugger;
    console.error("start > finish");
  }
  return Object.freeze({
    start: start,
    finish: finish,
  });
}

export function createTimeRangeOrNull(start: ZonedDateTime | null, finish: ZonedDateTime | null): TimeRangeOrNull {
  if (start != null && finish != null && start.compareTo(finish) > 0) {
    debugger;
    console.error("start > finish");
  }
  return Object.freeze({
    start: start,
    finish: finish,
  });
}

export function createTimeRangeDuration(start: ZonedDateTime, finish: ZonedDateTime): TimeRangeDuration {
  if (start.compareTo(finish) > 0) {
    debugger;
    console.error("start > finish");
  }
  return Object.freeze({
    start: start,
    finish: finish,
    duration: Duration.between(start, finish)
  });
}

export function setTimeRangeStartIfChanged(
  subject: DepDataProperty<TimeRange>,
  value: ZonedDateTime
) {
  const currentValue = subject.getValue();
  if (currentValue.start.isEqual(value)) {
    //skip
  } else {
    subject.setValue(createTimeRange(value, currentValue.finish));
  }
}

export function setTimeRangeFinishIfChanged(
  subject: DepDataProperty<TimeRange>,
  value: ZonedDateTime
) {
  const currentValue = subject.getValue();
  if (currentValue.finish.isEqual(value)) {
    //skip
  } else {
    subject.setValue(createTimeRange(currentValue.start, value));
  }
}

export function setTimeRangeIfChanged(
  subject: DepDataProperty<TimeRange>,
  value: TimeRange
) {
  const currentValue = subject.getValue();
  if (currentValue.start.isEqual(value.start)
    && currentValue.finish.isEqual(value.finish)) {
    //skip
  } else {
    subject.setValue(createTimeRange(value.start, value.finish));
  }
}

// TimeRangeDuration

export function setTimeRangeDurationStartIfChanged(
  subject: DepDataProperty<TimeRangeDuration>,
  value: ZonedDateTime
) {
  const currentValue = subject.getValue();
  if (currentValue.start.isEqual(value)) {
    //skip
  } else {
    subject.setValue(createTimeRangeDuration(value, currentValue.finish));
  }
}

export function setTimeRangeDurationFinishIfChanged(
  subject: DepDataProperty<TimeRangeDuration>,
  value: ZonedDateTime
) {
  const currentValue = subject.getValue();
  if (currentValue.finish.isEqual(value)) {
    //skip
  } else {
    subject.setValue(createTimeRangeDuration(currentValue.start, value));
  }
}

export function setTimeRangeDurationIfChanged(
  subject: DepDataProperty<TimeRangeDuration>,
  value: TimeRange
) {
  const currentValue = subject.getValue();
  if (currentValue.start.isEqual(value.start)
    && currentValue.finish.isEqual(value.finish)) {
    //skip
  } else {
    subject.setValue(createTimeRangeDuration(value.start, value.finish));
  }
}

// TimeRangeOrNull

export function setTimeRangeOrNullStartIfChanged(
  subject: DepDataProperty<TimeRangeOrNull>,
  value: ZonedDateTime | null
) {
  const currentValue = subject.getValue();
  if ((currentValue.start === null && value === null)
    || (currentValue.start !== null && value !== null && currentValue.start.isEqual(value))
  ) {
    //skip
  } else {
    subject.setValue(createTimeRangeOrNull(value, currentValue.finish));
  }
}

export function setTimeRangeOrNullFinishIfChanged(
  subject: DepDataProperty<TimeRangeOrNull>,
  value: ZonedDateTime | null
) {
  const currentValue = subject.getValue();
  if ((currentValue.finish === null && value === null)
    || (currentValue.finish !== null && value !== null && currentValue.finish.isEqual(value))
  ) {
    //skip
  } else {
    subject.setValue(createTimeRangeOrNull(currentValue.start, value));
  }
}

export function setTimeRangeOrNullIfChanged(
  subject: BehaviorSubject<TimeRangeOrNull>,
  value: TimeRangeOrNull
) {
  const currentValue = subject.getValue();
  if (((currentValue.start === null && value.start == null)
    || (currentValue.start !== null && value.start !== null && currentValue.start.isEqual(value.start)))

    && ((currentValue.finish === null && value.finish === null)
      || (currentValue.finish !== null && value.finish !== null && currentValue.finish.isEqual(value.finish)))
  ) {
    //skip
  } else {
    subject.next(createTimeRangeOrNull(value.start, value.finish));
  }
}

export function getEffectiveRange(list: TimeRange[]): TimeRangeDuration {
  let start: ZonedDateTime | null = null;
  let finish: ZonedDateTime | null = null;

  for (const item of list) {
    if (epoch0.isEqual(item.start)) {
      //
    } else if (start === null || start.compareTo(item.start) < 0) {
      start = item.start;
    }

    if (epoch1.isEqual(item.finish)) {
      //
    } else if (finish === null || item.finish.compareTo(finish) < 0) {
      finish = item.finish;
    }
  }
  if (start === null) { start = epoch0; }
  if (finish === null) { finish = epoch1; }
  if (start.compareTo(finish) > 0) {
    finish = start;
  }
  return createTimeRangeDuration(start, finish);
}

export function getTimeRangeToDebugString(value: TimeRange | TimeRangeOrNull | null | undefined) {
  if (value == null) { return value; }
  return { start: value?.start?.toString(), finish: value?.finish?.toString() }
}
export function getTimeRangeDurationToDebugString(value: TimeRangeDuration | null | undefined) {
  if (value == null) { return value; }
  return { start: value?.start?.toString(), finish: value?.finish?.toString(), duration: value?.duration?.toString() }
}

export function equalsTimeRangeOrNull(
  a: TimeRangeOrNull,
  b: TimeRangeOrNull
) {
  if ((a.start == null) && (b.start == null)) { return true; }
  if ((a.start == null) || (b.start == null)) { return false; }

  if ((a.finish == null) && (b.finish == null)) { return true; }
  if ((a.finish == null) || (b.finish == null)) { return false; }

  return (a.start.isEqual(b.start))
    && (a.finish.isEqual(b.finish));
}


export function equalsTimeRangeDuration(
  a: TimeRangeDuration,
  b: TimeRangeDuration
) {
  return (a.start.isEqual(b.start))
    && (a.finish.isEqual(b.finish));
}

export function calcZoomRange(boundaries: TimeRangeDuration, value: TimeRangeDuration): TimeRangeDuration {
  // TODO
  return boundaries;
}