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
            && (TracorDataUtility.TryConvertToStringValue(currentValue, out var value))
            && (string.Equals(value, expected, comparisonType));

    public static bool IsEqualInteger(this ITracorData data, string propertyName, long expected)
        => data.TryGetPropertyValue(propertyName, out var currentValue)
            && (TracorDataUtility.TryConvertToIntValue(currentValue, out var value))
            && (value == expected);

    public static bool IsEqualBoolean(this ITracorData data, string propertyName, bool expected)
        => data.TryGetPropertyValue(propertyName, out var currentValue)
            && (TracorDataUtility.TryConvertToBoolValue(currentValue, out var value))
            && (value == expected);

    public static bool IsEqualEnum(this ITracorData data, string propertyName, string expected)
        => data.TryGetPropertyValue(propertyName, out var currentValue)
            && (TracorDataUtility.TryConvertToEnumValue(currentValue, out var value))
            && (value == expected);

    public static bool IsEqualLevelValue(this ITracorData data, string propertyName, LogLevel expected)
        => data.TryGetPropertyValue(propertyName, out var currentValue)
            && (TracorDataUtility.TryConvertToLevelValue(currentValue, out var value))
            && (value == expected);

    public static bool IsEqualFloat(this ITracorData data, string propertyName, double expected)
        => data.TryGetPropertyValue(propertyName, out var currentValue)
            && (TracorDataUtility.TryConvertToFloatValue(currentValue, out var value))
            && (value == expected);

    public static bool IsEqualDateTime(this ITracorData data, string propertyName, DateTime expected)
        => data.TryGetPropertyValue(propertyName, out var currentValue)
            && (TracorDataUtility.TryConvertToDateTimeValue(currentValue, out var value))
            && (value == expected);

    public static bool IsEqualDateTimeOffset(this ITracorData data, string propertyName, DateTimeOffset expected)
        => data.TryGetPropertyValue(propertyName, out var currentValue)
            && (TracorDataUtility.TryConvertToDateTimeOffsetValue(currentValue, out var value))
            && (value == expected);

    public static bool IsEqualUuid(this ITracorData data, string propertyName, Guid expected)
        => data.TryGetPropertyValue(propertyName, out var currentValue)
            && (TracorDataUtility.TryConvertToUuidValue(currentValue, out var value))
            && (value == expected);


    public static bool TryGetPropertyValueString(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out string value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)
            && TracorDataUtility.TryConvertToStringValue(propertyValue, out value)) {
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryGetPropertyValueInteger(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out long value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)
            && TracorDataUtility.TryConvertToIntValue(propertyValue, out value)) {
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryGetPropertyValueBoolean(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out bool value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)
            && TracorDataUtility.TryConvertToBoolValue(propertyValue, out value)) {
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryGetPropertyValueEnum(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out string value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)
            && TracorDataUtility.TryConvertToEnumValue(propertyValue, out value)) {
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryGetPropertyValueLevelValue(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out LogLevel value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)
            && TracorDataUtility.TryConvertToLevelValue(propertyValue, out value)) {
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryGetPropertyValueFloat(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out double value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)
            && TracorDataUtility.TryConvertToFloatValue(propertyValue, out value)) {
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryGetPropertyValueDateTime(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out DateTime value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)
            && TracorDataUtility.TryConvertToDateTimeValue(propertyValue, out value)) {
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryGetPropertyValueDateTimeOffset(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out DateTimeOffset value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)
            && TracorDataUtility.TryConvertToDateTimeOffsetValue(propertyValue, out value)) {
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryGetPropertyValueUuid(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out Guid value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)
            && TracorDataUtility.TryConvertToUuidValue(propertyValue, out value)) {
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

    public static void ConvertPropertiesBase<TTracorData>(
        this ITracorData thatTracorData,
        List<TracorDataProperty> listProperty)
        where TTracorData : ITracorData {
        listProperty.Add(TracorDataProperty.CreateDateTime("timestamp", thatTracorData.Timestamp));
        if (thatTracorData.TracorIdentitfier.Source is { Length: > 0 } source) {
            listProperty.Add(TracorDataProperty.CreateString("source", source));
        }
        if (thatTracorData.TracorIdentitfier.Scope is { Length: > 0 } scope) {
            listProperty.Add(TracorDataProperty.CreateString("scope", scope));
        }
        if (thatTracorData.TracorIdentitfier.Message is { Length: > 0 } message) {
            listProperty.Add(TracorDataProperty.CreateString("message", message));
        }
    }
}