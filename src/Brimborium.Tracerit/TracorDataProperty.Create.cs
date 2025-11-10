#pragma warning disable IDE0079
#pragma warning disable IDE0009

namespace Brimborium.Tracerit;

public partial struct TracorDataProperty {
    
    public static TracorDataProperty Create(string argName, object? argValueQ) {
        if (argValueQ is not { } argValueNotNull) {
            return CreateNull(argName);
        }
        {
            if (TracorDataUtility.TryCastObjectToUuidValue(argValueNotNull, out var resultValue)) {
                return CreateUuidValue(argName, resultValue);
            }
        }
        {
            if (TracorDataUtility.TryCastObjectToStringValue(argValueNotNull, out var resultValue)) {
                return CreateStringValue(argName, resultValue);
            }
        }
        {
            if (TracorDataUtility.TryCastObjectToInteger(argValueNotNull, out var resultValue)) {
                return CreateIntegerValue(argName, resultValue);
            }
        }
        {
            if (TracorDataUtility.TryCastObjectToLogLevelValue(argValueNotNull, out var resultValue)) {
                return CreateLevelValue(argName, resultValue);
            }
        }
        {
            if (argValueNotNull is { } && argValueNotNull.GetType().IsEnum) {
                return CreateEnumValue(argName, argValueNotNull);
            }
        }
        {
            if (TracorDataUtility.TryCastObjectToDoubleValue(argValueNotNull, out var resultValue)) {
                return CreateDoubleValue(argName, resultValue);
            }
        }
        {
            if (TracorDataUtility.TryCastObjectToDateTimeValue(argValueNotNull, out var resultValue)) {
                return CreateDateTimeValue(argName, resultValue);
            }
        }
        {
            if (TracorDataUtility.TryCastObjectToDateTimeOffsetValue(argValueNotNull, out var resultValue)) {
                return CreateDateTimeOffsetValue(argName, resultValue);
            }
        }
        {
            if (TracorDataUtility.TryCastObjectToDurationValue(argValueNotNull, out var resultValue)) {
                return CreateDurationValue(argName, resultValue);
            }
        }
        {
            var result = new TracorDataProperty(argName);
            result.SetAnyValue(argValueNotNull);
            return result;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty Create(string argName, string argValue)
        => new TracorDataProperty(argName, argValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty Create(string argName, long argValue)
        => new TracorDataProperty(argName, argValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty Create(string argName, bool argValue)
        => new TracorDataProperty(argName, argValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty Create<T>(string argName, T argValue)
        where T : struct, Enum
        => new TracorDataProperty(argName, argValue.ToString(), TracorDataPropertyTypeValue.Enum);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty Create(string argName, LogLevel argValue)
        => new TracorDataProperty(argName, argValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty Create(string argName, double argValue)
        => new TracorDataProperty(argName, argValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty Create(string argName, DateTime argValue)
        => new TracorDataProperty(argName, argValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty Create(string argName, DateTimeOffset argValue)
        => new TracorDataProperty(argName, argValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty Create(string argName, TimeSpan argValue)
        => new TracorDataProperty(argName, argValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty Create(string argName, Guid argValue)
        => new TracorDataProperty(argName, argValue);

    //

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty CreateNull(string argName)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.Null,
            textValue: string.Empty
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty CreateStringValue(string argName, string argValue)
        => new TracorDataProperty(argName, argValue);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty CreateIntegerValue(string argName, long argValue) 
        => new TracorDataProperty(argName, argValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty CreateBoolean(string argName, bool argValue) 
        => new TracorDataProperty(argName, argValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty CreateEnumValue<T>(string argName, T argValue)
        where T : struct, Enum
        => new TracorDataProperty(argName, argValue.ToString(), TracorDataPropertyTypeValue.Enum);

    [MethodImpl(MethodImplOptions.AggressiveInlining)] 
    public static TracorDataProperty CreateEnumValue(string argName, object argValue) => new TracorDataProperty(argName, argValue.ToString()!, TracorDataPropertyTypeValue.Enum);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty CreateLevelValue(string argName, LogLevel argValue) 
        => new TracorDataProperty(argName, argValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty CreateDoubleValue(string argName, double argValue) 
        => new TracorDataProperty(argName, argValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty CreateDateTimeValue(string argName, DateTime argValue) 
        => new TracorDataProperty(argName, argValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty CreateDateTimeOffsetValue(string argName, DateTimeOffset argValue) 
        => new TracorDataProperty(argName, argValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty CreateDurationValue(string argName, TimeSpan argValue) 
        => new TracorDataProperty(argName, argValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TracorDataProperty CreateUuidValue(string argName, Guid argValue) 
        => new TracorDataProperty(argName, argValue);
}