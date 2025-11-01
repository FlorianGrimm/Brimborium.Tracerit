import { z } from 'zod';

export const LogFileInformationSchema = z.object({
    name: z.string(),
    creationTimeUtc: z.string(),
    length: z.number()    
});
export type LogFileInformation = z.infer<typeof LogFileInformationSchema>;

export const LogFileInformationListSchema = z.array(LogFileInformationSchema);
export type LogFileInformationList = z.infer<typeof LogFileInformationListSchema>;

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
export type MinimalPropertyBoolean = [propertyName: string, "bool", propertyValue: 0|1];
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

export function parseJsonl(content: string): MinimalLines {
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