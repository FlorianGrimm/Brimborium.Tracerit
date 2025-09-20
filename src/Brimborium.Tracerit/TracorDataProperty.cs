namespace Brimborium.Tracerit;

public enum TracorDataPropertyTypeValue {
    Any,
    String,
    Integer,
    LevelValue,
    DateTime,
    DateTimeOffset,
    Boolean,
    Long,
    Double
}

public struct TracorDataProperty {
    public const string TypeNameAny = "any";
    public const string TypeNameString = "str";
    public const string TypeNameInteger = "int";
    public const string TypeNameLevelValue = "lvl";
    public const string TypeNameDateTime = "dt";
    public const string TypeNameDateTimeOffset = "dto";
    public const string TypeNameBoolean = "bool";
    public const string TypeNameLong = "long";
    public const string TypeNameDouble = "dbl";

    private string? _TypeName;
    private TracorDataPropertyTypeValue _TypeValue;

    public TracorDataProperty(
       string name,
       TracorDataPropertyTypeValue typeValue,
       string textValue,
       object? value
       ) {
        this.Name = name;
        this._TypeValue = typeValue;
        this._TypeName = null;
        this.TextValue = textValue;
        this.Value = value;
    }

    public TracorDataProperty(
        string name,
        string typeName,
        string textValue,
        object? value
        ) {
        this.Name = name;
        (this._TypeValue, this._TypeName) = TracorDataUtility.TracorDataPropertyConvertStringToTypeName(typeName);
        this.TextValue = textValue;
        this.Value = value;
    }

    public string Name { get; set; }

    public string TypeName {
        readonly get => TracorDataUtility.TracorDataPropertyConvertTypeValueToString(this._TypeValue, this._TypeName);
        set => this._TypeName = value;
    }

    public TracorDataPropertyTypeValue TypeValue {
        readonly get {
            return this._TypeValue;
        }
        set {
            this._TypeValue = value;
            this._TypeName = null;
        }
    }

    public string TextValue { get; set; }

    public object? Value { get; set; }

    public static TracorDataProperty Create(string argName, object argValue) {
        if (argValue is string stringValue) {
            return CreateString(argName, stringValue);
        }
        if (argValue is int intValue) {
            return CreateInteger(argName, intValue);
        }
        if (argValue is LogLevel logLevelValue) {
            return CreateLevelValue(argName, logLevelValue);
        }
        if (argValue is DateTime dtValue) {
            return CreateDateTime(argName, dtValue);
        }
        if (argValue is DateTimeOffset dtoValue) {
            return CreateDateTimeOffset(argName, dtoValue);
        }
        if (argValue is long longValue) {
            return CreateLong(argName, longValue);
        }
        if (argValue is double doubleValue) {
            return CreateDouble(argName, doubleValue);
        }
        {
            return new TracorDataProperty(
                name: argName,
                typeValue: TracorDataPropertyTypeValue.Any,
                textValue: argValue.ToString() ?? string.Empty,
                value: argValue
            );
        }
    }

    public readonly void ToMinimizeString(StringBuilder sbOut) {
        sbOut
            .Append(this.Name)
            .Append(_SeperationJsonChar)
            .Append(this.TypeName)
            .Append(_SeperationJsonChar)
            .Append(this.TextValue)
            ;
    }

    private const char _SeperationJsonChar = ':';

    public static bool TryParseFromJsonString(ReadOnlySpan<char> value, out TracorDataProperty result) {
        int posNameColon = value.IndexOf(_SeperationJsonChar);
        if (1 <= posNameColon) {
            var argName = value[..posNameColon];
            int posAfterNameColon = posNameColon + 1;
            if (posAfterNameColon < value.Length) {
                var valueAfterNameColon = value[posAfterNameColon..];
                int posTypeColon = valueAfterNameColon.IndexOf(_SeperationJsonChar);
                var posTypeColonAfter = posTypeColon + 1;
                if (1 < posTypeColon && posTypeColonAfter <= valueAfterNameColon.Length) {
                    var typeName = valueAfterNameColon[..posTypeColon];
                    var textValue = valueAfterNameColon[posTypeColonAfter..];
                    var argNameString = TracorDataUtility.GetPropertyName(argName);
                    var textValueString = TracorDataUtility.GetPropertyValue(textValue);
                    if (TypeNameString.AsSpan().SequenceEqual(typeName)) {
                        result = new TracorDataProperty(
                            name: argNameString,
                            typeValue: TracorDataPropertyTypeValue.String,
                            textValue: textValueString,
                            value: textValueString
                        );
                        return true;
                    } else if (TypeNameInteger.AsSpan().SequenceEqual(typeName)) {
                        if (int.TryParse(textValue, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out var intValue)) {
                            result = new TracorDataProperty(
                                name: argNameString,
                                typeValue: TracorDataPropertyTypeValue.Integer,
                                textValue: textValueString,
                                value: intValue
                            );
                            return true;
                        }
                    } else if (TypeNameLevelValue.AsSpan().SequenceEqual(typeName)) {
                        if (LogLevelUtility.TryGetLogLevelByName(textValueString, out var level)) {
                            result = new TracorDataProperty(
                                name: argNameString,
                                typeValue: TracorDataPropertyTypeValue.LevelValue,
                                textValue: textValueString,
                                value: level
                            );
                            return true;
                        } else if (int.TryParse(textValue, out var intValue)) {
                            result = new TracorDataProperty(
                                name: argNameString,
                                typeValue: TracorDataPropertyTypeValue.LevelValue,
                                textValue: textValueString,
                                value: (LogLevel)intValue
                            );
                            return true;
                        }
                    } else if (TypeNameDateTime.AsSpan().SequenceEqual(typeName)) {
                        if (long.TryParse(textValue, null, out var ns)) {
                            result = new TracorDataProperty(
                                name: argNameString,
                                typeValue: TracorDataPropertyTypeValue.DateTime,
                                textValue: textValueString,
                                value: TracorDataUtility.UnixTimeNanosecondsToDateTime(ns)
                            );
                            return true;
                        } else if (DateTime.TryParseExact(textValue, "O", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.None, out var dtValue)) {
                            result = new TracorDataProperty(
                                name: argNameString,
                                typeValue: TracorDataPropertyTypeValue.DateTime,
                                textValue: textValueString,
                                value: dtValue
                            );
                            return true;
                        }
                    } else if (TypeNameDateTimeOffset.AsSpan().SequenceEqual(typeName)) {
                        if (long.TryParse(textValue, null, out var ns)) {
                            result = new TracorDataProperty(
                                name: argNameString,
                                typeValue: TracorDataPropertyTypeValue.DateTimeOffset,
                                textValue: textValueString,
                                value: TracorDataUtility.UnixTimeNanosecondsToDateTimeOffset(ns)
                            );
                            return true;
                        } else if (DateTimeOffset.TryParseExact(textValue, "O", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.None, out var dtoValue)) {
                            result = new TracorDataProperty(
                                name: argNameString,
                                typeValue: TracorDataPropertyTypeValue.DateTime,
                                textValue: textValueString,
                                value: dtoValue
                            );
                            return true;
                        }
                    } else if (TypeNameBoolean.AsSpan().SequenceEqual(typeName)) {
                        result = new TracorDataProperty(
                            name: argNameString,
                            typeValue: TracorDataPropertyTypeValue.Boolean,
                            textValue: textValueString,
                            value: TracorDataUtility.GetBoolValueBoxes(textValueString)
                        );
                        return true;
                    } else if (TypeNameLong.AsSpan().SequenceEqual(typeName)) {
                        if (long.TryParse(textValueString, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out var longValue)) {
                            result = new TracorDataProperty(
                                name: argNameString,
                                typeValue: TracorDataPropertyTypeValue.Long,
                                textValue: textValueString,
                                value: longValue
                            );
                            return true;
                        }
                    } else if (TypeNameDouble.AsSpan().SequenceEqual(typeName)) {
                        if (double.TryParse(textValueString, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out var doubleValue)) {
                            result = new TracorDataProperty(
                                name: argNameString,
                                typeValue: TracorDataPropertyTypeValue.Double,
                                textValue: textValueString,
                                value: doubleValue
                            );
                            return true;
                        }
                    } else if (TypeNameAny.AsSpan().SequenceEqual(typeName)) {
                        result = new TracorDataProperty(
                            name: argNameString,
                            typeValue: TracorDataPropertyTypeValue.Any,
                            textValue: textValueString,
                            value: textValueString
                        );
                        return true;
                    }
                }
            }
        }

        // fallback - error
        {
            result = new(
                name: string.Empty,
                typeValue: TracorDataPropertyTypeValue.Any,
                textValue: string.Empty,
                value: string.Empty
                );
            return false;
        }
    }

    public static TracorDataProperty CreateString(string argName, string argValue)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.String,
            textValue: argValue,
            value: argValue
        );

    public static TracorDataProperty CreateInteger(string argName, int argValue)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.Integer,
            textValue: argValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat),
            value: argValue
        );

    public static TracorDataProperty CreateLevelValue(string argName, LogLevel argValue)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.LevelValue,
            textValue: argValue.ToString(),
            value: argValue
        );

    public static TracorDataProperty CreateDateTime(string argName, DateTime argValue) {
        var ns = TracorDataUtility.DateTimeToUnixTimeNanoseconds(argValue);
        return new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.DateTime,
            textValue: ns.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat),
            value: argValue
        );
    }

    public static TracorDataProperty CreateDateTimeOffset(string argName, DateTimeOffset argValue) {
        var ns = TracorDataUtility.DateTimeOffsetToUnixTimeNanoseconds(argValue);
        return new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.DateTimeOffset,
            textValue: ns.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat),
            value: argValue
        );
    }

    public static TracorDataProperty CreateBoolean(string argName, bool argValue)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.Boolean,
            textValue: TracorDataUtility.GetBoolString(argValue),
            value: TracorDataUtility.GetBoolValueBoxes(argValue)
        );

    public static TracorDataProperty CreateLong(string argName, long argValue)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.Long,
            textValue: argValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat),
            value: argValue
        );

    public static TracorDataProperty CreateDouble(string argName, double argValue)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.Double,
            textValue: argValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat),
            value: argValue
        );

    public readonly bool HasEqualValue(object? currentPropertyValue) {
        if (TypeNameString == this.TypeName) {
            return string.Equals(this.TextValue, currentPropertyValue as string);
        }
        if (TypeNameInteger == this.TypeName) {
            return (currentPropertyValue is int currentValue)
                && (this.Value is int thisValue)
                && (currentValue == thisValue);
        }
        if (TypeNameLevelValue == this.TypeName) {
            return (currentPropertyValue is LogLevel currentValue)
                && (this.Value is LogLevel thisValue)
                && (currentValue == thisValue);
        }
        if (TypeNameDateTime == this.TypeName) {
            return (currentPropertyValue is DateTime currentValue)
                && (this.Value is DateTime thisValue)
                && (currentValue == thisValue);
        }
        if (TypeNameDateTimeOffset == this.TypeName) {
            return (currentPropertyValue is DateTimeOffset currentValue)
                && (this.Value is DateTimeOffset thisValue)
                && (currentValue == thisValue);
        }
        if (TypeNameBoolean == this.TypeName) {
            return (currentPropertyValue is bool currentValue)
                && (this.Value is bool thisValue)
                && (currentValue == thisValue);
        }
        if (TypeNameLong == this.TypeName) {
            return (currentPropertyValue is long currentValue)
                && (this.Value is long thisValue)
                && (currentValue == thisValue);
        }
        if (TypeNameDouble == this.TypeName) {
            return (currentPropertyValue is double currentValue)
                && (this.Value is double thisValue)
                && (currentValue == thisValue);
        }
        if (TypeNameAny == this.TypeName) {
            return false;
        }
        return false;
    }
}
