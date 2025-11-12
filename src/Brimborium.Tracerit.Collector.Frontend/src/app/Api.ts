import { ZonedDateTime, Duration, LocalDateTime, ZoneId, ZoneOffset } from '@js-joda/core'

/* DirectoryBrowse */

export type LogFileInformationList = LogFileInformation[];

export type LogFileInformation = {
    name: string,
    creationTimeUtc: ZonedDateTime,
    length: number
};

export type DirectoryBrowseResponse = {
    mode: "success";
    files: LogFileInformation[];
    error?: undefined;
} | {
    mode: "error";
    files?: undefined;
    error: string;
}

export function parseDirectoryBrowse(value: any): DirectoryBrowseResponse {
    if ("object" === typeof value) {
        {
            const value_result = value.result;
            if ("object" === typeof value_result) {
                const value_result_files = value_result.files;
                if (Array.isArray(value_result_files)) {
                    const files: LogFileInformation[] = [];
                    for (const file of value_result_files) {
                        const file_name = file.name;
                        const file_creationTimeUtc = file.creationTimeUtc;
                        const file_length = file.length;
                        if (!(("string" === typeof file_name)
                            && ("string" === typeof file_creationTimeUtc)
                            && ("number" === typeof file_length))) {
                            continue;
                        }
                        const item: LogFileInformation = {
                            name: file_name,
                            creationTimeUtc: ZonedDateTime.parse(file_creationTimeUtc),
                            length: file_length
                        };
                        files.push(item);
                    }

                    return ({ mode: "success", files: files });
                }
            }
        }
        {
            const value_result = value.error;
            if ("string" === typeof value_result) {
                return ({ mode: "error", error: value_result });
            }
        }
    }
    return ({ mode: "error", error: "error" });
}

/* GetFile */

export type GetFileResponse = {
    mode: "success";
    data: LogLine[];
    error?: undefined;
} | {
    mode: "error";
    data?: undefined;
    error: string;
}

export type LogLine = {
    id: number
    data: Map<string, LogLineValue>;
};
export type LogLineValue = LogLineNull
    | LogLineString
    | LogLineInteger
    | LogLineLevelValue
    | LogLineDateTime
    | LogLineDateTimeOffset
    | LogLineDuration
    | LogLineBoolean
    | LogLineDouble
    | LogLineEnum
    | LogLineUuid
    ;

export type TypeValue = "null" | "str" | "int" | "lvl" | "dt" | "dto" | "dur" | "bool" | "dbl" | "enum" | "uuid";

export type LevelValue = "trace" | "debug" | "information" | "warning" | "error" | "critical" | "none";
export const ListLevelValue: LevelValue[] = ["trace", "debug", "information", "warning", "error", "critical", "none"];

export type LogLineNull = { name: string; typeValue: "null"; value?: undefined | null; };
export type LogLineString = { name: string; typeValue: "str"; value: string; };
export type LogLineInteger = { name: string; typeValue: "int"; value: number; };
export type LogLineLevelValue = { name: string, typeValue: "lvl", value: LevelValue };
export type LogLineDateTime = { name: string, typeValue: "dt", value: ZonedDateTime };
export type LogLineDateTimeOffset = { name: string, typeValue: "dto", value: ZonedDateTime };
export type LogLineDuration = { name: string, typeValue: "dur", value: Duration };
export type LogLineBoolean = { name: string, typeValue: "bool", value: boolean };
export type LogLineDouble = { name: string, typeValue: "dbl", value: number };
export type LogLineEnum = { name: string, typeValue: "enum", value: string };
export type LogLineUuid = { name: string, typeValue: "uuid", value: string };

export function parseJsonl(content: string): LogLine[] {
    const result: LogLine[] = [];
    const lines = content.split(/\r\n|\n/);
    let id = 1;
    for (const lineText of lines) {
        try {
            const lineObj = JSON.parse(lineText);
            if (!Array.isArray(lineObj)) { continue; }

            const itemResult: LogLine = {
                id: id,
                data: new Map<string, LogLineValue>(),
            };
            for (const itemObj of lineObj) {
                if (!Array.isArray(itemObj)) { continue; }

                if (itemObj.length < 2) { continue; }
                const name = itemObj[0];
                const typeValue = itemObj[1];
                if (!("string" === typeof name && "string" === typeof typeValue)) { continue; }
                if ("null" == typeValue) { itemResult.data.set(name, { name, typeValue, value: null }); continue; }

                if (itemObj.length < 3) { continue; }
                const value = itemObj[2];
                if ("str" == typeValue || "enum" == typeValue || "uuid" == typeValue) {
                    if (("string" === typeof value)) {
                        itemResult.data.set(name, { name, typeValue, value });
                    }
                    continue;
                }
                if ("int" == typeValue || "dbl" == typeValue) {
                    if (("number" === typeof value)) {
                        itemResult.data.set(name, { name, typeValue, value });
                    }
                    continue;
                }
                if ("lvl" == typeValue) {
                    if (("string" === typeof value) && ListLevelValue.includes(value as LevelValue)) {
                        itemResult.data.set(name, { name, typeValue, value: value as LevelValue });
                    }
                    continue;
                }
                if ("dt" == typeValue) {
                    if (("string" === typeof value)) {
                        let localDateTime: LocalDateTime
                        if (value.endsWith("Z")) {
                            const dto = ZonedDateTime.parse(value);
                            localDateTime = dto.toLocalDateTime();
                        } else {
                            localDateTime = LocalDateTime.parse(value);
                        }
                        const dtValue = ZonedDateTime.of(localDateTime, ZoneId.UTC);
                        itemResult.data.set(name, { name, typeValue, value: dtValue });
                    }
                    continue;
                }
                if ("dto" == typeValue) {
                    if (("string" === typeof value)) {
                        const dtoValue = ZonedDateTime.parse(value);
                        itemResult.data.set(name, { name, typeValue, value: dtoValue });
                    }
                    continue;
                }
                if ("dur" == typeValue) {
                    if (("number" === typeof value)) {
                        const durValue = Duration.ofNanos(value);
                        itemResult.data.set(name, { name, typeValue, value: durValue });
                        continue;
                    }
                }
                if ("bool" == typeValue) {
                    if (("number" === typeof value)) {
                        itemResult.data.set(name, { name, typeValue, value: (value != 0) });
                        continue;
                    }
                }
                continue;
            }

            id++;
            result.push(itemResult);
        } catch { }
    }

    return result;
}

export type PropertyHeader = {
    id: string;
    name: string;
    typeValue: TypeValue;
    index: number;

    visualIndex: number;
    show: boolean;
    filter?: LogLineValue;

    headerCellStyle?: any;
    dataCellStyle?: any;
};

export function getLogLinePropertyByName(
    logLine: LogLine,
    name: string, typeValue?: TypeValue): (LogLineValue | undefined) {
    const property = logLine.data.get(name);
    if ((undefined !== property)
        && ((undefined === typeValue) || (typeValue === property.typeValue))) {
        return property;
    }
    return undefined;
}

export function getLogLineTimestampValue(
    logLine: LogLine | null | undefined
): (ZonedDateTime | null) {
    if (undefined === logLine || null === logLine) { return null; }
    const property = logLine.data.get("timestamp");
    if ((undefined !== property)
        && (("dt" === property.typeValue) || ("dto" === property.typeValue))) {
        return property.value;
    }
    return null;
}

export function filterListLogLine(
    listLogLine: LogLine[] | null | undefined,
    listHeader: PropertyHeader[]) {
    const result: LogLine[] = [];
    if (undefined === listLogLine || null === listLogLine) { return result; }
    const listFilteredHeader: PropertyHeader[] = [];
    for (const header of listHeader) {
        if (undefined === header.filter) { continue; }
        listFilteredHeader.push(header);
    }

    if (0 === listFilteredHeader.length) { return listLogLine.slice(); }

    for (const logLine of listLogLine) {
        let add = true;
        for (const header of listFilteredHeader) {
            const dataProperty = logLine.data.get(header.name);
            if (undefined === dataProperty) { add = false; break; }
            if (undefined === header.filter) { continue; }

            if (!isPropertyValueEqual(dataProperty, header.filter!)) { add = false; break; }
        }
        if (add) { result.push(logLine); }
    }
    return result;
}

export function isPropertyValueEqual(
    dataProperty: LogLineValue,
    filter: LogLineValue) {

    if (dataProperty.typeValue !== filter.typeValue) { return false; }

    switch (dataProperty.typeValue) {
        case "str": return dataProperty.value.startsWith(filter.value as string);
        case "int": return dataProperty.value == filter.value;
        case "lvl": return dataProperty.value == filter.value;
        case "dt": return dataProperty.value == filter.value;
        case "dto": return dataProperty.value == filter.value;
        case "dur": return dataProperty.value == filter.value;
        case "bool": return dataProperty.value == filter.value;
        case "dbl": return dataProperty.value == filter.value;
        case "enum": return dataProperty.value == filter.value;
        case "uuid": return dataProperty.value == filter.value;
        default: return false;
    }
}