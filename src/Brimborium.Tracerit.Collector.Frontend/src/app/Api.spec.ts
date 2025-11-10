import { TestBed } from '@angular/core/testing';
import { parseJsonl } from './Api';
import { Duration, ZonedDateTime } from '@js-joda/core';

describe('App', () => {
  it('parseJsonl empty', () => {
    const actual = parseJsonl('');
    expect(actual).toEqual([]);
  });

  it('parseJsonl one line one str property', () => {
    const actual = parseJsonl('[["abc","str","def"]]');
    expect(actual).toEqual([
      { "abc": { name: "abc", typeValue: "str", value: "def" } },
    ]);
  });

  it('parseJsonl one line one int property', () => {
    const actual = parseJsonl('[["abc","int",123]]');
    expect(actual).toEqual([
      { "abc": { name: "abc", typeValue: "int", value: 123 } },
    ]);
  });

  it('parseJsonl one line one int property', () => {
    const actual = parseJsonl('[["abc","lvl","error"]]');
    expect(actual).toEqual([
      { "abc": { name: "abc", typeValue: "lvl", value: "error" } },
    ]);
  });

  it('parseJsonl one line one dt property', () => {
    const actual = parseJsonl('[["abc","dt","2012-12-24T12:00:00"]]');
    expect(actual).toEqual([
      { "abc": { name: "abc", typeValue: "dt", value: ZonedDateTime.parse("2012-12-24T12:00:00Z") } },
    ]);
  });

  it('parseJsonl one line one dto property', () => {
    const actual = parseJsonl('[["abc","dto","2012-12-24T12:00:00Z"]]');
    expect(actual).toEqual([
      { "abc": { name: "abc", typeValue: "dto", value: ZonedDateTime.parse("2012-12-24T12:00:00Z") } },
    ]);
  });

  it('parseJsonl one line one dur 1000000ns property', () => {
    const actual = parseJsonl('[["abc","dur", 1000000]]');
    expect(actual).toEqual([
      { "abc": { name: "abc", typeValue: "dur", value: Duration.ofNanos(1000000) } },
    ]);
  });

  it('parseJsonl one line one dur 1ms property', () => {
    const actual = parseJsonl('[["abc","dur", 1000000]]');
    expect(actual).toEqual([
      { "abc": { name: "abc", typeValue: "dur", value: Duration.ofMillis(1) } },
    ]);
  });

  it('parseJsonl one line one dur 60000000000 property', () => {
    const actual = parseJsonl('[["abc","dur", 60000000000]]');
    expect(actual).toEqual([
      { "abc": { name: "abc", typeValue: "dur", value: Duration.ofMinutes(1) } },
    ]);
  });

  it('parseJsonl one line one bool property', () => {
    const actual = parseJsonl('[["abc","bool", 1]]');
    expect(actual).toEqual([
      { "abc": { name: "abc", typeValue: "bool", value: true } },
    ]);
  });

  it('parseJsonl two lines', () => {
    const actual = parseJsonl('[["abc","str","def"]]\r\n[["abc","str","def"],["ghi","str","jkl"]]');
    expect(actual).toEqual([
      { "abc": { name: "abc", typeValue: "str", value: "def" } },
      { "abc": { name: "abc", typeValue: "str", value: "def" }, "ghi": { name: "ghi", typeValue: "str", value: "jkl" } },
    ]);
  });
});
