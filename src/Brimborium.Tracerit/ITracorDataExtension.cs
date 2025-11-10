namespace Brimborium.Tracerit;

/// <summary>
/// Provides extension methods for <see cref="ITracorData"/> to enhance functionality with strongly-typed operations.
/// </summary>
public static class ITracorDataExtension {
    public static bool IsEqualNull(this ITracorData tracorData, string propertyName)
        => (tracorData.TryGetDataProperty(propertyName, out var tdp)
            && TracorDataPropertyTypeValue.Null == tdp.TypeValue);

    public static bool IsEqualString(this ITracorData tracorData, string propertyName, string expected, StringComparison comparisonType = StringComparison.Ordinal)
        => (tracorData.TryGetDataProperty(propertyName, out var tdp))
        && (tdp.TryGetStringValue(out var currentValue))
        && (string.Equals(currentValue, expected, comparisonType));

    public static bool IsEqualInteger(this ITracorData tracorData, string propertyName, long expected)
        => (tracorData.TryGetDataProperty(propertyName, out var tdp))
        && (tdp.TryGetIntegerValue(out var currentValue))
        && (currentValue == expected);

    public static bool IsEqualBoolean(this ITracorData tracorData, string propertyName, bool expected)
        => (tracorData.TryGetDataProperty(propertyName, out var tdp))
        && (tdp.TryGetBooleanValue(out var currentValue))
        && (currentValue == expected);

    public static bool IsEqualEnum(this ITracorData tracorData, string propertyName, string expected)
        => (tracorData.TryGetDataProperty(propertyName, out var tdp))
        && (tdp.TryGetEnumValue(out var currentValue))
        && string.Equals(currentValue, expected, StringComparison.OrdinalIgnoreCase);

    public static bool IsEqualLevelValue(this ITracorData tracorData, string propertyName, LogLevel expected)
        => (tracorData.TryGetDataProperty(propertyName, out var tdp))
        && (tdp.TryGetLevelValue(out var currentValue))
        && (currentValue == expected);

    public static bool IsEqualDouble(this ITracorData tracorData, string propertyName, double expected)
        => (tracorData.TryGetDataProperty(propertyName, out var tdp))
        && (tdp.TryGetDoubleValue(out var currentValue))
        && (currentValue == expected);

    public static bool IsEqualDateTime(this ITracorData tracorData, string propertyName, DateTime expected)
        => (tracorData.TryGetDataProperty(propertyName, out var tdp))
        && (tdp.TryGetDateTimeValue(out var currentValue))
        && (currentValue == expected);

    public static bool IsEqualDateTimeOffset(this ITracorData tracorData, string propertyName, DateTimeOffset expected)
        => (tracorData.TryGetDataProperty(propertyName, out var tdp))
        && (tdp.TryGetDateTimeOffsetValue(out var currentValue))
        && (currentValue == expected);

    public static bool IsEqualUuid(this ITracorData tracorData, string propertyName, Guid expected)
        => (tracorData.TryGetDataProperty(propertyName, out var tdp))
        && (tdp.TryGetUuidValue(out var currentValue))
        && (currentValue == expected);

    public static bool TryGetPropertyValueNull(this ITracorData tracorData, string propertyName) {
        return (tracorData.TryGetDataProperty(propertyName, out var tdp)
            && TracorDataPropertyTypeValue.Null == tdp.TypeValue);
    }

    public static bool TryGetPropertyValueString(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out string value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)
            && TracorDataUtility.TryConvertObjectToStringValue(propertyValue, out value)) {
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryGetPropertyValueInteger(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out long value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)
            && TracorDataUtility.TryConvertObjectToIntegerValue(propertyValue, out value)) {
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryGetPropertyValueBoolean(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out bool value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)
            && TracorDataUtility.TryConvertObjectToBooleanValue(propertyValue, out value)) {
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryGetPropertyValueEnum(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out string value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)
            && TracorDataUtility.TryConvertObjectToEnumValue(propertyValue, out var textValue)) {
            if (textValue is { Length: > 0 }) {
                value = textValue;
                return true;
            }
        }
        value = default;
        return false;
    }

    public static bool TryGetPropertyValueLevelValue(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out LogLevel value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)
            && TracorDataUtility.TryConvertObjectToLogLevelValue(propertyValue, out value)) {
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryGetPropertyValueDouble(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out double value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)
            && TracorDataUtility.TryConvertObjectToDoubleValue(propertyValue, out value)) {
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryGetPropertyValueDateTime(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out DateTime value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)
            && TracorDataUtility.TryConvertObjectToDateTimeValue(propertyValue, out value)) {
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryGetPropertyValueDateTimeOffset(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out DateTimeOffset value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)
            && TracorDataUtility.TryConvertObjectToDateTimeOffsetValue(propertyValue, out value)) {
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryGetPropertyValueUuid(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out Guid value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)
            && TracorDataUtility.TryConvertObjectToUuidValue(propertyValue, out value)) {
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryGetTracorDataProperty(
        this ITracorData thatTracorData,
        string propertyName,
        out TracorDataProperty result) {
        if (string.Equals(TracorConstants.TracorDataPropertyNameTimestamp, propertyName, StringComparison.Ordinal)) {
            result = TracorDataProperty.CreateDateTimeValue(
                TracorConstants.TracorDataPropertyNameTimestamp,
                thatTracorData.Timestamp);
            return true;
        }
        if (string.Equals(TracorConstants.TracorDataPropertyNameSource, propertyName, StringComparison.Ordinal)) {
            var value = (thatTracorData.TracorIdentifier.SourceProvider is { Length: > 0 } source) ? source : string.Empty;
            result = TracorDataProperty.CreateStringValue(
                    TracorConstants.TracorDataPropertyNameSource,
                    value);
            return true;
        }
        if (string.Equals(TracorConstants.TracorDataPropertyNameScope, propertyName, StringComparison.Ordinal)) {
            var value = (thatTracorData.TracorIdentifier.Scope is { Length: > 0 } scope) ? scope : string.Empty;
            result = TracorDataProperty.CreateStringValue(
                    TracorConstants.TracorDataPropertyNameScope,
                    value);
            return true;
        }
        if (string.Equals(TracorConstants.TracorDataPropertyNameMessage, propertyName, StringComparison.Ordinal)) {
            var value = (thatTracorData.TracorIdentifier.Message is { Length: > 0 } message) ? message : string.Empty;
            result = TracorDataProperty.CreateStringValue(
                    TracorConstants.TracorDataPropertyNameMessage,
                    value);
            return true;
        }

        return thatTracorData.TryGetDataProperty(propertyName, out result);
    }

    public static void ConvertPropertiesBase<TTracorData>(
        this ITracorData thatTracorData,
        List<TracorDataProperty> listProperty)
        where TTracorData : ITracorData {
        listProperty.Add(
            TracorDataProperty.CreateDateTimeValue(
                TracorConstants.TracorDataPropertyNameTimestamp,
                thatTracorData.Timestamp));
        if (thatTracorData.TracorIdentifier.SourceProvider is { Length: > 0 } source) {
            listProperty.Add(
                TracorDataProperty.CreateStringValue(
                    TracorConstants.TracorDataPropertyNameSource,
                    source));
        }
        if (thatTracorData.TracorIdentifier.Scope is { Length: > 0 } scope) {
            listProperty.Add(
                TracorDataProperty.CreateStringValue(
                    TracorConstants.TracorDataPropertyNameScope,
                    scope));
        }
        if (thatTracorData.TracorIdentifier.Message is { Length: > 0 } message) {
            listProperty.Add(
                TracorDataProperty.CreateStringValue(
                    TracorConstants.TracorDataPropertyNameMessage,
                    message));
        }
    }

    public static void CopyPropertiesToSinkBase<TTracorData>(
        ITracorData thatTracorData,
        TracorPropertySinkTarget target)
        where TTracorData : ITracorData {
        { 
            ref var targetProperty = ref (target.ListHeader.GetNext());
            targetProperty.Name = TracorConstants.TracorDataPropertyNameTimestamp;
            targetProperty.SetDateTimeValue(thatTracorData.Timestamp);
        }
        if (thatTracorData.TracorIdentifier.SourceProvider is { Length: > 0 } source) {
            ref var targetProperty = ref (target.ListHeader.GetNext());
            targetProperty.Name = TracorConstants.TracorDataPropertyNameSource;
            targetProperty.SetStringValue(source);
        }
        if (thatTracorData.TracorIdentifier.Scope is { Length: > 0 } scope) {
            ref var targetProperty = ref (target.ListHeader.GetNext());
            targetProperty.Name = TracorConstants.TracorDataPropertyNameScope;
            targetProperty.SetStringValue(scope);
        }
        if (thatTracorData.TracorIdentifier.Message is { Length: > 0 } message) {
            ref var targetProperty = ref (target.ListHeader.GetNext());
            targetProperty.Name = TracorConstants.TracorDataPropertyNameMessage;
            targetProperty.SetStringValue(message);
        }
    }
}