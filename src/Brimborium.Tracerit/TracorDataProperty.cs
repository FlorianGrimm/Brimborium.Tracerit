namespace Brimborium.Tracerit;

public partial struct TracorDataProperty {
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

    public TracorDataPropertyOpertation Operation { get; set; } = TracorDataPropertyOpertation.Data;

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

    private const char _SeperationJsonChar = ':';

    public readonly void ToMinimizeString(StringBuilder sbOut) {
        sbOut
            .Append(this.Name)
            .Append(_SeperationJsonChar)
            .Append(TracorDataUtility.TracorDataPropertyOpertationToString(this.Operation))
            .Append(_SeperationJsonChar)
            .Append(this.TypeName)
            .Append(_SeperationJsonChar)
            .Append(this.TextValue)
            ;
    }

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
    