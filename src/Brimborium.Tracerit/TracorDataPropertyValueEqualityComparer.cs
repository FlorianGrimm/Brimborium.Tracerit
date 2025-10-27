namespace Brimborium.Tracerit;

public sealed class TracorDataPropertyValueEqualityComparer : EqualityComparer<TracorDataProperty> {
    public override bool Equals(TracorDataProperty x, TracorDataProperty y) {
        if (!(x.TypeValue == y.TypeValue)) { return false; }
        switch (x.TypeValue) {
            case TracorDataPropertyTypeValue.Null:
                return true;
            case TracorDataPropertyTypeValue.String: {
                    return x.TryGetStringValue(out var a)
                        && y.TryGetStringValue(out var b)
                        && string.Equals(a, b, StringComparison.Ordinal);
                }
            case TracorDataPropertyTypeValue.Integer: {
                    return x.TryGetIntegerValue(out var a)
                        && y.TryGetIntegerValue(out var b)
                        && (a == b);
                }
            case TracorDataPropertyTypeValue.Boolean: {
                    return x.TryGetBooleanValue(out var a)
                        && y.TryGetBooleanValue(out var b)
                        && (a == b);
                }
            case TracorDataPropertyTypeValue.Enum: {
                    return x.TryGetEnumUntypedValue(out var aLongValue, out var aTextValue)
                        && y.TryGetEnumUntypedValue(out var bLongValue, out var bTextValue)
                        && ((aTextValue is { Length: > 0 } && bTextValue is { Length: > 0 } && string.Equals(aTextValue, bTextValue, StringComparison.Ordinal))
                            || (aTextValue is not { Length: > 0 } && bTextValue is not { Length: > 0 } && (aLongValue == bLongValue))
                            );
                }
            case TracorDataPropertyTypeValue.Level: {
                    return x.TryGetLevelValue(out var a)
                        && y.TryGetLevelValue(out var b)
                        && (a == b);
                }
            case TracorDataPropertyTypeValue.Double: {
                    return x.TryGetDoubleValue(out var a)
                        && y.TryGetDoubleValue(out var b)
                        && (a == b);
                }
            case TracorDataPropertyTypeValue.DateTime: {
                    return x.TryGetDateTimeValue(out var a)
                        && y.TryGetDateTimeValue(out var b)
                        && (a == b);
                }
            case TracorDataPropertyTypeValue.DateTimeOffset: {
                    return x.TryGetDateTimeOffsetValue(out var a)
                        && y.TryGetDateTimeOffsetValue(out var b)
                        && (a == b);
                }
            case TracorDataPropertyTypeValue.Uuid: {
                    return x.TryGetUuidValue(out var a)
                        && y.TryGetUuidValue(out var b)
                        && (a == b);
                }
            case TracorDataPropertyTypeValue.Any:
                return false;
            default:
                return false;
        }
    }

    public override int GetHashCode([DisallowNull] TracorDataProperty obj) {
        switch (obj.TypeValue) {
            case TracorDataPropertyTypeValue.Null:
                return HashCode.Combine(
                        obj.TypeValue);
            case TracorDataPropertyTypeValue.String: {
                    return HashCode.Combine(
                        obj.TypeValue,
                        obj.TryGetStringValue(out var value) ? value.GetHashCode() : 0);
                }
            case TracorDataPropertyTypeValue.Integer: {
                    return HashCode.Combine(
                        obj.TypeValue,
                        obj.TryGetIntegerValue(out var value) ? value.GetHashCode() : 0);
                }
            case TracorDataPropertyTypeValue.Boolean: {
                    return HashCode.Combine(
                        obj.TypeValue,
                        obj.TryGetBooleanValue(out var value) ? value.GetHashCode() : 0);
                }
            case TracorDataPropertyTypeValue.Enum: {
                    return HashCode.Combine(
                        obj.TypeValue,
                        obj.TryGetEnumUntypedValue(out var longValue, out var textValue)
                            ? ((textValue is { Length: > 0 })
                                ? textValue.GetHashCode()
                                : longValue.GetHashCode())
                            : 0);
                }
            case TracorDataPropertyTypeValue.Level: {
                    return HashCode.Combine(
                        obj.TypeValue,
                        obj.TryGetLevelValue(out var value) ? value.GetHashCode() : 0);
                }
            case TracorDataPropertyTypeValue.Double: {
                    return HashCode.Combine(
                        obj.TypeValue,
                        obj.TryGetDoubleValue(out var value) ? value.GetHashCode() : 0);
                }
            case TracorDataPropertyTypeValue.DateTime: {
                    return HashCode.Combine(
                        obj.TypeValue,
                        obj.TryGetDateTimeValue(out var value) ? value.GetHashCode() : 0);
                }
            case TracorDataPropertyTypeValue.DateTimeOffset: {
                    return HashCode.Combine(
                        obj.TypeValue,
                        obj.TryGetDateTimeOffsetValue(out var value) ? value.GetHashCode() : 0);
                }
            case TracorDataPropertyTypeValue.Uuid: {
                    return HashCode.Combine(
                        obj.TypeValue,
                        obj.TryGetUuidValue(out var value) ? value.GetHashCode() : 0);
                }
            case TracorDataPropertyTypeValue.Any:
                return HashCode.Combine(
                        obj.TypeValue);
            default:
                return 0;
        }
    }
}