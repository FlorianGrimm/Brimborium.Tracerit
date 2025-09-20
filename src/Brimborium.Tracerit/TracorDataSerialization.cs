namespace Brimborium.Tracerit;

public static class TracorDataSerialization {
    public static TracorDataCollection ToTracorDataCollection(
        List<TracorIdentitfierData> srcListTracorIdentitfierData) {
        TracorDataCollection result = new();
        foreach (var itemData in srcListTracorIdentitfierData) {
            TracorDataRecord tracorData = new();
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
        TracorDataRecord dstTracorData = new();
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
        TracorDataRecord result = new();
        bool isSourceParsed = false;
        bool isCalleeParsed = false;
        string? tracorIdentitfierSource = null;
        string? tracorIdentitfierCallee = null;

        for (int idx = 0; idx < srcTracorData.Count; idx++) {
            string itemTracorDataProperty = srcTracorData[idx];
            var success = TracorDataProperty.TryParseFromJsonString(itemTracorDataProperty, out var dstTracorDataProperty);
            if (!success) { continue; }

            if (!isSourceParsed) {
                if (nameof(TracorIdentitfier.Source) == dstTracorDataProperty.Name) {
                    if (TracorDataProperty.TypeNameString == dstTracorDataProperty.TypeName) {
                        tracorIdentitfierSource = dstTracorDataProperty.TextValue;
                        isSourceParsed = true;
                        continue;
                    }
                }
            }
            if (!isCalleeParsed) {
                if (nameof(TracorIdentitfier.Scope) == dstTracorDataProperty.Name) {
                    if (TracorDataProperty.TypeNameString == dstTracorDataProperty.TypeName) {
                        tracorIdentitfierCallee = dstTracorDataProperty.TextValue;
                        isCalleeParsed = true;
                        continue;
                    }
                }
            }

            {
                result.ListProperty.Add(dstTracorDataProperty);
            }
        }
        if (isSourceParsed && isCalleeParsed) {
            result.TracorIdentitfier = new(
                tracorIdentitfierSource ?? string.Empty,
                tracorIdentitfierCallee ?? string.Empty);
        }
        return result;
    }
}
