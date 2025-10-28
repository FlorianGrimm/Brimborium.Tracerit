using System.Xml.Xsl;

namespace Brimborium.Tracerit;

/// <summary>
/// Provides extension methods for <see cref="ITracorData"/> to enhance functionality with strongly-typed operations.
/// </summary>
public static class ITracorDataExtension {
    public static bool IsEqualNull(this ITracorData data, string propertyName)
        => data.TryGetPropertyValue(propertyName, out var currentValue)
            && currentValue is null;

    public static bool IsEqualString(this ITracorData data, string propertyName, string expected, StringComparison comparisonType = StringComparison.Ordinal)
        => data.TryGetPropertyValue(propertyName, out var currentValue)
            && (TracorDataUtility.TryConvertObjectToStringValue(currentValue, out var value))
            && (string.Equals(value, expected, comparisonType));

    public static bool IsEqualInteger(this ITracorData data, string propertyName, long expected)
        => data.TryGetPropertyValue(propertyName, out var currentValue)
            && (TracorDataUtility.TryConvertObjectToIntegerValue(currentValue, out var value))
            && (value == expected);

    public static bool IsEqualBoolean(this ITracorData data, string propertyName, bool expected)
        => data.TryGetPropertyValue(propertyName, out var currentValue)
            && (TracorDataUtility.TryConvertObjectToBooleanValue(currentValue, out var value))
            && (value == expected);

    public static bool IsEqualEnum(this ITracorData data, string propertyName, string expected)
        => data.TryGetPropertyValue(propertyName, out var currentValue)
            && (TracorDataUtility.TryConvertObjectToEnumValue(currentValue, out var thisTextValue))
            && string.Equals(thisTextValue, expected, StringComparison.OrdinalIgnoreCase);

    public static bool IsEqualLevelValue(this ITracorData data, string propertyName, LogLevel expected)
        => data.TryGetPropertyValue(propertyName, out var currentValue)
            && (TracorDataUtility.TryConvertObjectToLogLevelValue(currentValue, out var value))
            && (value == expected);

    public static bool IsEqualDouble(this ITracorData data, string propertyName, double expected)
        => data.TryGetPropertyValue(propertyName, out var currentValue)
            && (TracorDataUtility.TryConvertObjectToDoubleValue(currentValue, out var value))
            && (value == expected);

    public static bool IsEqualDateTime(this ITracorData data, string propertyName, DateTime expected)
        => data.TryGetPropertyValue(propertyName, out var currentValue)
            && (TracorDataUtility.TryConvertObjectToDateTimeValue(currentValue, out var value))
            && (value == expected);

    public static bool IsEqualDateTimeOffset(this ITracorData data, string propertyName, DateTimeOffset expected)
        => data.TryGetPropertyValue(propertyName, out var currentValue)
            && (TracorDataUtility.TryConvertObjectToDateTimeOffsetValue(currentValue, out var value))
            && (value == expected);

    public static bool IsEqualUuid(this ITracorData data, string propertyName, Guid expected)
        => data.TryGetPropertyValue(propertyName, out var currentValue)
            && (TracorDataUtility.TryConvertObjectToUuidValue(currentValue, out var value))
            && (value == expected);


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

    /// <summary>
    /// Attempts to get a strongly-typed property value from the trace data.
    /// </summary>
    /// <typeparam name="T">The expected type of the property value.</typeparam>
    /// <param name="tracorData">The trace data to query.</param>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <param name="value">When this method returns, contains the strongly-typed property value if found and can be cast to the specified type; otherwise, the default value.</param>
    /// <returns>True if the property was found and can be cast to the specified type; otherwise, false.</returns>
    public static bool TryGetPropertyValue<T>(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out T value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)) {
            if (propertyValue is T propertyValueTyped) {
                value = propertyValueTyped;
                return true;
            }
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
            var value = (thatTracorData.TracorIdentifier.Source is { Length: > 0 } source) ? source : string.Empty;
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
        if (thatTracorData.TracorIdentifier.Source is { Length: > 0 } source) {
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
}