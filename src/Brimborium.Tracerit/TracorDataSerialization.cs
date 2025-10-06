namespace Brimborium.Tracerit;

public static class TracorDataSerialization {
    public static TracorDataCollection ToTracorDataCollection(
        List<TracorIdentitfierData> srcListTracorIdentitfierData) {
        TracorDataCollection result = new();
        foreach (var itemData in srcListTracorIdentitfierData) {
            TracorDataRecord tracorData = new(null);
            ToTracorData(itemData, tracorData);
            result.ListData.Add(tracorData);
        }
        return result;
    }

    public static void ToTracorData(TracorIdentitfierData that, TracorDataRecord result) {
        if (that.TracorIdentitfier.Source is string valueSource) {
            result.ListProperty.Add(TracorDataProperty.CreateString(nameof(TracorIdentitfier.Source), valueSource));
        }
        if (that.TracorIdentitfier.Scope is string valueCallee) {
            result.ListProperty.Add(TracorDataProperty.CreateString(nameof(TracorIdentitfier.Scope), valueCallee));
        }
        that.TracorData.ConvertProperties(result.ListProperty);
    }

    public static string ToTracorDataCollectionJson(
        List<TracorIdentitfierData> listData,
        System.Text.Json.JsonSerializerOptions? options = null) {
        var converted = ToTracorDataCollectionMinimizeString(listData);
        return System.Text.Json.JsonSerializer.Serialize(converted, options);
    }

    public static List<List<string>> ToTracorDataCollectionMinimizeString(
        List<TracorIdentitfierData> listData) {
        List<List<string>> dstTracorDataCollection = new();
        TracorDataRecord dstTracorData = new(null);
        StringBuilder sbHelper = new();

        foreach (var itemData in listData) {
            List<string> dstTracorDataMinimizeString = new();
            ToTracorData(itemData, dstTracorData);
            ToTracorDataMinimizeString(dstTracorData, dstTracorDataMinimizeString, sbHelper);
            dstTracorData.ListProperty.Clear();
            if (0 < dstTracorDataMinimizeString.Count) {
                dstTracorDataCollection.Add(dstTracorDataMinimizeString);
            }
        }

        return dstTracorDataCollection;
    }

    public static void ToTracorDataMinimizeString(
        TracorDataRecord srcTracorData,
        List<string> dstTracorDataMinimizeString,
        StringBuilder sbHelper) {
        foreach (var itemProperty in srcTracorData.ListProperty) {
            itemProperty.ToMinimizeString(sbHelper);
            dstTracorDataMinimizeString.Add(sbHelper.ToString());
            sbHelper.Clear();
        }
    }

    public static TracorDataCollection ParseTracorDataCollection(string? json) {
        TracorDataCollection result = new();
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
            var success = ParseTracorDataProperty(itemTracorDataProperty, out var dstTracorDataProperty);
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
                            tracorIdentitfierSource = dstTracorDataProperty.TextValue;
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
                            tracorIdentitfierScope = dstTracorDataProperty.TextValue;
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

    public static bool ParseTracorDataProperty(ReadOnlySpan<char> value, out TracorDataProperty result) {
        int posNameColon = value.IndexOf(_SeperationJsonChar);
        if (1 <= posNameColon && posNameColon <value.Length) {
            var argName = value[..posNameColon];
            int posAfterNameColon = posNameColon + 1;
            if (posAfterNameColon < value.Length) {
                var valueAfterNameColon = value[posAfterNameColon..];
                int posOperationColon = valueAfterNameColon.IndexOf(_SeperationJsonChar);
                var posAfterOperationColon = posOperationColon + 1;
                if (0 <= posOperationColon && posAfterOperationColon <= valueAfterNameColon.Length) {
                    var valueAfterOperationColon = valueAfterNameColon[posAfterOperationColon..];
                    int posTypeColon = valueAfterOperationColon.IndexOf(_SeperationJsonChar);
                    var posAfterTypeColon = posTypeColon + 1;
                    if (0 < posTypeColon && posAfterTypeColon <= valueAfterOperationColon.Length) {
                        var typeName = valueAfterOperationColon[..posTypeColon];
                        var textValue = valueAfterOperationColon[posAfterTypeColon..];

                        {
                            var argNameString = TracorDataUtility.GetPropertyName(argName);
                            var textValueString = TracorDataUtility.GetPropertyValue(textValue);
                            if (TracorDataProperty.TypeNameString.AsSpan().SequenceEqual(typeName)) {
                                result = new TracorDataProperty(
                                    name: argNameString,
                                    typeValue: TracorDataPropertyTypeValue.String,
                                    textValue: textValueString
                                    #warning TODO value: textValueString
                                );
                                return true;
                            } else if (TracorDataProperty.TypeNameInteger.AsSpan().SequenceEqual(typeName)) {
                                if (int.TryParse(textValue, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out var intValue)) {
                                    result = new TracorDataProperty(
                                        name: argNameString,
                                        typeValue: TracorDataPropertyTypeValue.Integer,
                                        textValue: textValueString
#warning TODO                                         value: intValue
                                    );
                                    return true;
                                }
                            } else if (TracorDataProperty.TypeNameLevelValue.AsSpan().SequenceEqual(typeName)) {
                                if (LogLevelUtility.TryGetLogLevelByName(textValueString, out var level)) {
                                    result = new TracorDataProperty(
                                        name: argNameString,
                                        typeValue: TracorDataPropertyTypeValue.LevelValue,
                                        textValue: textValueString
#warning TODO                                         value: level
                                    );
                                    return true;
                                } else if (int.TryParse(textValue, out var intValue)) {
                                    result = new TracorDataProperty(
                                        name: argNameString,
                                        typeValue: TracorDataPropertyTypeValue.LevelValue,
                                        textValue: textValueString
#warning TODO value: (LogLevel)intValue
                                    );
                                    return true;
                                }
                            } else if (TracorDataProperty.TypeNameDateTime.AsSpan().SequenceEqual(typeName)) {
                                if (long.TryParse(textValue, null, out var ns)) {
                                    result = new TracorDataProperty(
                                        name: argNameString,
                                        typeValue: TracorDataPropertyTypeValue.DateTime,
                                        textValue: textValueString
#warning TODO                                         value: TracorDataUtility.UnixTimeNanosecondsToDateTime(ns)
                                    );
                                    return true;
                                } else if (DateTime.TryParseExact(textValue, "O", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.None, out var dtValue)) {
                                    result = new TracorDataProperty(
                                        name: argNameString,
                                        typeValue: TracorDataPropertyTypeValue.DateTime,
                                        textValue: textValueString
#warning TODO                                         value: dtValue
                                    );
                                    return true;
                                }
                            } else if (TracorDataProperty.TypeNameDateTimeOffset.AsSpan().SequenceEqual(typeName)) {
                                if (long.TryParse(textValue, null, out var ns)) {
                                    result = new TracorDataProperty(
                                        name: argNameString,
                                        typeValue: TracorDataPropertyTypeValue.DateTimeOffset,
                                        textValue: textValueString
#warning TODO                                         value: TracorDataUtility.UnixTimeNanosecondsToDateTimeOffset(ns)
                                    );
                                    return true;
                                } else if (DateTimeOffset.TryParseExact(textValue, "O", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.None, out var dtoValue)) {
                                    result = new TracorDataProperty(
                                        name: argNameString,
                                        typeValue: TracorDataPropertyTypeValue.DateTime,
                                        textValue: textValueString
#warning TODO                                         value: dtoValue
                                    );
                                    return true;
                                }
                            } else if (TracorDataProperty.TypeNameBoolean.AsSpan().SequenceEqual(typeName)) {
                                result = new TracorDataProperty(
                                    name: argNameString,
                                    typeValue: TracorDataPropertyTypeValue.Boolean,
                                    textValue: textValueString
#warning TODO                                     value: TracorDataUtility.GetBoolValueBoxes(textValueString)
                                );
                                return true;
                            } else if (TracorDataProperty.TypeNameLong.AsSpan().SequenceEqual(typeName)) {
                                if (long.TryParse(textValueString, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out var longValue)) {
                                    result = new TracorDataProperty(
                                        name: argNameString,
                                        typeValue: TracorDataPropertyTypeValue.Long,
                                        textValue: textValueString
#warning TODO                                         value: longValue
                                    );
                                    return true;
                                }
                            } else if (TracorDataProperty.TypeNameDouble.AsSpan().SequenceEqual(typeName)) {
                                if (double.TryParse(textValueString, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out var doubleValue)) {
                                    result = new TracorDataProperty(
                                        name: argNameString,
                                        typeValue: TracorDataPropertyTypeValue.Float,
                                        textValue: textValueString
#warning TODO                                         value: doubleValue
                                    );
                                    return true;
                                }
                            } else if (TracorDataProperty.TypeNameAny.AsSpan().SequenceEqual(typeName)) {
                                result = new TracorDataProperty(
                                    name: argNameString,
                                    typeValue: TracorDataPropertyTypeValue.Any,
                                    textValue: textValueString
#warning TODO                                     value: textValueString
                                );
                                return true;
                            }
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

}
