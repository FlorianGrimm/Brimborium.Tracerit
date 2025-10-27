

namespace Brimborium.Tracerit.DataAccessor;

public abstract class TracorDataValueAccessorFactory<TValue>
    : ITracorDataAccessorFactory<TValue> {
    protected readonly TracorDataRecordPool Pool;

    protected TracorDataValueAccessorFactory(
        TracorDataRecordPool tracorDataRecordPool
        ) {
        this.Pool = tracorDataRecordPool;
    }
    public bool TryGetData(object value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        if (typeof(TValue) == value?.GetType()) {
            return this.TryGetDataTyped((TValue)value, out tracorData);
        }
        tracorData = default;
        return false;
    }

    public abstract bool TryGetDataTyped(TValue value, [MaybeNullWhen(false)] out ITracorData tracorData);
}

public sealed class TracorDataStringValueAccessorFactory :
    TracorDataValueAccessorFactory<string> {
    public TracorDataStringValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

    public override bool TryGetDataTyped(
        string value,
        [MaybeNullWhen(false)] out ITracorData tracorData) {
        var result = this.Pool.Rent();
        var tracorDataProperty = TracorDataProperty.CreateStringValue("value", value);
        result.ListProperty.Add(tracorDataProperty);
        tracorData = result;
        return false;
    }
}

public sealed class TracorDataBoolValueAccessorFactory :
    TracorDataValueAccessorFactory<bool> {
    public TracorDataBoolValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

    public override bool TryGetDataTyped(
        bool value,
        [MaybeNullWhen(false)] out ITracorData tracorData) {
        var result = this.Pool.Rent();
        var tracorDataProperty = TracorDataProperty.CreateBoolean("value", value);
        result.ListProperty.Add(tracorDataProperty);
        tracorData = result;
        return false;
    }
}

public sealed class TracorDataIntValueAccessorFactory :
    TracorDataValueAccessorFactory<int> {
    public TracorDataIntValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

    public override bool TryGetDataTyped(
        int value,
        [MaybeNullWhen(false)] out ITracorData tracorData) {
        var result = this.Pool.Rent();
        var tracorDataProperty = TracorDataProperty.CreateIntegerValue("value", value);
        result.ListProperty.Add(tracorDataProperty);
        tracorData = result;
        return false;
    }
}

public sealed class TracorDataLongValueAccessorFactory :
    TracorDataValueAccessorFactory<long> {
    public TracorDataLongValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

    public override bool TryGetDataTyped(
        long value,
        [MaybeNullWhen(false)] out ITracorData tracorData) {
        var result = this.Pool.Rent();
        var tracorDataProperty = TracorDataProperty.CreateIntegerValue("value", value);
        result.ListProperty.Add(tracorDataProperty);
        tracorData = result;
        return false;
    }
}

public sealed class TracorDataFloatValueAccessorFactory :
    TracorDataValueAccessorFactory<float> {
    public TracorDataFloatValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

    public override bool TryGetDataTyped(
        float value,
        [MaybeNullWhen(false)] out ITracorData tracorData) {
        var result = this.Pool.Rent();
        var tracorDataProperty = TracorDataProperty.CreateDoubleValue("value", value);
        result.ListProperty.Add(tracorDataProperty);
        tracorData = result;
        return false;
    }
}

public sealed class TracorDataDoubleValueAccessorFactory :
    TracorDataValueAccessorFactory<double> {
    public TracorDataDoubleValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

    public override bool TryGetDataTyped(
        double value,
        [MaybeNullWhen(false)] out ITracorData tracorData) {
        var result = this.Pool.Rent();
        var tracorDataProperty = TracorDataProperty.CreateDoubleValue("value", value);
        result.ListProperty.Add(tracorDataProperty);
        tracorData = result;
        return false;
    }
}

public sealed class TracorDataDateTimeValueAccessorFactory :
    TracorDataValueAccessorFactory<DateTime> {
    public TracorDataDateTimeValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

    public override bool TryGetDataTyped(
        DateTime value,
        [MaybeNullWhen(false)] out ITracorData tracorData) {
        var result = this.Pool.Rent();
        var tracorDataProperty = TracorDataProperty.CreateDateTimeValue("value", value);
        result.ListProperty.Add(tracorDataProperty);
        tracorData = result;
        return false;
    }
}

public sealed class TracorDataDateTimeOffsetValueAccessorFactory :
    TracorDataValueAccessorFactory<DateTimeOffset> {
    public TracorDataDateTimeOffsetValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

    public override bool TryGetDataTyped(
        DateTimeOffset value,
        [MaybeNullWhen(false)] out ITracorData tracorData) {
        var result = this.Pool.Rent();
        var tracorDataProperty = TracorDataProperty.CreateDateTimeOffsetValue("value", value);
        result.ListProperty.Add(tracorDataProperty);
        tracorData = result;
        return false;
    }
}


public sealed class TracorDataUuidValueAccessorFactory :
    TracorDataValueAccessorFactory<Guid> {
    public TracorDataUuidValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

    public override bool TryGetDataTyped(
        Guid value,
        [MaybeNullWhen(false)] out ITracorData tracorData) {
        var result = this.Pool.Rent();
        var tracorDataProperty = TracorDataProperty.CreateUuidValue("value", value);
        result.ListProperty.Add(tracorDataProperty);
        tracorData = result;
        return false;
    }
}