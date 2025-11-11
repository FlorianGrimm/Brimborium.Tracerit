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

export function setTimeRangeStartIfChanged(
  subject: BehaviorSubject<TimeRange>,
  value: ZonedDateTime
) {
  const currentValue = subject.getValue();
  if (currentValue.start.isEqual(value)) {
    //skip
  } else {
    subject.next(Object.freeze({
      start: value,
      finish: currentValue.finish,
    }));
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
    subject.next(Object.freeze({
      start: currentValue.start,
      finish: value,
    }));
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
    subject.next(Object.freeze(value));
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
    subject.next(Object.freeze({
      start: value,
      finish: currentValue.finish,
      duration: Duration.between(value, currentValue.finish)
    }));
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
    subject.next(Object.freeze({
      start: currentValue.start,
      finish: value,
      duration: Duration.between(currentValue.start, value)
    }));
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
    subject.next(Object.freeze({
      start: value.start,
      finish: value.finish,
      duration: Duration.between(value.start, value.finish)
    }));
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
    subject.next(Object.freeze({
      start: value,
      finish: currentValue.finish,
    }));
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
    subject.next(Object.freeze({
      start: currentValue.start,
      finish: value,
    }));
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
    subject.next(Object.freeze(value));
  }
}
