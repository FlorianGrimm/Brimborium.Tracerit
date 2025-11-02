import { object, z } from 'zod';
import { ZonedDateTime, Duration } from '@js-joda/core'

/*
export const LogFileInformationSchema = z.object({
    name: z.string(),
    creationTimeUtc: z.string(),
    length: z.number()
});
export type LogFileInformation = z.infer<typeof LogFileInformationSchema>;

export const LogFileInformationListSchema = z.array(LogFileInformationSchema);
export type LogFileInformationList = z.infer<typeof LogFileInformationListSchema>;

export const DirectoryBrowseResponseSchema = z.object({
    files: LogFileInformationListSchema
});
export type DirectoryBrowseResponse = z.infer<typeof DirectoryBrowseResponseSchema>;

export const ResponseDirectoryBrowseSuccessfulSchema = z.object({
    result: DirectoryBrowseResponseSchema
})
export const ResponseFailedSchema = z.object({
    error: z.string().or(z.any()),
    result: z.null().optional()
})
export const ResponseDirectoryBrowseSchema = ResponseDirectoryBrowseSuccessfulSchema.or(ResponseFailedSchema);
export type ResponseDirectoryBrowse = z.infer<typeof ResponseDirectoryBrowseSchema>;
*/

export type LogFileInformationList= LogFileInformation[];

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
    error: string;
    files?: undefined;
} 

export function parseDirectoryBrowse(value: any) : DirectoryBrowseResponse {
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



export type MinimalLines = MinimalRecord[];
export type MinimalRecord = MinimalProperty[];
export type MinimalProperty
    = MinimalPropertyString
    | MinimalPropertyInteger
    | MinimalPropertyLevelValue
    | MinimalPropertyDateTime
    | MinimalPropertyDateTimeOffset
    | MinimalPropertyDuration
    | MinimalPropertyBoolean
    | MinimalPropertyDouble
    | MinimalPropertyEnum
    | MinimalPropertyUuid
    | MinimalPropertyNull
    ;

export type MinimalPropertyString = [propertyName: string, "str", propertyValue: string];
export type MinimalPropertyInteger = [propertyName: string, "int", propertyValue: number];
export type MinimalPropertyLevelValue = [propertyName: string, "lvl", propertyValue: string];
export type MinimalPropertyDateTime = [propertyName: string, "dt", propertyValue: string];
export type MinimalPropertyDateTimeOffset = [propertyName: string, "dto", propertyValue: string];
export type MinimalPropertyDuration = [propertyName: string, "dur", propertyValue: number];
export type MinimalPropertyBoolean = [propertyName: string, "bool", propertyValue: 0 | 1];
export type MinimalPropertyDouble = [propertyName: string, "dbl", propertyValue: number];
export type MinimalPropertyEnum = [propertyName: string, "enum", propertyValue: string];
export type MinimalPropertyUuid = [propertyName: string, "uuid", propertyValue: string];
export type MinimalPropertyNull = [propertyName: string, "null", propertyValue?: null];

export type TypeNameAny = "any";
export type TypeNameString = "str";
export type TypeNameInteger = "int";
export type TypeNameLevelValue = "lvl";
export type TypeNameDateTime = "dt";
export type TypeNameDateTimeOffset = "dto";
export type TypeNameDuration = "dur";
export type TypeNameBoolean = "bool";
export type TypeNameDouble = "dbl";
export type TypeNameEnum = "enum";
export type TypeNameUuid = "uuid";
export type TypeNameNull = "null";


export const MinimalPropertyNullSchema = z.tuple([
    z.string(),
    z.literal("null"),
    z.null().optional()
]);

export const MinimalPropertyStringSchema = z.tuple([
    z.string(),
    z.literal("str"),
    z.string()
]);

export const MinimalPropertyIntegerSchema = z.tuple([
    z.string(),
    z.literal("int"),
    z.number()
]);

export const MinimalPropertyBooleanSchema = z.tuple([
    z.string(),
    z.literal("bool"),
    z.literal(0).or(z.literal(1))
]);

export const MinimalPropertyDoubleSchema = z.tuple([
    z.string(),
    z.literal("dbl"),
    z.number()
]);

export const MinimalPropertyLevelSchema = z.tuple([
    z.string(),
    z.literal("lvl"),
    z.string()
]);

export const MinimalPropertyEnumSchema = z.tuple([
    z.string(),
    z.literal("enum"),
    z.string()
]);

export const MinimalPropertyDateTimeSchema = z.tuple([
    z.string(),
    z.literal("dt"),
    z.string()
]);

export const MinimalPropertyDateTimeOffsetSchema = z.tuple([
    z.string(),
    z.literal("dto"),
    z.string()
]);

export const MinimalPropertyDurationSchema = z.tuple([
    z.string(),
    z.literal("dur"),
    z.number()
]);

export const MinimalPropertyUuidSchema = z.tuple([
    z.string(),
    z.literal("uuid"),
    z.string()
]);

export const MinimalPropertySchema = MinimalPropertyNullSchema
    .or(MinimalPropertyStringSchema)
    .or(MinimalPropertyIntegerSchema)
    .or(MinimalPropertyBooleanSchema)
    .or(MinimalPropertyDoubleSchema)
    .or(MinimalPropertyLevelSchema)
    .or(MinimalPropertyEnumSchema)
    .or(MinimalPropertyDateTimeSchema)
    // .or(MinimalPropertyDurationSchema)
    // .or(MinimalPropertyUuidSchema)
    ;
export const MinimalRecordSchema = z.array(
    MinimalPropertySchema
)

export function parseJsonlOld(content: string): MinimalLines {
    const result: MinimalLines = [];
    const lines = content.split(/\r\n|\n/);
    for (const lineText of lines) {
        try {
            const lineObj = JSON.parse(lineText);
            const lineResult = MinimalRecordSchema.safeParse(lineObj);
            if (lineResult.success) {
                result.push(lineResult.data);
            }
        } catch { }
    }

    return result;
}

export type LogLine = Record<string, LogLineValue>;
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
    for (const lineText of lines) {
        try {
            const lineObj = JSON.parse(lineText);
            if (!Array.isArray(lineObj)) { continue; }

            const itemResult: Record<string, LogLineValue> = {};
            for (const itemObj of lineObj) {
                if (!Array.isArray(itemObj)) { continue; }

                if (itemObj.length < 2) { continue; }
                const name = itemObj[0];
                const typeValue = itemObj[1];
                if (!("string" === typeof name && "string" === typeof typeValue)) { continue; }
                if ("null" == typeValue) { itemResult[name] = { name, typeValue, value: null }; continue; }

                if (itemObj.length < 3) { continue; }
                const value = itemObj[2];
                if ("str" == typeValue || "enum" == typeValue || "uuid" == typeValue) {
                    if (("string" === typeof value)) {
                        itemResult[name] = { name, typeValue, value };
                    }
                    continue;
                }
                if ("int" == typeValue || "dbl" == typeValue) {
                    if (("number" === typeof value)) {
                        itemResult[name] = { name, typeValue, value };
                    }
                    continue;
                }
                if ("lvl" == typeValue) {
                    if (("string" === typeof value) && ListLevelValue.includes(value as LevelValue)) {
                        itemResult[name] = { name, typeValue, value: value as LevelValue };
                    }
                    continue;
                }
                if ("dt" == typeValue) {
                    if (("string" === typeof value)) {
                        const dtValue = ZonedDateTime.parse(value);
                        itemResult[name] = { name, typeValue, value: dtValue };
                    }
                    continue;
                }
                if ("dto" == typeValue) {
                    if (("string" === typeof value)) {
                        const dtoValue = ZonedDateTime.parse(value);
                        itemResult[name] = { name, typeValue, value: dtoValue };
                    }
                    continue;
                }
                if ("dur" == typeValue) {
                    if (("number" === typeof value)) {
                        const durValue = Duration.ofNanos(value);
                        itemResult[name] = { name, typeValue, value: durValue };
                        continue;
                    }
                }
                if ("bool" == typeValue) {
                    if (("number" === typeof value)) {
                        itemResult[name] = { name, typeValue, value: (value != 0) };
                        continue;
                    }
                }
                continue;
            }
            result.push(itemResult);

        } catch { }
    }

    return result;
}