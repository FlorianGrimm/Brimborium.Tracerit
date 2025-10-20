﻿using Brimborium.Tracerit.FileSink;
using System.IO.Compression;

namespace Brimborium.Tracerit;

public static class TracorDataSerialization {
    public static JsonSerializerOptions GetMinimalJsonSerializerOptions(
        JsonSerializerOptions? options,
        TracorDataRecordPool? tracorDataRecordPool
        ) {
        options ??= new();
        options.Converters.Add(new TracorDataJsonMinimalConverterFactory(tracorDataRecordPool));
        options.Converters.Add(new TracorDataRecordMinimalJsonConverter(tracorDataRecordPool));
        options.Converters.Add(new TracorDataPropertyMinimalJsonConverter());

        /*
        result.Converters.Add(new TracorDataJsonSimpleConverterFactory());
        result.Converters.Add(new TracorDataRecordSimpleJsonConverter());
        result.Converters.Add(new TracorDataPropertySimpleJsonConverter());
        */
        return options;
    }


    public static TracorDataRecordCollection DeserializeSimple(
        string json,
        System.Text.Json.JsonSerializerOptions? options = null) {
        var usedOptions = GetMinimalJsonSerializerOptions(options, null);
        var listTracorDataRecord = System.Text.Json.JsonSerializer.Deserialize<List<TracorDataRecord>>(
            json,
            usedOptions);
        TracorDataRecordCollection result = new();
        if (listTracorDataRecord is not null) {
            result.ListData.AddRange(listTracorDataRecord);
        }
        return result;
    }

    public static string SerializeSimple(
        IEnumerable<ITracorData> value,
        System.Text.Json.JsonSerializerOptions? options = null) {
        var usedOptions = GetMinimalJsonSerializerOptions(options, null);
        var json = System.Text.Json.JsonSerializer.Serialize<IEnumerable<ITracorData>>(value, usedOptions);
        return json;
    }

    public static TracorDataRecordCollection ToTracorDataRecordCollection(
        List<ITracorData> srcList) {
        TracorDataRecordCollection result = new();
        foreach (ITracorData tracorData in srcList) {
            if (tracorData is TracorDataRecord tracorDataRecord) {
                tracorDataRecord.IncrementReferenceCount();
                result.ListData.Add(tracorDataRecord);
            } else {
                tracorDataRecord = TracorDataRecord.Convert(tracorData);
                result.ListData.Add(tracorDataRecord);
            }
        }
        return result;
    }

    public static Stream? GetReadStream(
        string logfile) {
        var compression = FileTracorCollectiveSink.GetCompressionFromFileName(logfile);
        if (compression is { } compressionValue) {
            var fileStream = System.IO.File.Open(logfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (FileTracorCollectiveCompression.Gzip == compression) {
                return new GZipStream(fileStream, new ZLibCompressionOptions() { }, leaveOpen: false);
            }
            if (FileTracorCollectiveCompression.Brotli == compression) {
                return new BrotliStream(fileStream, new BrotliCompressionOptions() { }, leaveOpen: false);
            }
            return fileStream;
        } else {
            return default;
        }
    }

    public static async Task DeserializeMinimalJsonlCallbackAsync(
        string logfile,
        Action<TracorDataRecord> callback,
        JsonSerializerOptions options,
        CancellationToken cancellationToken) {
        using (var utf8Json = TracorDataSerialization.GetReadStream(logfile)) {
            if (utf8Json is null) { throw new Exception("Cannot open file"); }
            using (var splitStream = new Brimborium.JSONLines.SplitStream(utf8Json, false)) {
                while (splitStream.MoveNextStream()) {
                    var item = await System.Text.Json.JsonSerializer.DeserializeAsync<TracorDataRecord>(
                        splitStream,
                        options,
                        cancellationToken);
                    if (item is { }) {
                        callback(item);
                    }
                }
            }
        }
    }

#if later

    public static string ConvertToMinimizeStringJson(
        TracorDataRecordCollection tracorDataCollection,
        System.Text.Json.JsonSerializerOptions? options = null) {
        var converted = ConvertTracorDataRecordCollectionToMinimizeString(
            tracorDataCollection.ListData);
        return System.Text.Json.JsonSerializer.Serialize(converted, options);
    }

    public static string ConvertToMinimizeStringJson(
        TracorDataCollection tracorDataCollection,
        System.Text.Json.JsonSerializerOptions? options = null) {
        var converted = ToTracorDataCollectionMinimizeString(
            tracorDataCollection.ListData);
        return System.Text.Json.JsonSerializer.Serialize(converted, options);
    }


    public static string ConvertToMinimizeStringJson(
        List<ITracorData> listData,
        System.Text.Json.JsonSerializerOptions? options = null) {
        var converted = ToTracorDataCollectionMinimizeString(listData);
        return System.Text.Json.JsonSerializer.Serialize(converted, options);
    }

    public static List<List<string>> ConvertTracorDataRecordCollectionToMinimizeString(
        List<TracorDataRecord> listTracorData) {
        List<List<string>> dstTracorDataCollection = new();
        TracorDataRecord dstTracorData = new(null);
        StringBuilder sbHelper = new();

        foreach (var tracorData in listTracorData) {
            List<string> dstTracorDataMinimizeString = new();
            ToTracorDataMinimizeString(tracorData, dstTracorDataMinimizeString, sbHelper);
            dstTracorData.ListProperty.Clear();
            if (0 < dstTracorDataMinimizeString.Count) {
                dstTracorDataCollection.Add(dstTracorDataMinimizeString);
            }
        }

        return dstTracorDataCollection;
    }
    public static List<List<string>> ToTracorDataCollectionMinimizeString(
        List<ITracorData> listTracorData) {
        List<List<string>> dstTracorDataCollection = new();
        TracorDataRecord dstTracorData = new(null);
        StringBuilder sbHelper = new();

        foreach (var tracorData in listTracorData) {
            List<string> dstTracorDataMinimizeString = new();
            ToTracorDataMinimizeString(tracorData, dstTracorDataMinimizeString, sbHelper);
            dstTracorData.ListProperty.Clear();
            if (0 < dstTracorDataMinimizeString.Count) {
                dstTracorDataCollection.Add(dstTracorDataMinimizeString);
            }
        }

        return dstTracorDataCollection;
    }

    public static void ToTracorDataMinimizeString(
        ITracorData srcTracorData,
        List<string> dstTracorDataMinimizeString,
        StringBuilder sbHelper) {
        List<TracorDataProperty> listProperty = new(128);
        srcTracorData.ConvertProperties(listProperty);
        foreach (var itemProperty in listProperty) {
            itemProperty.ToMinimizeString(sbHelper);
            dstTracorDataMinimizeString.Add(sbHelper.ToString());
            sbHelper.Clear();
        }
    }

    public static TracorDataRecordCollection ParseTracorDataRecordCollectionCollection(string? json) {
        TracorDataRecordCollection result = new();
        if (json is null || 0 == json.Length) {
            return result;
        }

        {
            var srcTracorDataCollection = System.Text.Json.JsonSerializer.Deserialize<List<List<string>>>(json);
            if (srcTracorDataCollection is null) { return result; }

            foreach (var srcTracorData in srcTracorDataCollection) {
                var tracorDataRecord = ParseTracorData(srcTracorData);
                result.ListData.Add(tracorDataRecord);
            }

            return result;
        }
    }

    public static TracorDataRecord ParseTracorData(List<string> srcTracorData) {
        TracorDataRecord result = new(null);
        int state = 0;
        string? tracorIdentitfierSource = null;
        string? tracorIdentitfierScope = null;

        int idx = 1;
        for (; idx < srcTracorData.Count; idx++) {
            string itemTracorDataProperty = srcTracorData[idx];
            var success = ParseTracorDataPropertyMinimizeString(itemTracorDataProperty, out var dstTracorDataProperty);
            if (!success) { continue; }

            if (state < 4) {
                // state = 0: before Operation
                // state = 1: after Operation
                //if (0 == state) {
                //    if (nameof(TracorDataRecord.Operation) == dstTracorDataProperty.Name) {
                //        state = 1;
                //        if (TracorDataProperty.TypeNameString == dstTracorDataProperty.TypeName) {
                //            result.Operation = TracorDataUtility.ParseTracorDataRecordOperation(
                //                dstTracorDataProperty.TextValue);
                //            continue;
                //        }
                //    } else {
                //        state = 1;
                //    }
                //}

                // state 1 : after Operation
                // state 2 : after Source
                if (1 == state) {
                    if (nameof(TracorIdentitfier.Source) == dstTracorDataProperty.Name) {
                        state = 2;
                        if (TracorDataProperty.TypeNameString == dstTracorDataProperty.TypeName) {
                            tracorIdentitfierSource = dstTracorDataProperty.InnerTextValue;
                            continue;
                        }
                    }
                }

                // state 2 : after Source
                // state 3 : after Scope
                if (state is 1 or 2) {
                    if (nameof(TracorIdentitfier.Scope) == dstTracorDataProperty.Name) {
                        state = 3;
                        if (TracorDataProperty.TypeNameString == dstTracorDataProperty.TypeName) {
                            tracorIdentitfierScope = dstTracorDataProperty.InnerTextValue;
                            continue;
                        }
                    }
                }
                if (state is 1 or 2 or 3) { state = 4; }
            }

            {
                result.ListProperty.Add(dstTracorDataProperty);
            }
        }

        result.TracorIdentitfier = new(
            tracorIdentitfierSource ?? string.Empty,
            tracorIdentitfierScope ?? string.Empty);

        return result;
    }

    private const char _SeperationJsonChar = ':';

    public static bool ParseTracorDataPropertyMinimizeString(ReadOnlySpan<char> value, out TracorDataProperty result) {
        int posNameColon = value.IndexOf(_SeperationJsonChar);
        if (1 <= posNameColon && posNameColon < value.Length) {
            var argName = value[..posNameColon];
            int posAfterNameColon = posNameColon + 1;
            if (posAfterNameColon < value.Length) {
                var valueAfterNameColon = value[posAfterNameColon..];
                int posTypeColon = valueAfterNameColon.IndexOf(_SeperationJsonChar);
                var posAfterTypeColon = posTypeColon + 1;
                if (0 < posTypeColon && posAfterTypeColon <= valueAfterNameColon.Length) {
                    var typeName = valueAfterNameColon[..posTypeColon];
                    var textValue = valueAfterNameColon[posAfterTypeColon..];

                    {
                        var argNameString = TracorDataUtility.GetPropertyName(argName);
                        var textValueString = TracorDataUtility.GetPropertyValue(textValue);
                        if (TracorDataProperty.TypeNameString.AsSpan().SequenceEqual(typeName)) {
                            result = new TracorDataProperty(
                                name: argNameString,
                                typeValue: TracorDataPropertyTypeValue.String,
                                textValue: textValueString
                            );
                            return true;
                        } else if (TracorDataProperty.TypeNameInteger.AsSpan().SequenceEqual(typeName)) {
                            if (int.TryParse(textValue, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out var intValue)) {
                                result = new TracorDataProperty(
                                    name: argNameString,
                                    typeValue: TracorDataPropertyTypeValue.Integer,
                                    textValue: textValueString
                                ) {
                                    InnerLongValue = intValue
                                };
                                return true;
                            }
                        } else if (TracorDataProperty.TypeNameLevelValue.AsSpan().SequenceEqual(typeName)) {
                            if (LogLevelUtility.TryGetLogLevelByName(textValueString, out var level)) {
                                result = new TracorDataProperty(
                                    name: argNameString,
                                    typeValue: TracorDataPropertyTypeValue.LevelValue,
                                    textValue: textValueString
                                ) {
                                    InnerLongValue = (long)level
                                };
                                return true;
                            } else if (int.TryParse(textValue, out var intValue)) {
                                result = new TracorDataProperty(
                                    name: argNameString,
                                    typeValue: TracorDataPropertyTypeValue.LevelValue,
                                    textValue: textValueString
                                ) {
                                    InnerLongValue = intValue
                                };
                                return true;
                            }
                        } else if (TracorDataProperty.TypeNameDateTime.AsSpan().SequenceEqual(typeName)) {
                            if (long.TryParse(textValue, null, out var ns)) {
                                result = new TracorDataProperty(
                                    name: argNameString,
                                    typeValue: TracorDataPropertyTypeValue.DateTime,
                                    textValue: textValueString
                                ) {
                                    InnerLongValue = ns,
                                    InnerFloatValue = 0
                                };
                                return true;
                            } else if (DateTime.TryParseExact(textValue, "O", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.None, out var dtValue)) {
                                result = new TracorDataProperty(
                                    name: argNameString,
                                    typeValue: TracorDataPropertyTypeValue.DateTime,
                                    textValue: textValueString
                                ) {
                                    InnerLongValue = TracorDataUtility.DateTimeToUnixTimeNanoseconds(dtValue)
                                };
                                return true;
                            }
                        } else if (TracorDataProperty.TypeNameDateTimeOffset.AsSpan().SequenceEqual(typeName)) {
                            if (long.TryParse(textValue, null, out var ns)) {
                                result = new TracorDataProperty(
                                    name: argNameString,
                                    typeValue: TracorDataPropertyTypeValue.DateTimeOffset,
                                    textValue: textValueString
                                ) {
                                    InnerLongValue = ns
                                };
                                return true;
                            } else if (DateTimeOffset.TryParseExact(textValue, "O", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.None, out var dtoValue)) {
                                var dto = TracorDataUtility.DateTimeOffsetToUnixTimeNanosecondsAndOffset(dtoValue);
                                result = new TracorDataProperty(
                                    name: argNameString,
                                    typeValue: TracorDataPropertyTypeValue.DateTimeOffset,
                                    textValue: textValueString
                                ) {
                                    InnerLongValue = dto.longVaule,
                                    InnerFloatValue = dto.floatValue
                                };
                                return true;
                            }
                        } else if (TracorDataProperty.TypeNameBoolean.AsSpan().SequenceEqual(typeName)) {
                            var boolValue = TracorDataUtility.GetBoolValue(textValueString);
                            result = new TracorDataProperty(
                                name: argNameString,
                                typeValue: TracorDataPropertyTypeValue.Boolean,
                                textValue: textValueString
                            ) {
                                InnerLongValue = boolValue ? 1 : 0,
                                InnerObjectValue = TracorDataUtility.GetBoolValueBoxes(boolValue)
                            };
                            return true;
                        } else if (TracorDataProperty.TypeNameInteger.AsSpan().SequenceEqual(typeName)) {
                            if (long.TryParse(textValueString, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out var longValue)) {
                                result = new TracorDataProperty(
                                    name: argNameString,
                                    typeValue: TracorDataPropertyTypeValue.Integer,
                                    textValue: textValueString
                                ) {
                                    InnerLongValue = longValue
                                };
                                return true;
                            }
                        } else if (TracorDataProperty.TypeNameFloat.AsSpan().SequenceEqual(typeName)) {
                            if (double.TryParse(textValueString, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out var floatValue)) {
                                result = new TracorDataProperty(
                                    name: argNameString,
                                    typeValue: TracorDataPropertyTypeValue.Float,
                                    textValue: textValueString
                                ) {
                                    InnerFloatValue = floatValue
                                };
                                return true;
                            }
                        } else if (TracorDataProperty.TypeNameAny.AsSpan().SequenceEqual(typeName)) {
                            result = new TracorDataProperty(
                                name: argNameString,
                                typeValue: TracorDataPropertyTypeValue.Any,
                                textValue: textValueString
                            );
                            return true;
                        }
                    }
                }
            }
        }

        // fallback - error
        {
            result = new(
                name: string.Empty,
                typeValue: TracorDataPropertyTypeValue.Any,
                textValue: string.Empty
                );
            return false;
        }
    }
#endif
}