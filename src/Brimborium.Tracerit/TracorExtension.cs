namespace Brimborium.Tracerit;

/// <summary>
/// Represents a wrapped value for use in Tracor expressions and conditions.
/// </summary>
/// <typeparam name="TValue">The type of the wrapped value.</typeparam>
/// <param name="Value">The wrapped value.</param>
public record struct WrapValue<TValue>(TValue Value);

/// <summary>
/// Provides extension methods and utilities for working with Tracor expressions and conditions.
/// </summary>
public static partial class TracorExtension {
    /// <summary>
    /// Wraps a value for use in Tracor expressions.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to wrap.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A wrapped value.</returns>
    public static WrapValue<TValue> Wrap<TValue>(TValue value)
        => new(value);
}

/// <summary>
/// Represents a wrapped function with caller expression information for debugging and display purposes.
/// </summary>
/// <typeparam name="T">The input type of the function.</typeparam>
/// <typeparam name="TResult">The return type of the function.</typeparam>
/// <param name="Value">The wrapped function.</param>
/// <param name="ValueDisplay">The string representation of the function expression.</param>
public record struct WrapFunc<T, TResult>(
    Func<T, TResult> Value,
    [CallerArgumentExpression(nameof(Value))] string? ValueDisplay = null);

/// <summary>
/// Represents a wrapped function with two input parameters and caller expression information for debugging and display purposes.
/// </summary>
/// <typeparam name="T1">The first input type of the function.</typeparam>
/// <typeparam name="T2">The second input type of the function.</typeparam>
/// <typeparam name="TResult">The return type of the function.</typeparam>
/// <param name="Value">The wrapped function.</param>
/// <param name="ValueDisplay">The string representation of the function expression.</param>
public record struct WrapFunc<T1, T2, TResult>(
    Func<T1, T2, TResult> Value,
    [CallerArgumentExpression(nameof(Value))] string? ValueDisplay = null);

/// <summary>
/// Represents a wrapped function with two input parameters and caller expression information for debugging and display purposes.
/// </summary>
/// <typeparam name="T1">The first input type of the function.</typeparam>
/// <typeparam name="T2">The second input type of the function.</typeparam>
/// <typeparam name="TResult">The return type of the function.</typeparam>
/// <param name="Value">The wrapped function.</param>
/// <param name="ValueDisplay">The string representation of the function expression.</param>
public record struct WrapFunc<T1, T2, T3, TResult>(
    Func<T1, T2, T3, TResult> Value,
    [CallerArgumentExpression(nameof(Value))] string? ValueDisplay = null);


public static partial class TracorExtension {
    /// <summary>
    /// Wraps a function with caller expression information for use in Tracor expressions.
    /// </summary>
    /// <typeparam name="TValue">The input type of the function.</typeparam>
    /// <typeparam name="TResult">The return type of the function.</typeparam>
    /// <param name="fn">The function to wrap.</param>
    /// <param name="doNotPopulateThisValue">Automatically populated with the function expression string.</param>
    /// <returns>A wrapped function with expression information.</returns>
    public static WrapFunc<TValue, TResult> Wrap<TValue, TResult>(
        Func<TValue, TResult> fn,
        [CallerArgumentExpression(nameof(fn))] string? doNotPopulateThisValue = null)
        => new(fn, doNotPopulateThisValue);

    /// <summary>
    /// Wraps a function with two parameters and caller expression information for use in Tracor expressions.
    /// </summary>
    /// <typeparam name="T1">The first input type of the function.</typeparam>
    /// <typeparam name="T2">The second input type of the function.</typeparam>
    /// <typeparam name="TResult">The return type of the function.</typeparam>
    /// <param name="fn">The function to wrap.</param>
    /// <param name="doNotPopulateThisValue">Automatically populated with the function expression string.</param>
    /// <returns>A wrapped function with expression information.</returns>
    public static WrapFunc<T1, T2, TResult> Wrap<T1, T2, TResult>(
        Func<T1, T2, TResult> fn,
        [CallerArgumentExpression(nameof(fn))] string? doNotPopulateThisValue = null)
        => new(fn, doNotPopulateThisValue);
}

public static partial class TracorExtension {
    public static PredicateCondition Predicate(
        this WrapFunc<ITracorData, TracorForkState, TracorGlobalState, bool> fnCondition
        ) => new PredicateCondition(fnCondition.Value, fnCondition.ValueDisplay);

    public static PredicateCondition Predicate(
        this WrapFunc<ITracorData, TracorForkState, TracorGlobalState, TracorValidatorOnTraceResult> fnCondition
        ) => new PredicateCondition(fnCondition.Value, fnCondition.ValueDisplay);
    
    public static PredicateCondition Predicate(
        Func<ITracorData, TracorForkState, TracorGlobalState, bool> fnCondition,
        [CallerArgumentExpression(nameof(fnCondition))] string? doNotPopulateThisValue = null
        ) => new PredicateCondition(fnCondition, doNotPopulateThisValue);

    public static PredicateCondition Predicate(
        Func<ITracorData, TracorForkState, TracorGlobalState, TracorValidatorOnTraceResult> fnCondition,
        [CallerArgumentExpression(nameof(fnCondition))] string? doNotPopulateThisValue = null
        ) => new PredicateCondition(fnCondition, doNotPopulateThisValue);

    public static PredicateTracorDataCondition PredicateTracorData(
        this WrapFunc<ITracorData, bool> fnCondition
        ) => new(fnCondition.Value, fnCondition.ValueDisplay);

    public static PredicateTracorDataCondition PredicateTracorData(
        this WrapFunc<ITracorData, TracorValidatorOnTraceResult> fnCondition
        ) => new(fnCondition.Value, fnCondition.ValueDisplay);

    public static PredicateTracorDataCondition<TTracorData> PredicateTracorData<TTracorData>(
        this WrapFunc<TTracorData, bool> fnCondition
        )
        where TTracorData : ITracorData
        => new(fnCondition.Value, fnCondition.ValueDisplay);

    public static PredicateTracorDataCondition<TTracorData> PredicateTracorData<TTracorData>(
        this WrapFunc<TTracorData, TracorValidatorOnTraceResult> fnCondition
        )
        where TTracorData : ITracorData
        => new(fnCondition.Value, fnCondition.ValueDisplay);

    public static PredicateValueCondition<TValue> PredicateValue<TValue>(
        this WrapFunc<TValue, bool> fnCondition
        ) => new(fnCondition.Value, fnCondition.ValueDisplay);

    public static PredicateValueCondition<TValue> PredicateValue<TValue>(
        this WrapFunc<TValue, TracorValidatorOnTraceResult> fnCondition
        ) => new(fnCondition.Value, fnCondition.ValueDisplay);

    public static PredicateTracorDataCondition Predicate(
        Func<ITracorData, bool> condition
        ) => new(condition);

    public static PredicateTracorDataCondition Predicate(
        Func<ITracorData, TracorValidatorOnTraceResult> condition
        ) => new(condition);

    public static PredicateTracorDataCondition<TTracorData> PredicateTracorData<TTracorData>(
        Func<TTracorData, bool> fnCondition,
        [CallerArgumentExpression(nameof(fnCondition))] string? doNotPopulateThisValue = null
        ) where TTracorData : ITracorData
        => new(fnCondition, doNotPopulateThisValue);

    public static PredicateTracorDataCondition<TTracorData> PredicateTracorData<TTracorData>(
        Func<TTracorData, TracorValidatorOnTraceResult> fnCondition,
        [CallerArgumentExpression(nameof(fnCondition))] string? doNotPopulateThisValue = null
        ) where TTracorData : ITracorData
        => new(fnCondition, doNotPopulateThisValue);

    public static PredicateValueCondition<TValue> PredicateValue<TValue>(
        Func<TValue, bool> fnCondition,
        [CallerArgumentExpression(nameof(fnCondition))] string? doNotPopulateThisValue = null
        ) => new(fnCondition, doNotPopulateThisValue);

    public static PredicateValueCondition<TValue> PredicateValue<TValue>(
        Func<TValue, TracorValidatorOnTraceResult> fnCondition,
        [CallerArgumentExpression(nameof(fnCondition))] string? doNotPopulateThisValue = null
        ) => new(fnCondition, doNotPopulateThisValue);
}

public static partial class TracorExtension {

    public static EqualsTracorDataFuncCondition<TProperty> EqualsTracorDataFunc<TProperty>(
        WrapFunc<ITracorData, TProperty> fnGetProperty,
        TProperty expectedValue,
        Func<TProperty, TProperty, bool>? fnEquality = default,
        string? setGlobalState = default
        )
        => new(fnGetProperty.Value, expectedValue, fnEquality, setGlobalState, fnGetProperty.ValueDisplay);

    public static EqualsTracorDataFuncCondition<TProperty> EqualsTracorDataFunc<TProperty>(
        Func<ITracorData, TProperty> fnGetProperty,
        TProperty expectedValue,
        Func<TProperty, TProperty, bool>? fnEquality = default,
        string? setGlobalState = default,
        [CallerArgumentExpression(nameof(fnGetProperty))] string? doNotPopulateThisValue = null
        )
        => new(fnGetProperty, expectedValue, fnEquality, setGlobalState, doNotPopulateThisValue);

    public static EqualPropertyNameCondition<TProperty> EqualPropertyName<TProperty>(
        string property,
        TProperty expectedValue,
        Func<TProperty, TProperty, bool>? fnEquality = default,
        string? setGlobalState = default
        )
        => new(property, expectedValue, fnEquality, setGlobalState);

    public static EqualsTracorDataPropertyCondition<TValue, TProperty> EqualsValue<TValue, TProperty>(
        this WrapFunc<TValue, TProperty> fnGetProperty,
        TProperty expectedValue,
        Func<TProperty, TProperty, bool>? fnEquality = default,
        string? setGlobalState = default)
        => new(
            fnGetProperty.Value, expectedValue, fnEquality, setGlobalState, fnGetProperty.ValueDisplay);

    public static EqualsTracorDataPropertyCondition<TValue, TProperty> EqualsValue<TValue, TProperty>(
        Func<TValue, TProperty> fnGetProperty,
        TProperty expectedValue,
        Func<TProperty, TProperty, bool>? fnEquality = default,
        string? setGlobalState = default,
        [CallerArgumentExpression(nameof(fnGetProperty))] string? doNotPopulateThisValue = null
        ) => new(
            fnGetProperty, expectedValue, fnEquality, setGlobalState, doNotPopulateThisValue);
}

public static partial class TracorExtension {

    public static IValidatorExpression Match<TValue>(
        IExpressionCondition<TValue> expressionCondition,
        string? label = default,
        params IValidatorExpression[] listChild
        ) {
        return new MatchExpression(
            label: label,
            condition: expressionCondition,
            listChild: listChild);
    }

    public static IValidatorExpression Match(
        IExpressionCondition expressionCondition,
        string? label = default,
        params IValidatorExpression[] listChild) {
        return new MatchExpression(
            label: label,
            condition: expressionCondition,
            listChild: listChild);
    }

    public static FilterExpression FilterExpression(
        this IExpressionCondition condition,
        string? label = default,
        params IValidatorExpression[] listChild) {
        return new(label, condition, listChild);
    }

    public static FilterExpression Filter(
        IExpressionCondition condition,
        string? label = default,
        params IValidatorExpression[] listChild) {
        return new(label, condition, listChild);
    }
}
