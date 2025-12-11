import { PropertyHeader } from "../Api";

export function getVisualHeader(value: PropertyHeader[]): PropertyHeader[] {
    const listShown = value.filter(header => header.show);
    listShown.sort((a,b)=>a.visualHeaderIndex-b.visualHeaderIndex);
    return listShown;
}