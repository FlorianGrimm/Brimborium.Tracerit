//namespace Brimborium.Tracerit.DataAccessor;

//public abstract class TracorDataValueAccessorFactory<TValue>
//    : ITracorDataAccessorFactory<TValue> {
//#pragma warning disable IDE1006 // Naming Styles
//    protected readonly TracorDataRecordPool Pool;
//#pragma warning restore IDE1006 // Naming Styles

//    protected TracorDataValueAccessorFactory(
//        TracorDataRecordPool tracorDataRecordPool
//        ) {
//        this.Pool = tracorDataRecordPool;
//    }
//    public bool TryGetData(object value, [MaybeNullWhen(false)] out ITracorData tracorData) {
//        if (typeof(TValue) == value?.GetType()) {
//            return this.TryGetDataTyped((TValue)value, out tracorData);
//        }
//        tracorData = default;
//        return false;
//    }

//    public abstract bool TryGetDataTyped(TValue value, [MaybeNullWhen(false)] out ITracorData tracorData);
//}

//internal sealed class TracorDataStringValueAccessorFactory :
//    TracorDataValueAccessorFactory<string> {
//    public TracorDataStringValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

//    public override bool TryGetDataTyped(
//        string value,
//        [MaybeNullWhen(false)] out ITracorData tracorData) {
//        var result = this.Pool.Rent();
//        TracorDataUtility.SetActivity(result.ListProperty);
//        result.ListProperty.Add(new TracorDataProperty("value", value));
//        tracorData = result;
//        return false;
//    }
//}

//internal sealed class TracorDataBoolValueAccessorFactory :
//    TracorDataValueAccessorFactory<bool> {
//    public TracorDataBoolValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

//    public override bool TryGetDataTyped(
//        bool value,
//        [MaybeNullWhen(false)] out ITracorData tracorData) {
//        var result = this.Pool.Rent();
//        TracorDataUtility.SetActivity(result.ListProperty);
//        result.ListProperty.Add(new TracorDataProperty("value", value));
//        tracorData = result;
//        return false;
//    }
//}

//internal sealed class TracorDataByteValueAccessorFactory :
//    TracorDataValueAccessorFactory<byte> {
//    public TracorDataByteValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

//    public override bool TryGetDataTyped(
//        byte value,
//        [MaybeNullWhen(false)] out ITracorData tracorData) {
//        var result = this.Pool.Rent();
//        TracorDataUtility.SetActivity(result.ListProperty);
//        result.ListProperty.Add(new TracorDataProperty("value", (long)value));
//        tracorData = result;
//        return false;
//    }
//}

//internal sealed class TracorDataSByteValueAccessorFactory :
//    TracorDataValueAccessorFactory<sbyte> {
//    public TracorDataSByteValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

//    public override bool TryGetDataTyped(
//        sbyte value,
//        [MaybeNullWhen(false)] out ITracorData tracorData) {
//        var result = this.Pool.Rent();
//        TracorDataUtility.SetActivity(result.ListProperty);
//        result.ListProperty.Add(new TracorDataProperty("value", (long)value));
//        tracorData = result;
//        return false;
//    }
//}

//internal sealed class TracorDataShortValueAccessorFactory :
//    TracorDataValueAccessorFactory<short> {
//    public TracorDataShortValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

//    public override bool TryGetDataTyped(
//        short value,
//        [MaybeNullWhen(false)] out ITracorData tracorData) {
//        var result = this.Pool.Rent();
//        TracorDataUtility.SetActivity(result.ListProperty);
//        result.ListProperty.Add(new TracorDataProperty("value", value));
//        tracorData = result;
//        return false;
//    }
//}
//internal sealed class TracorDataUshortValueAccessorFactory :
//    TracorDataValueAccessorFactory<ushort> {
//    public TracorDataUshortValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

//    public override bool TryGetDataTyped(
//        ushort value,
//        [MaybeNullWhen(false)] out ITracorData tracorData) {
//        var result = this.Pool.Rent();
//        TracorDataUtility.SetActivity(result.ListProperty);
//        result.ListProperty.Add(new TracorDataProperty("value", value));
//        tracorData = result;
//        return false;
//    }
//}

//internal sealed class TracorDataNintValueAccessorFactory :
//    TracorDataValueAccessorFactory<nint> {
//    public TracorDataNintValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

//    public override bool TryGetDataTyped(
//        nint value,
//        [MaybeNullWhen(false)] out ITracorData tracorData) {
//        var result = this.Pool.Rent();
//        TracorDataUtility.SetActivity(result.ListProperty);
//        result.ListProperty.Add(new TracorDataProperty("value", value));
//        tracorData = result;
//        return false;
//    }
//}

//internal sealed class TracorDataNuintValueAccessorFactory :
//    TracorDataValueAccessorFactory<nuint> {
//    public TracorDataNuintValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

//    public override bool TryGetDataTyped(
//        nuint value,
//        [MaybeNullWhen(false)] out ITracorData tracorData) {
//        var result = this.Pool.Rent();
//        TracorDataUtility.SetActivity(result.ListProperty);
//        result.ListProperty.Add(new TracorDataProperty("value", value));
//        tracorData = result;
//        return false;
//    }
//}

//internal sealed class TracorDataIntValueAccessorFactory :
//    TracorDataValueAccessorFactory<int> {
//    public TracorDataIntValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

//    public override bool TryGetDataTyped(
//        int value,
//        [MaybeNullWhen(false)] out ITracorData tracorData) {
//        var result = this.Pool.Rent();
//        TracorDataUtility.SetActivity(result.ListProperty);
//        result.ListProperty.Add(new TracorDataProperty("value", value));
//        tracorData = result;
//        return false;
//    }
//}

//internal sealed class TracorDataUintValueAccessorFactory :
//    TracorDataValueAccessorFactory<uint> {
//    public TracorDataUintValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

//    public override bool TryGetDataTyped(
//        uint value,
//        [MaybeNullWhen(false)] out ITracorData tracorData) {
//        var result = this.Pool.Rent();
//        TracorDataUtility.SetActivity(result.ListProperty);
//        result.ListProperty.Add(new TracorDataProperty("value", value));
//        tracorData = result;
//        return false;
//    }
//}

//internal sealed class TracorDataLongValueAccessorFactory :
//    TracorDataValueAccessorFactory<long> {
//    public TracorDataLongValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

//    public override bool TryGetDataTyped(
//        long value,
//        [MaybeNullWhen(false)] out ITracorData tracorData) {
//        var result = this.Pool.Rent();
//        TracorDataUtility.SetActivity(result.ListProperty);
//        result.ListProperty.Add(new TracorDataProperty("value", value));
//        tracorData = result;
//        return false;
//    }
//}

//internal sealed class TracorDataFloatValueAccessorFactory :
//    TracorDataValueAccessorFactory<float> {
//    public TracorDataFloatValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

//    public override bool TryGetDataTyped(
//        float value,
//        [MaybeNullWhen(false)] out ITracorData tracorData) {
//        var result = this.Pool.Rent();
//        TracorDataUtility.SetActivity(result.ListProperty);
//        result.ListProperty.Add(new TracorDataProperty("value", value));
//        tracorData = result;
//        return false;
//    }
//}

//internal sealed class TracorDataDoubleValueAccessorFactory :
//    TracorDataValueAccessorFactory<double> {
//    public TracorDataDoubleValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

//    public override bool TryGetDataTyped(
//        double value,
//        [MaybeNullWhen(false)] out ITracorData tracorData) {
//        var result = this.Pool.Rent();
//        TracorDataUtility.SetActivity(result.ListProperty);
//        result.ListProperty.Add(new TracorDataProperty("value", value));
//        tracorData = result;
//        return false;
//    }
//}

//internal sealed class TracorDataDecimalValueAccessorFactory :
//    TracorDataValueAccessorFactory<decimal> {
//    public TracorDataDecimalValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

//    public override bool TryGetDataTyped(
//        decimal value,
//        [MaybeNullWhen(false)] out ITracorData tracorData) {
//        var result = this.Pool.Rent();
//        TracorDataUtility.SetActivity(result.ListProperty);
//        result.ListProperty.Add(new TracorDataProperty("value", value));
//        tracorData = result;
//        return false;
//    }
//}

//internal sealed class TracorDataDateTimeValueAccessorFactory :
//    TracorDataValueAccessorFactory<DateTime> {
//    public TracorDataDateTimeValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

//    public override bool TryGetDataTyped(
//        DateTime value,
//        [MaybeNullWhen(false)] out ITracorData tracorData) {
//        var result = this.Pool.Rent();
//        TracorDataUtility.SetActivity(result.ListProperty);
//        result.ListProperty.Add(new TracorDataProperty("value", value));
//        tracorData = result;
//        return false;
//    }
//}

//internal sealed class TracorDataDateTimeOffsetValueAccessorFactory :
//    TracorDataValueAccessorFactory<DateTimeOffset> {
//    public TracorDataDateTimeOffsetValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

//    public override bool TryGetDataTyped(
//        DateTimeOffset value,
//        [MaybeNullWhen(false)] out ITracorData tracorData) {
//        var result = this.Pool.Rent();
//        TracorDataUtility.SetActivity(result.ListProperty);
//        result.ListProperty.Add(new TracorDataProperty("value", value));
//        tracorData = result;
//        return false;
//    }
//}

//internal sealed class TracorDataUuidValueAccessorFactory :
//    TracorDataValueAccessorFactory<Guid> {
//    public TracorDataUuidValueAccessorFactory(TracorDataRecordPool tracorDataRecordPool) : base(tracorDataRecordPool) { }

//    public override bool TryGetDataTyped(
//        Guid value,
//        [MaybeNullWhen(false)] out ITracorData tracorData) {
//        var result = this.Pool.Rent();
//        TracorDataUtility.SetActivity(result.ListProperty);
//        result.ListProperty.Add(new TracorDataProperty("value", value));
//        tracorData = result;
//        return false;
//    }
//}