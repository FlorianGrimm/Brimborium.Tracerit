import { filterHeaderByName } from './propertyHeaderUtility';

describe('filterHeaderByName', () => {
  it('filter empty', () => {
    expect(filterHeaderByName([], "")).toStrictEqual([]);
  });
});
