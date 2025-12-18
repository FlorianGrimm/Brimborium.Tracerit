import { LogLineValue, parseJsonl } from './Api';
import { Duration, ZonedDateTime } from '@js-joda/core';

describe('App', () => {
  it('parseJsonl empty', () => {
    const actual = parseJsonl('', 1);
    expect(actual).toEqual({
      listLogline: [],
      nextId: 1
    });
  });

  it('parseJsonl one line one str property', () => {
    const actual = parseJsonl('[["abc","str","def"]]', 1);
    expect(actual).toEqual({
      listLogline:
        [
          {
            id: 1,
            ts: null,
            data: new Map<string, LogLineValue>([
              ["abc", { name: "abc", typeValue: "str", value: "def" }]
            ]),
            traceInformation: null,
            source: null
          }
        ],
      nextId: 2
    });
  });

  it('parseJsonl one line one int property', () => {
    const actual = parseJsonl('[["abc","int",123]]', 1);
    expect(actual).toEqual({
      listLogline:
        [
          {
            id: 1,
            ts: null,
            data: new Map<string, LogLineValue>([
              ["abc", { name: "abc", typeValue: "int", value: 123 }]
            ]),
            traceInformation: null,
            source: null
          }
        ],
      nextId: 2
    });
  });


  it('parseJsonl one line one int property', () => {
    const actual = parseJsonl('[["abc","lvl","error"]]', 1);
    expect(actual).toEqual({
      listLogline:
        [
          {
            id: 1,
            ts: null,
            data: new Map<string, LogLineValue>([
              ["abc", { name: "abc", typeValue: "lvl", value: "error" }]
            ]),
            traceInformation: null,
            source: null
          }
        ],
      nextId: 2
    });
  });
// TODO
  /*
it('parseJsonl one line one dt property', () => {
const actual = parseJsonl('[["abc","dt","2012-12-24T12:00:00"]]',1);
expect(actual).toEqual({
listLogline: [
{ "abc": { name: "abc", typeValue: "dt", value: ZonedDateTime.parse("2012-12-24T12:00:00Z") } },
]);
});

it('parseJsonl one line one dto property', () => {
const actual = parseJsonl('[["abc","dto","2012-12-24T12:00:00Z"]]',1);
expect(actual).toEqual({
listLogline: [
{ "abc": { name: "abc", typeValue: "dto", value: ZonedDateTime.parse("2012-12-24T12:00:00Z") } },
]);
});

it('parseJsonl one line one dur 1000000ns property', () => {
const actual = parseJsonl('[["abc","dur", 1000000]]',1);
expect(actual).toEqual({
listLogline: [
{ "abc": { name: "abc", typeValue: "dur", value: Duration.ofNanos(1000000) } },
]);
});

it('parseJsonl one line one dur 1ms property', () => {
const actual = parseJsonl('[["abc","dur", 1000000]]',1);
expect(actual).toEqual({
listLogline: [
{ "abc": { name: "abc", typeValue: "dur", value: Duration.ofMillis(1) } },
]);
});

it('parseJsonl one line one dur 60000000000 property', () => {
const actual = parseJsonl('[["abc","dur", 60000000000]]',1);
expect(actual).toEqual({
listLogline: [
{ "abc": { name: "abc", typeValue: "dur", value: Duration.ofMinutes(1) } },
]);
});

it('parseJsonl one line one bool property', () => {
const actual = parseJsonl('[["abc","bool", 1]]',1);
expect(actual).toEqual({
listLogline: [
{ "abc": { name: "abc", typeValue: "bool", value: true } },
]);
});

it('parseJsonl two lines', () => {
const actual = parseJsonl('[["abc","str","def"]]\r\n[["abc","str","def"],["ghi","str","jkl"]]',1);
expect(actual).toEqual({
listLogline: [
{ "abc": { name: "abc", typeValue: "str", value: "def" } },
{ "abc": { name: "abc", typeValue: "str", value: "def" }, "ghi": { name: "ghi", typeValue: "str", value: "jkl" } },
]);
});
*/
});
