namespace Brimborium.Tracerit;

public struct TracorDataProperty {
    public const string TypeNameString = "str";
    public const string TypeNameInteger = "int";
    public const string TypeNameLevelValue = "lvl";
    public const string TypeNameDateTime = "dt";
    public const string TypeNameDateTimeOffset = "dto";
    public const string TypeNameBoolean = "bool";
    public const string TypeNameLong = "long";
    public const string TypeNameDouble = "dbl";
    public const string TypeNameAny = "any";

    public required string Name { get; set; }
    public required string TypeName { get; set; }
    public required string TextValue { get; set; }
    public required object? Value { get; set; }

    public static TracorDataProperty Create(string argName, object argValue) {
        if (argValue is string stringValue) {
            return ConvertString(argName, stringValue);
        }
        if (argValue is int intValue) {
            return ConvertInteger(argName, intValue);
        }
        if (argValue is LogLevel logLevelValue) {
            return ConvertLevelValue(argName, logLevelValue);
        }
        if (argValue is DateTime dtValue) {
            return ConvertDateTime(argName, dtValue);
        }
        if (argValue is DateTimeOffset dtoValue) {
            return ConvertDateTimeOffset(argName, dtoValue);
        }
        if (argValue is long longValue) {
            return ConvertLong(argName, longValue);
        }
        if (argValue is double doubleValue) {
            return ConvertDouble(argName, doubleValue);
        }
        {
            return new TracorDataProperty() {
                Name = argName,
                TypeName = TypeNameAny,
                TextValue = argValue.ToString() ?? string.Empty,
                Value = argValue
            };
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
                        result = new TracorDataProperty() {
                            Name = argNameString,
                            TypeName = TypeNameString,
                            TextValue = textValueString,
                            Value = textValueString
                        };
                        return true;
                    } else if (TypeNameInteger.AsSpan().SequenceEqual(typeName)) {
                        if (int.TryParse(textValue, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out var intValue)) {
                            result = new TracorDataProperty() {
                                Name = argNameString,
                                TypeName = TypeNameInteger,
                                TextValue = textValueString,
                                Value = intValue
                            };
                            return true;
                        }
                    } else if (TypeNameLevelValue.AsSpan().SequenceEqual(typeName)) {
                        if (LogLevelUtility.TryGetLogLevelByName(textValueString, out var level)) {
                            result = new TracorDataProperty() {
                                Name = argNameString,
                                TypeName = TypeNameLevelValue,
                                TextValue = textValueString,
                                Value = level
                            };
                            return true;
                        } else if (int.TryParse(textValue, out var intValue)) {
                            result = new TracorDataProperty() {
                                Name = argNameString,
                                TypeName = TypeNameLevelValue,
                                TextValue = textValueString,
                                Value = (LogLevel)intValue
                            };
                            return true;
                        }
                    } else if (TypeNameDateTime.AsSpan().SequenceEqual(typeName)) {
                        if (long.TryParse(textValue, null, out var ns)) {
                            result = new TracorDataProperty() {
                                Name = argNameString,
                                TypeName = TypeNameDateTime,
                                TextValue = textValueString,
                                Value = TracorDataUtility.UnixTimeNanosecondsToDateTime(ns)
                            };
                            return true;
                        } else if (DateTime.TryParseExact(textValue, "O", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.None, out var dtValue)) {
                            result = new TracorDataProperty() {
                                Name = argNameString,
                                TypeName = TypeNameDateTime,
                                TextValue = textValueString,
                                Value = dtValue
                            };
                            return true;
                        }
                    } else if (TypeNameDateTimeOffset.AsSpan().SequenceEqual(typeName)) {
                        if (long.TryParse(textValue, null, out var ns)) {
                            result = new TracorDataProperty() {
                                Name = argNameString,
                                TypeName = TypeNameDateTimeOffset,
                                TextValue = textValueString,
                                Value = TracorDataUtility.UnixTimeNanosecondsToDateTimeOffset(ns)
                            };
                            return true;
                        } else if (DateTimeOffset.TryParseExact(textValue, "O", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.None, out var dtoValue)) {
                            result = new TracorDataProperty() {
                                Name = argNameString,
                                TypeName = TypeNameDateTime,
                                TextValue = textValueString,
                                Value = dtoValue
                            };
                            return true;
                        }
                    } else if (TypeNameBoolean.AsSpan().SequenceEqual(typeName)) {
                        result = new TracorDataProperty() {
                            Name = argNameString,
                            TypeName = TypeNameBoolean,
                            TextValue = textValueString,
                            Value = TracorDataUtility.GetBoolValueBoxes(textValueString)
                        };
                        return true;
                    } else if (TypeNameLong.AsSpan().SequenceEqual(typeName)) {
                        if (long.TryParse(textValueString, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out var longValue)) {
                            result = new TracorDataProperty() {
                                Name = argNameString,
                                TypeName = TypeNameLong,
                                TextValue = textValueString,
                                Value = longValue
                            };
                            return true;
                        }
                    } else if (TypeNameDouble.AsSpan().SequenceEqual(typeName)) {
                        if (double.TryParse(textValueString, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out var doubleValue)) {
                            result = new TracorDataProperty() {
                                Name = argNameString,
                                TypeName = TypeNameDouble,
                                TextValue = textValueString,
                                Value = doubleValue
                            };
                            return true;
                        }
                    } else if (TypeNameAny.AsSpan().SequenceEqual(typeName)) {
                        result = new TracorDataProperty() {
                            Name = argNameString,
                            TypeName = TypeNameAny,
                            TextValue = textValueString,
                            Value = textValueString
                        };
                        return true;
                    }
                }
            }
        }

        result = new() { Name = string.Empty, TypeName = string.Empty, TextValue = string.Empty, Value = null };
        return false;
    }

    public static TracorDataProperty ConvertString(string argName, string argValue)
        => new TracorDataProperty() {
            Name = argName,
            TypeName = TypeNameString,
            TextValue = argValue,
            Value = argValue
        };

    public static TracorDataProperty ConvertInteger(string argName, int argValue)
        => new TracorDataProperty() {
            Name = argName,
            TypeName = TypeNameInteger,
            TextValue = argValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat),
            Value = argValue
        };

    public static TracorDataProperty ConvertLevelValue(string argName, LogLevel argValue)
        => new TracorDataProperty() {
            Name = argName,
            TypeName = TypeNameLevelValue,
            TextValue = argValue.ToString(),
            Value = argValue
        };

    public static TracorDataProperty ConvertDateTime(string argName, DateTime argValue) {
        var ns = TracorDataUtility.DateTimeToUnixTimeNanoseconds(argValue);
        return new TracorDataProperty() {
            Name = argName,
            TypeName = TypeNameDateTime,
            TextValue = ns.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat),
            Value = argValue
        };
    }

    public static TracorDataProperty ConvertDateTimeOffset(string argName, DateTimeOffset argValue) {
        var ns = TracorDataUtility.DateTimeOffsetToUnixTimeNanoseconds(argValue);
        return new TracorDataProperty() {
            Name = argName,
            TypeName = TypeNameDateTimeOffset,
            TextValue = ns.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat),
            Value = argValue
        };
    }

    public static TracorDataProperty ConvertBoolean(string argName, bool argValue)
        => new TracorDataProperty() {
            Name = argName,
            TypeName = TypeNameBoolean,
            TextValue = TracorDataUtility.GetBoolString(argValue),
            Value = TracorDataUtility.GetBoolValueBoxes(argValue)
        };

    public static TracorDataProperty ConvertLong(string argName, long argValue)
        => new TracorDataProperty() {
            Name = argName,
            TypeName = TypeNameLong,
            TextValue = argValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat),
            Value = argValue
        };

    public static TracorDataProperty ConvertDouble(string argName, double argValue)
        => new TracorDataProperty() {
            Name = argName,
            TypeName = TypeNameDouble,
            TextValue = argValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat),
            Value = argValue
        };

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
