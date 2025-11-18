namespace Brimborium.Tracerit;

#if false


$t=@("bool",
 "byte",
 "DateOnly",
 "DateTime",
 "DateTimeOffset",
 "decimal",
 "double",
 "float",
 "Guid",
 "int",
 "LogLevel",
 "long",
 "sbyte",
 "short",
 "string",
 "TimeSpan",
 "uint",
 "ulong",
 "ushort",
 "nint",
 "nuint"
 )

$t | Sort-Object | % {
    $type=$_
    $name=$type.Substring(0,1).ToUpperInvariant()+$type.Substring(1)
"
 internal sealed class TracorConvert$($name)ValueToListProperty
    : TracorConvertValueToListProperty<$($type)> {
    
    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, $($type) value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value));
    }
}
"
} | set-clipboard


$t | Sort-Object | % {
    $type=$_
    $name=$type.Substring(0,1).ToUpperInvariant()+$type.Substring(1)
"new TracorConvert$($name)ValueToListProperty(),"
} | set-clipboard


#endif

internal sealed class TracorConvertBoolValueToListProperty
   : TracorConvertValueToListProperty<bool> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, bool value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value));
    }
}


internal sealed class TracorConvertByteValueToListProperty
   : TracorConvertValueToListProperty<byte> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, byte value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value));
    }
}


internal sealed class TracorConvertDateOnlyValueToListProperty
   : TracorConvertValueToListProperty<DateOnly> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, DateOnly value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value.ToDateTime(new TimeOnly())));
    }
}


internal sealed class TracorConvertDateTimeValueToListProperty
   : TracorConvertValueToListProperty<DateTime> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, DateTime value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value));
    }
}


internal sealed class TracorConvertDateTimeOffsetValueToListProperty
   : TracorConvertValueToListProperty<DateTimeOffset> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, DateTimeOffset value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value));
    }
}


internal sealed class TracorConvertDecimalValueToListProperty
   : TracorConvertValueToListProperty<decimal> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, decimal value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, (double)value));
    }
}


internal sealed class TracorConvertDoubleValueToListProperty
   : TracorConvertValueToListProperty<double> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, double value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value));
    }
}


internal sealed class TracorConvertFloatValueToListProperty
   : TracorConvertValueToListProperty<float> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, float value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value));
    }
}


internal sealed class TracorConvertGuidValueToListProperty
   : TracorConvertValueToListProperty<Guid> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, Guid value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value));
    }
}


internal sealed class TracorConvertIntValueToListProperty
   : TracorConvertValueToListProperty<int> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, int value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value));
    }
}


internal sealed class TracorConvertLogLevelValueToListProperty
   : TracorConvertValueToListProperty<LogLevel> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, LogLevel value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value));
    }
}


internal sealed class TracorConvertLongValueToListProperty
   : TracorConvertValueToListProperty<long> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, long value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value));
    }
}


internal sealed class TracorConvertNintValueToListProperty
   : TracorConvertValueToListProperty<nint> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, nint value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value));
    }
}


internal sealed class TracorConvertNuintValueToListProperty
   : TracorConvertValueToListProperty<nuint> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, nuint value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value));
    }
}


internal sealed class TracorConvertSbyteValueToListProperty
   : TracorConvertValueToListProperty<sbyte> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, sbyte value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value));
    }
}


internal sealed class TracorConvertShortValueToListProperty
   : TracorConvertValueToListProperty<short> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, short value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value));
    }
}


internal sealed class TracorConvertStringValueToListProperty
   : TracorConvertValueToListProperty<string> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, string value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value));
    }
}


internal sealed class TracorConvertTimeSpanValueToListProperty
   : TracorConvertValueToListProperty<TimeSpan> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, TimeSpan value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value));
    }
}


internal sealed class TracorConvertUintValueToListProperty
   : TracorConvertValueToListProperty<uint> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, uint value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value));
    }
}


internal sealed class TracorConvertUlongValueToListProperty
   : TracorConvertValueToListProperty<ulong> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, ulong value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value));
    }
}


internal sealed class TracorConvertUshortValueToListProperty
   : TracorConvertValueToListProperty<ushort> {

    public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, ushort value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
        name = name is { Length: > 0 } ? name : "value";
        listProperty.Add(new TracorDataProperty(name, value));
    }
}
