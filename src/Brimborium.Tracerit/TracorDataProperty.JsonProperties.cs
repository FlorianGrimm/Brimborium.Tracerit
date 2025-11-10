#pragma warning disable IDE0009 // Member access should be qualified.

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text.Json.Serialization;

namespace Brimborium.Tracerit;

public partial struct TracorDataProperty {
    /*
        "StringValue",
        "BoolValue", 
        "IntValue", 
        "DoubleValue", 
        "ArrayValue", 
        "KvlistValue", 
        "BytesValue" 
    */

    [JsonIgnore]
    public string TypeName {
        readonly get => TracorDataUtility.TracorDataPropertyConvertTypeValueToString(_TypeValue, null);
        set {
            var (typeValue, typeName) = TracorDataUtility.TracorDataPropertyConvertStringToTypeName(value);
            _TypeValue = typeValue;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [JsonPropertyName("type_Value"), JsonInclude]
    public TracorDataPropertyTypeValue TypeValue {
        readonly get {
            return _TypeValue;
        }
        set {
            _TypeValue = value;
        }
    }

    /*
    [EditorBrowsable(EditorBrowsableState.Never)]
    [JsonPropertyName("text_Value"), JsonInclude()]
    public string? InnerTextValue {
        get => _InnerTextValue;
        set {
            _TypeValue = TracorDataPropertyTypeValue.String;
            _InnerTextValue = value;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [JsonPropertyName("long_Value"), JsonInclude()]
    public long InnerLongValue {
        readonly get {
            if (TracorDataPropertyTypeValue.Integer == _TypeValue) {
                if (MemoryMarshal.TryRead(GetValueReadSpan(), out long result)) {
                    return result;
                }
            }
            return 0L;
        }
        set {
            _TypeValue = TracorDataPropertyTypeValue.Integer;
            MemoryMarshal.TryWrite(GetValueWriteSpan(), value);
        }
    }


    [EditorBrowsable(EditorBrowsableState.Never)]
    [JsonPropertyName("double_Value"), JsonInclude()]
    public double InnerDoubleValue {
        readonly get {
            if (TracorDataPropertyTypeValue.Float == _TypeValue) {
                if (MemoryMarshal.TryRead(GetValueReadSpan(), out double result)) {
                    return result;
                }
            }
            return 0L;
        }
        set {
            _TypeValue = TracorDataPropertyTypeValue.Float;
            MemoryMarshal.TryWrite(GetValueWriteSpan(), value);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [JsonPropertyName("uuid_Value"), JsonInclude()]
    public Guid InnerUuidValue {
        readonly get {
            if (TracorDataPropertyTypeValue.Uuid == _TypeValue) {
                return new Guid(GetValueReadSpan());
            }
            return Guid.Empty;
        }
        set {
            _TypeValue = TracorDataPropertyTypeValue.Uuid;
            value.TryWriteBytes(GetValueWriteSpan());
        }
    }
    */
}