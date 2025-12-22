import { PropertyHeader } from "../Api";

export function getVisualHeader(value:readonly PropertyHeader[]): readonly PropertyHeader[] {
    const listShown = value.filter(header => header.show);
    listShown.sort((a,b)=>a.visualHeaderIndex-b.visualHeaderIndex);
    return Object.freeze(listShown);
}
export function updateHeaderWidth(header: PropertyHeader, width: number): Partial<PropertyHeader> | null {
    if (header.width === width) { return null; }
    const nextHeader = { 
      width: width,
      headerCellStyle: { width: `${width}px` },
      dataCellStyle: { width: `${width}px` }
    };
    return nextHeader;
}

export function filterHeaderByName(list:readonly PropertyHeader[], name: string): readonly PropertyHeader[] {
    return list.filter(header => header.name === name);
}

