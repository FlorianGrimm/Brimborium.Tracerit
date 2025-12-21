import { Duration, ZonedDateTime } from "@js-joda/core";
import { FilterAstNode, FilterAstOperator, getLogLinePropertyByName, isPropertyValueEqual, LogLine, LogLineValue, TypeValue } from "../Api";

export type UIFilterAstNodeSelection = {
    id: string | undefined;
    property: 'operator' | 'value' | 'list' | undefined;
}

export const initialUIFilterAstNodeSelection: UIFilterAstNodeSelection = Object.freeze({
    id: undefined,
    index: undefined,
    property: undefined,
});

export type UIFilterAstNode = {
    id: string;
    operator: FilterAstOperator;
    listChild: UIFilterAstNode[] | undefined;
    value: LogLineValue | undefined;
}

export type OutputFilterAstNode = {
    nextNode: UIFilterAstNode | undefined;
    nextNodeRoot: UIFilterAstNode | undefined;
    nextSelection: UIFilterAstNodeSelection | undefined;
}

export type FilterOperatorsDisplayKind = "list" | "value";
export type FilterOperatorsDisplay = {
    operator: FilterAstOperator;
    display: string;
    kind: FilterOperatorsDisplayKind;
}

export const filterOperatorsListDisplay: FilterOperatorsDisplay[] = [
    { operator: "and", display: "and", kind: "list" },
    { operator: "or", display: "or", kind: "list" },
    { operator: "not", display: "not", kind: "list" },
];
export const filterOperatorsValueDisplay: FilterOperatorsDisplay[] = [
    { operator: "eq", display: "equals", kind: "value" },
    { operator: "ne", display: "not equals", kind: "value" },
    { operator: "lt", display: "less than", kind: "value" },
    { operator: "le", display: "less or equal", kind: "value" },
    { operator: "gt", display: "greater than", kind: "value" },
    { operator: "ge", display: "greater or equal", kind: "value" },
    { operator: "contains", display: "contains", kind: "value" },
    { operator: "startsWith", display: "starts with", kind: "value" },
];

export const mapFilterOperatorsDisplayByOperator = new Map<string, FilterOperatorsDisplay>(
    filterOperatorsListDisplay.concat(filterOperatorsValueDisplay)
        .map((item) => [item.operator, item]));


export function convertFilterAstNodeToUIFilterAstNode(node: FilterAstNode | null): UIFilterAstNode {
    if (node == null) {
        const uiNode: UIFilterAstNode = {
            id: crypto.randomUUID(),
            operator: "and",
            listChild: [],
            value: undefined,
        };
        return uiNode;
    } else {

        const uiNode: UIFilterAstNode = {
            id: crypto.randomUUID(),
            operator: node.operator,
            listChild: node.listChild?.map((child) => convertFilterAstNodeToUIFilterAstNode(child)).filter(item => item != null),
            value: node.value,
        };
        return uiNode;
    }
}

export function convertUIFilterAstNodeToFilterAstNode(node: UIFilterAstNode): FilterAstNode {
    const astNode: FilterAstNode = {
        operator: node.operator,
        listChild: node.listChild?.map((child) => convertUIFilterAstNodeToFilterAstNode(child)),
        value: node.value,
    };
    return astNode;
}

export function handleNodeChange(change: OutputFilterAstNode): UIFilterAstNode | undefined {
    return change.nextNode;
}

export function getUiNodeById(uiNode: UIFilterAstNode, id: string): (UIFilterAstNode | undefined) {
    if (uiNode.id == id) {
        return uiNode;
    }
    if (uiNode.listChild != null) {
        for (const child of uiNode.listChild) {
            const found = getUiNodeById(child, id);
            if (found != null) { return found; }
        }
    }
    return undefined;
}

export function getPathUiNodeById(uiNode: UIFilterAstNode, id: string): (UIFilterAstNode[] | undefined) {
    if (uiNode.id == id) {
        return [uiNode];
    }
    if (uiNode.listChild != null) {
        for (const child of uiNode.listChild) {
            const found = getPathUiNodeById(child, id);
            if (found != null) {
                found.push(uiNode);
                return found;
            }
        }
    }
    return undefined;
}

export type ReplaceUiNodeResult = { node: UIFilterAstNode, replaced: boolean };

export function replaceUiNode(uiNode: UIFilterAstNode, replaceNode: UIFilterAstNode): UIFilterAstNode {
    const result = replaceUiNodeInner(uiNode, replaceNode);
    if (result.replaced) {
        return result.node;
    } else {
        throw new Error('replaceUiNode failed');
    }
}

function replaceUiNodeInner(uiNode: UIFilterAstNode, replaceNode: UIFilterAstNode): ReplaceUiNodeResult {
    if (uiNode.id == replaceNode.id) {
        return { node: replaceNode, replaced: true };
    }
    if (uiNode.listChild != null) {
        for (let index = 0; index < uiNode.listChild.length; index++) {
            const result = replaceUiNodeInner(uiNode.listChild[index], replaceNode);
            if (result.replaced) {
                const newList = [...uiNode.listChild];
                newList[index] = result.node;
                const nextNode: UIFilterAstNode = {
                    ...uiNode,
                    listChild: newList,
                };
                return { node: nextNode, replaced: true };
            }
        }
    }
    return { node: uiNode, replaced: false };
}

export type ReplaceUiNodeFn = (node: UIFilterAstNode) => ReplaceUiNodeResult;

export function replaceUiNodeFn(uiNode: UIFilterAstNode, fnReplace: ReplaceUiNodeFn): UIFilterAstNode {
    const result = replaceUiNodeFnInner(uiNode, fnReplace);
    if (result.replaced) {
        return result.node;
    } else {
        throw new Error('replaceUiNodeFn failed');
    }
}

function replaceUiNodeFnInner(uiNode: UIFilterAstNode, fnReplace: ReplaceUiNodeFn): ReplaceUiNodeResult {
    const replaceResult = fnReplace(uiNode);
    if (replaceResult.replaced) {
        return replaceResult;
    }
    if (uiNode.listChild != null) {
        for (let index = 0; index < uiNode.listChild.length; index++) {
            const result = replaceUiNodeFnInner(uiNode.listChild[index], fnReplace);
            if (result.replaced) {
                const newList = uiNode.listChild.slice();
                newList[index] = result.node;
                const nextNode: UIFilterAstNode = {
                    ...uiNode,
                    listChild: newList,
                };
                return { node: nextNode, replaced: true };
            }
        }
    }
    return { node: uiNode, replaced: false };
}

export function getValidLogLineValue(typeValue: TypeValue, value: any | undefined) {
    if (value == null) { return undefined; }
    //"null" | "str" | "int" | "lvl" | "dt" | "dto" | "dur" | "bool" | "dbl" | "enum" | "uuid";
    if (typeValue == "null") {
        return undefined;
    }
    if (typeValue == "str") {
        if (typeof value != "string") { return undefined; }
        return value;
    }
    if (typeValue == "int") {
        if (typeof value != "number") { return undefined; }
        return value;
    }
    if (typeValue == "lvl") {
        if (typeof value != "string") { return undefined; }
        return value;
    }
    if (typeValue == "dt") {
        if (!(value instanceof ZonedDateTime)) { return undefined; }
        return value;
    }
    if (typeValue == "dto") {
        if (!(value instanceof ZonedDateTime)) { return undefined; }
        return value;
    }
    if (typeValue == "dur") {
        if (!(value instanceof Duration)) { return undefined; }
        return value;
    }
    if (typeValue == "bool") {
        if (typeof value != "boolean") { return undefined; }
        return value;
    }
    if (typeValue == "dbl") {
        if (typeof value != "number") { return undefined; }
        return value;
    }
    if (typeValue == "enum") {
        if (typeof value != "string") { return undefined; }
        return value;
    }
    if (typeValue == "uuid") {
        if (typeof value != "string") { return undefined; }
        return value;
    }
    return value;
}

export type FilterFunction = (logLine: LogLine) => boolean;
export function generateFilterFunction(node: FilterAstNode | null): FilterFunction {
    if (node == null) {
        return () => true;
    }
    switch (node.operator) {
        case "and":
            {
                const listChild = node.listChild;
                if (listChild == null) { return () => true; }
                if (listChild == undefined) { return () => true; }
                if (listChild.length == 0) { return () => true; }
                if (listChild.length == 1) { return generateFilterFunction(listChild[0]); }
                function opAnd(logLine: LogLine, listChildFn: FilterFunction[]) {
                    for (const childFn of listChildFn) {
                        if (childFn(logLine)) {
                            // continue
                        } else {
                            return false;
                        }
                    }
                    return true;
                }
                const listChildFn = listChild.map(item => generateFilterFunction(item));
                return (logLine) => opAnd(logLine, listChildFn);
            }
        case "or":
            {
                const listChild = node.listChild;
                if (listChild == null) { return () => true; }
                if (listChild.length == 0) { return () => true; }
                if (listChild.length == 1) { return generateFilterFunction(listChild[0]); }
                function opOr(logLine: LogLine, listChildFn: FilterFunction[]) {
                    for (const childFn of listChildFn) {
                        if (childFn(logLine)) {
                            return true;
                        } else {
                            // continue
                        }
                    }
                    return false;
                }
                const listChildFn = listChild.map(item => generateFilterFunction(item));
                return (logLine) => opOr(logLine, listChildFn);
            }
        case "not":
            {
                const listChild = node.listChild;
                if (listChild == null) { return () => true; }
                if (listChild.length == 0) { return () => true; }
                function opNot(logLine: LogLine, listChildFn: FilterFunction[]) {
                    return !listChildFn[0](logLine);
                }
                const listChildFn = listChild.map(item => generateFilterFunction(item));
                return (logLine) => opNot(logLine, listChildFn);
            }
        case "eq":
            {
                const value = node.value;
                if (value == null) { return () => true; }
                return (logLine) => {
                    const dataProperty = getLogLinePropertyByName(logLine, value.name);
                    if (dataProperty == null) { return false; }
                    return isPropertyValueEqual(dataProperty, value);
                };
            }
        case "ne":
            {
                const value = node.value;
                if (value == null) { return () => true; }
                return (logLine) => {
                    const dataProperty = getLogLinePropertyByName(logLine, value.name);
                    if (dataProperty == null) { return false; }
                    return !isPropertyValueEqual(dataProperty, value);
                };
            }
        case "lt":
        case "le":
        case "gt":
        case "ge":
        case "contains":
        case "startsWith":
            {
                throw new Error("Not implemented");
            }

        default:
            return () => true;
    }
}