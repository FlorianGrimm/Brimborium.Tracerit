namespace Brimborium.Tracerit.Condition;

public sealed class EqualsTracorDataFuncCondition<TProperty> : IExpressionCondition {
    private readonly Func<ITracorData, TProperty> _FnGetProperty;
    private readonly TProperty _ExpectedValue;
    private readonly Func<TProperty, TProperty, bool> _FnEquality;
    private readonly string? _SetGlobalState;
    private readonly string? _FnGetPropertyDisplay;

    public EqualsTracorDataFuncCondition(
        Func<ITracorData, TProperty> fnGetProperty,
        TProperty expectedValue,
        Func<TProperty, TProperty, bool>? fnEquality = default,
        string? setGlobalState = default,
        [CallerArgumentExpression(nameof(fnGetProperty))] string? doNotPopulateThisValue = null
        ) {
        this._FnGetProperty = fnGetProperty;
        this._ExpectedValue = expectedValue;
        this._FnEquality = fnEquality ?? EqualityComparer<TProperty>.Default.Equals;
        this._SetGlobalState = setGlobalState;
        this._FnGetPropertyDisplay = doNotPopulateThisValue;
    }
    public bool DoesMatch(ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        var propertyValue = this._FnGetProperty(tracorData);
        var result = this._FnEquality(propertyValue, this._ExpectedValue);
        currentContext.LoggerUtility.LogCondition(tracorData.TracorIdentifier, result, $"{this._FnGetPropertyDisplay} == {this._ExpectedValue}");
        if (result) {
            if (this._SetGlobalState is { Length: > 0 } setGlobalState) {
                if (propertyValue is not null) {
                    currentContext.GlobalState[setGlobalState] = TracorDataProperty.Create(setGlobalState, propertyValue);
                }
            }
        }
        return result;
    }

    public static CalleeCondition operator /(TracorIdentifier expected, EqualsTracorDataFuncCondition<TProperty> and) {
        return new CalleeCondition(expected, and);
    }

}

public sealed class EqualPropertyNameCondition<TProperty> : IExpressionCondition {
    private readonly string _Property;
    private readonly TProperty _ExpectedValue;
    private readonly Func<TProperty, TProperty, bool> _FnEquality;
    private readonly string? _SetGlobalState;

    public EqualPropertyNameCondition(
        string property,
        TProperty expectedValue,
        Func<TProperty, TProperty, bool>? fnEquality = default,
        string? setGlobalState = default
        ) {
        this._Property = property;
        this._ExpectedValue = expectedValue;
        this._FnEquality = fnEquality ?? EqualityComparer<TProperty>.Default.Equals;
        this._SetGlobalState = setGlobalState;
    }
    public bool DoesMatch(ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        if (tracorData.TryGetPropertyValue<TProperty>(this._Property, out var propertyValue)) {
            var result = this._FnEquality(propertyValue, this._ExpectedValue);
            currentContext.LoggerUtility.LogCondition(tracorData.TracorIdentifier, result, $"{this._Property} == {this._ExpectedValue}");
            if (result) {
                if (this._SetGlobalState is { Length: > 0 } setGlobalState) {
                    if (propertyValue is not null) {
                        currentContext.GlobalState[setGlobalState] = TracorDataProperty.Create(setGlobalState, propertyValue);
                    }
                }
            }
            return result;
        }
        return false;
    }
}

public sealed class EqualsTracorDataPropertyCondition<TValue, TProperty> : IExpressionCondition {
    private readonly Func<TValue, TProperty> _FnGetProperty;
    private readonly TProperty _ExpectedValue;
    private readonly Func<TProperty, TProperty, bool> _FnEquality;
    private readonly string? _SetGlobalState;
    private readonly string? _FnGetPropertyDisplay;

    public EqualsTracorDataPropertyCondition(
        Func<TValue, TProperty> fnGetProperty,
        TProperty expectedValue,
        Func<TProperty, TProperty, bool>? fnEquality = default,
        string? setGlobalState = default,
        [CallerArgumentExpression(nameof(fnGetProperty))] string? doNotPopulateThisValue = null
        ) {
        this._FnGetProperty = fnGetProperty;
        this._ExpectedValue = expectedValue;
        this._FnEquality = fnEquality ?? EqualityComparer<TProperty>.Default.Equals;
        this._SetGlobalState = setGlobalState;
        this._FnGetPropertyDisplay = doNotPopulateThisValue;
    }
    public bool DoesMatch(ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        if (tracorData is ITracorData<TValue> tracorDataTyped
            && tracorDataTyped.TryGetOriginalValue(out var value)) {
            var propertyValue = this._FnGetProperty(value);
            var result = this._FnEquality(propertyValue, this._ExpectedValue);
            if (this._FnGetPropertyDisplay is { }) {
                currentContext.LoggerUtility.LogCondition(tracorData.TracorIdentifier, result, $"{this._FnGetPropertyDisplay} == {this._ExpectedValue}");
            } else {
                currentContext.LoggerUtility.LogCondition(tracorData.TracorIdentifier, result, default);
            }
            if (result) {
                if (this._SetGlobalState is { Length: > 0 } setGlobalState) {
                    if (propertyValue is not null) {
                        currentContext.GlobalState[setGlobalState] = TracorDataProperty.Create(setGlobalState, propertyValue);
                    }
                }
            }
            return result;
        }
        return false;
    }

    public static CalleeCondition operator /(TracorIdentifier expected, EqualsTracorDataPropertyCondition<TValue, TProperty> and) {
        return new CalleeCondition(expected, and);
    }

    public static AndCondition operator *(IExpressionCondition left, EqualsTracorDataPropertyCondition<TValue, TProperty> right) {
        if (left is AndCondition andCondition) {
            return new AndCondition([.. andCondition.ExpressionConditions, right]);
        } else {
            return new AndCondition(left, right);
        }
    }
}

public static class EqualsConditionExtension {
    public static EqualsTracorDataFuncCondition<TProperty> Equals<TProperty>(
        this WrapFunc<ITracorData, TProperty> fnGetProperty,
        TProperty expectedValue,
        Func<TProperty, TProperty, bool>? fnEquality = default,
        string? setGlobalState = default,
        [CallerArgumentExpression(nameof(fnGetProperty))] string? doNotPopulateThisValue = null
        )
        => new(fnGetProperty.Value, expectedValue, fnEquality, setGlobalState, doNotPopulateThisValue);

    public static EqualPropertyNameCondition<TProperty> Equals<TProperty>(
        this string property,
        TProperty expectedValue,
        Func<TProperty, TProperty, bool>? fnEquality = default,
        string? setGlobalState = default
        )
        => new(property, expectedValue, fnEquality, setGlobalState);

    public static EqualsTracorDataPropertyCondition<TValue, TProperty> EqualsProperty<TValue, TProperty>(
        this WrapFunc<TValue, TProperty> fnGetProperty,
        TProperty expectedValue,
        Func<TProperty, TProperty, bool>? fnEquality = default,
        string? setGlobalState = default
        )
        => new(fnGetProperty.Value, expectedValue, fnEquality, setGlobalState, fnGetProperty.ValueDisplay
            );
}

