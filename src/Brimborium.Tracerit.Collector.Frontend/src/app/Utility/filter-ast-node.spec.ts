import { FilterAstNode, parseJsonl } from '@app/Api';
import { convertFilterAstNodeToUIFilterAstNode, generateFilterFunction } from './filter-ast-node';

describe('convertFilterAstNodeToUIFilterAstNode', () => {
  it('empty node', () => {
    const node = convertFilterAstNodeToUIFilterAstNode(null);
    expect(node.operator).toBe("and");
    expect(node.listChild).toEqual([]);
    expect(node.value).toBeUndefined();
  });
});



describe('generateFilterFunction', () => {
  it('empty node', () => {
    const fn = generateFilterFunction(null);
    const { listLogline, nextId } = parseJsonl('[["abc","str","def"]]', 1);
    expect(fn(listLogline[0])).toBe(true);
  });

  it('one string', () => {
    const filter: FilterAstNode = {
      operator: "eq",
      listChild: undefined,
      value: {
        name: "abc",
        typeValue: "str",
        value: "def",
      },
    };
    const fn = generateFilterFunction(filter);
    {
      const { listLogline, nextId } = parseJsonl('[["abc","str","def"]]', 1);
      expect(fn(listLogline[0])).toBe(true);
    }
    {
      const { listLogline, nextId } = parseJsonl('[["abc","str","wrong"]]', 1);
      // console.log(listLogline[0]);
      expect(fn(listLogline[0])).toBe(false);
    }
    
    {
      const { listLogline, nextId } = parseJsonl('[["def","str","def"]]', 1);
      // console.log(listLogline[0]);
      expect(fn(listLogline[0])).toBe(false);
    }

  });
});
