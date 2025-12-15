import { BehaviorSubject } from 'rxjs';
import { Duration, ZonedDateTime, ZoneId } from '@js-joda/core';

export const epoch0 = ZonedDateTime.of(1970, 1, 1, 0, 0, 0, 0, ZoneId.UTC);
export const epoch1 = ZonedDateTime.of(1970, 1, 1, 1, 1, 1, 1, ZoneId.UTC);

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

// TimeRange
export function createTimeRange(start: ZonedDateTime, finish: ZonedDateTime): TimeRange {
  if(start.compareTo(finish) > 0) {
    debugger;
    console.error("start > finish");
  }
  return Object.freeze({
    start: start,
    finish: finish,
  });
}

export function createTimeRangeOrNull(start: ZonedDateTime | null, finish: ZonedDateTime | null): TimeRangeOrNull {
  if(start != null && finish != null && start.compareTo(finish) > 0) {
    debugger;
    console.error("start > finish");
  }
  return Object.freeze({
    start: start,
    finish: finish,
  });
}

export function createTimeRangeDuration(start: ZonedDateTime, finish: ZonedDateTime): TimeRangeDuration {
  if(start.compareTo(finish) > 0) {
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
  subject: BehaviorSubject<TimeRange>,
  value: ZonedDateTime
) {
  const currentValue = subject.getValue();
  if (currentValue.start.isEqual(value)) {
    //skip
  } else {    
    subject.next(createTimeRange(value, currentValue.finish));
  }
}

export function setTimeRangeFinishIfChanged(
  subject: BehaviorSubject<TimeRange>,
  value: ZonedDateTime
) {
  const currentValue = subject.getValue();
  if (currentValue.finish.isEqual(value)) {
    //skip
  } else {
    subject.next(createTimeRange(currentValue.start, value));
  }
}

export function setTimeRangeIfChanged(
  subject: BehaviorSubject<TimeRange>,
  value: TimeRange
) {
  const currentValue = subject.getValue();
  if (currentValue.start.isEqual(value.start)
    && currentValue.finish.isEqual(value.finish)) {
    //skip
  } else {
    subject.next(createTimeRange(value.start, value.finish));
  }
}

// TimeRangeDuration

export function setTimeRangeDurationStartIfChanged(
  subject: BehaviorSubject<TimeRangeDuration>,
  value: ZonedDateTime
) {
  const currentValue = subject.getValue();
  if (currentValue.start.isEqual(value)) {
    //skip
  } else {
    subject.next(createTimeRangeDuration(value, currentValue.finish));
  }
}

export function setTimeRangeDurationFinishIfChanged(
  subject: BehaviorSubject<TimeRangeDuration>,
  value: ZonedDateTime
) {
  const currentValue = subject.getValue();
  if (currentValue.finish.isEqual(value)) {
    //skip
  } else {
    subject.next(createTimeRangeDuration(currentValue.start, value));
  }
}

export function setTimeRangeDurationIfChanged(
  subject: BehaviorSubject<TimeRangeDuration>,
  value: TimeRange
) {
  const currentValue = subject.getValue();
  if (currentValue.start.isEqual(value.start)
    && currentValue.finish.isEqual(value.finish)) {
    //skip
  } else {
    subject.next(createTimeRangeDuration(value.start, value.finish));
  }
}

// TimeRangeOrNull

export function setTimeRangeOrNullStartIfChanged(
  subject: BehaviorSubject<TimeRangeOrNull>,
  value: ZonedDateTime | null
) {
  const currentValue = subject.getValue();
  if ((currentValue.start === null && value === null)
    || (currentValue.start !== null && value !== null && currentValue.start.isEqual(value))
  ) {
    //skip
  } else {
    subject.next(createTimeRangeOrNull(value, currentValue.finish));
  }
}

export function setTimeRangeOrNullFinishIfChanged(
  subject: BehaviorSubject<TimeRangeOrNull>,
  value: ZonedDateTime | null
) {
  const currentValue = subject.getValue();
  if ((currentValue.finish === null && value === null)
    || (currentValue.finish !== null && value !== null && currentValue.finish.isEqual(value))
  ) {
    //skip
  } else {
    subject.next(createTimeRangeOrNull(currentValue.start, value));
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
    if (epoch0.isEqual(item.start) ) {
      //
    } else if (start === null || start.compareTo(item.start) < 0) {
      start = item.start;
    }

    if (epoch1.isEqual(item.finish) ) {
      //
    } else if (finish === null || item.finish.compareTo(finish)<0) {
      finish = item.finish;
    }
  }
  return createTimeRangeDuration(start ?? epoch0, finish ?? epoch1);  
}

export function getTimeRangeToDebugString(value: TimeRange|TimeRangeOrNull|null|undefined){
  if (value == null) { return value; }
  return { start: value?.start?.toString(), finish: value?.finish?.toString() }
}
export function getTimeRangeDurationToDebugString(value: TimeRangeDuration|null|undefined){
  if (value == null) { return value; }
  return { start: value?.start?.toString(), finish: value?.finish?.toString(), duration: value?.duration?.toString() }
}