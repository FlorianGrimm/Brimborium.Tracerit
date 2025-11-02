import { TestBed } from '@angular/core/testing';
import { parseJsonl, parseJsonlOld } from './Api';

describe('App', () => {
  it('parseJsonl empty', () => {
    const actual = parseJsonlOld('');
    expect(actual).toEqual([]);
  });

  it('parseJsonl one line one property', () => {
    const actual = parseJsonlOld('[["abc","str","def"]]');
    expect(actual).toEqual([[["abc", "str", "def"]]]);
  });


  it('parseJsonl old two lines one property', () => {
    const actual = parseJsonlOld('[["abc","str","def"]]\r\n[["abc","str","def"]]');
    expect(actual).toEqual([[["abc", "str", "def"]], [["abc", "str", "def"]]]);
  });


  it('parseJsonl two lines one property', () => {
    debugger;
    const actual = parseJsonl('[["abc","str","def"]]\r\n[["abc","str","def"]]');
    console.log("actual",actual);
    expect(actual).toEqual([
      { "abc": { name: "abc", typeValue: "str", value: "def" } },
      { "abc": { name: "abc", typeValue: "str", value: "def" } },
    ]);
  });
});
