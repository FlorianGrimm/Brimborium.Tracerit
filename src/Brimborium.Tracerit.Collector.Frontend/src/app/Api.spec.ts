import { TestBed } from '@angular/core/testing';
import { parseJsonl } from './Api';

describe('App', () => {
  it('parseJsonl empty', () => {
    const actual = parseJsonl('');
    expect(actual).toEqual([]);
  });

  it('parseJsonl one line one property', () => {
    const actual = parseJsonl('[["abc","str","def"]]');
    expect(actual).toEqual([ [ ["abc", "str", "def"] ] ]);
  });

  
  it('parseJsonl two lines one property', () => {
    const actual = parseJsonl('[["abc","str","def"]]\r\n[["abc","str","def"]]');
    expect(actual).toEqual([ [ ["abc", "str", "def"] ],[ ["abc", "str", "def"] ] ]);
  });

});
