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

    /// <summary>
    /// Wraps a function with three parameters and caller expression information for use in Tracor expressions.
    /// </summary>
    /// <typeparam name="T1">The first input type of the function.</typeparam>
    /// <typeparam name="T2">The second input type of the function.</typeparam>
    /// <typeparam name="T3">The third input type of the function.</typeparam>
    /// <typeparam name="TResult">The return type of the function.</typeparam>
    /// <param name="fn">The function to wrap.</param>
    /// <param name="doNotPopulateThisValue">Automatically populated with the function expression string.</param>
    /// <returns>A wrapped function with expression information.</returns>
    public static WrapFunc<T1, T2, T3, TResult> Wrap<T1, T2, T3, TResult>(
        Func<T1, T2, T3, TResult> fn,
        [CallerArgumentExpression(nameof(fn))] string? doNotPopulateThisValue = null)
        => new(fn, doNotPopulateThisValue);
}

public static partial class TracorExtension {
    /// <summary>
    /// Creates a predicate condition from a wrapped function that evaluates trace data.
    /// </summary>
    /// <param name="fnCondition">The wrapped function that evaluates trace data and returns a boolean.</param>
    /// <returns>A predicate condition for use in validator expressions.</returns>
    public static PredicateCondition Predicate(
        this WrapFunc<ITracorData, bool> fnCondition
    ) => new PredicateCondition(fnCondition.Value, fnCondition.ValueDisplay);

    /// <summary>
    /// Creates a predicate condition from a wrapped function that evaluates trace data with detailed result.
    /// </summary>
    /// <param name="fnCondition">The wrapped function that evaluates trace data and returns a validation result.</param>
    /// <returns>A predicate condition for use in validator expressions.</returns>
    public static PredicateCondition Predicate(
        this WrapFunc<ITracorData, TracorValidatorOnTraceResult> fnCondition
        ) => new PredicateCondition(fnCondition.Value, fnCondition.ValueDisplay);

    /// <summary>
    /// Creates a predicate condition from a function that evaluates trace data.
    /// </summary>
    /// <param name="fnCondition">The function that evaluates trace data and returns a boolean.</param>
    /// <param name="doNotPopulateThisValue">Automatically populated with the function expression string.</param>
    /// <returns>A predicate condition for use in validator expressions.</returns>
    public static PredicateCondition Predicate(
        Func<ITracorData, bool> fnCondition,
        [CallerArgumentExpression(nameof(fnCondition))] string? doNotPopulateThisValue = null
        ) => new PredicateCondition(fnCondition, doNotPopulateThisValue);

    /// <summary>
    /// Creates a predicate condition from a function that evaluates trace data with detailed result.
    /// </summary>
    /// <param name="fnCondition">The function that evaluates trace data and returns a validation result.</param>
    /// <param name="doNotPopulateThisValue">Automatically populated with the function expression string.</param>
    /// <returns>A predicate condition for use in validator expressions.</returns>
    public static PredicateCondition Predicate(
        Func<ITracorData, TracorValidatorOnTraceResult> fnCondition,
        [CallerArgumentExpression(nameof(fnCondition))] string? doNotPopulateThisValue = null
        ) => new PredicateCondition(fnCondition, doNotPopulateThisValue);

    /// <summary>
    /// Creates a predicate condition from a wrapped function that evaluates trace data with global state.
    /// </summary>
    /// <param name="fnCondition">The wrapped function that evaluates trace data and global state, returning a boolean.</param>
    /// <returns>A predicate condition for use in validator expressions.</returns>
    public static PredicateCondition Predicate(
        this WrapFunc<ITracorData, TracorGlobalState, bool> fnCondition
        ) => new PredicateCondition(fnCondition.Value, fnCondition.ValueDisplay);

    /// <summary>
    /// Creates a predicate condition from a wrapped function that evaluates trace data with global state and detailed result.
    /// </summary>
    /// <param name="fnCondition">The wrapped function that evaluates trace data and global state, returning a validation result.</param>
    /// <returns>A predicate condition for use in validator expressions.</returns>
    public static PredicateCondition Predicate(
        this WrapFunc<ITracorData, TracorGlobalState, TracorValidatorOnTraceResult> fnCondition
        ) => new PredicateCondition(fnCondition.Value, fnCondition.ValueDisplay);

    /// <summary>
    /// Creates a predicate condition from a function that evaluates trace data with global state.
    /// </summary>
    /// <param name="fnCondition">The function that evaluates trace data and global state, returning a boolean.</param>
    /// <param name="doNotPopulateThisValue">Automatically populated with the function expression string.</param>
    /// <returns>A predicate condition for use in validator expressions.</returns>
    public static PredicateCondition Predicate(
        Func<ITracorData, TracorGlobalState, bool> fnCondition,
        [CallerArgumentExpression(nameof(fnCondition))] string? doNotPopulateThisValue = null
        ) => new PredicateCondition(fnCondition, doNotPopulateThisValue);

    /// <summary>
    /// Creates a predicate condition from a function that evaluates trace data with global state and detailed result.
    /// </summary>
    /// <param name="fnCondition">The function that evaluates trace data and global state, returning a validation result.</param>
    /// <param name="doNotPopulateThisValue">Automatically populated with the function expression string.</param>
    /// <returns>A predicate condition for use in validator expressions.</returns>
    public static PredicateCondition Predicate(
        Func<ITracorData, TracorGlobalState, TracorValidatorOnTraceResult> fnCondition,
        [CallerArgumentExpression(nameof(fnCondition))] string? doNotPopulateThisValue = null
        ) => new PredicateCondition(fnCondition, doNotPopulateThisValue);

#if false
    public static PredicateTracorDataCondition Predicate(
        this WrapFunc<ITracorData, bool> fnCondition
        ) => new(fnCondition.Value, fnCondition.ValueDisplay);

    public static PredicateTracorDataCondition Predicate(
        this WrapFunc<ITracorData, TracorValidatorOnTraceResult> fnCondition
        ) => new(fnCondition.Value, fnCondition.ValueDisplay);

    public static PredicateTracorDataCondition<TTracorData> PredicateT<TTracorData>(
        this WrapFunc<TTracorData, bool> fnCondition
        )
        where TTracorData : ITracorData
        => new(fnCondition.Value, fnCondition.ValueDisplay);

    public static PredicateTracorDataCondition<TTracorData> PredicateT<TTracorData>(
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
#endif
}

public static partial class TracorExtension {

    /// <summary>
    /// Creates a match expression that tests trace data against a typed condition.
    /// </summary>
    /// <typeparam name="TValue">The type of value being matched.</typeparam>
    /// <param name="expressionCondition">The condition to evaluate against trace data.</param>
    /// <param name="label">An optional label for identifying this match in validation results.</param>
    /// <param name="listChild">Child expressions to evaluate when this condition matches.</param>
    /// <returns>A validator expression representing the match condition.</returns>
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

    /// <summary>
    /// Creates a match expression that tests trace data against a condition.
    /// </summary>
    /// <param name="expressionCondition">The condition to evaluate against trace data.</param>
    /// <param name="label">An optional label for identifying this match in validation results.</param>
    /// <param name="listChild">Child expressions to evaluate when this condition matches.</param>
    /// <returns>A validator expression representing the match condition.</returns>
    public static IValidatorExpression Match(
        IExpressionCondition expressionCondition,
        string? label = default,
        params IValidatorExpression[] listChild) {
        return new MatchExpression(
            label: label,
            condition: expressionCondition,
            listChild: listChild);
    }

    /// <summary>
    /// Creates a filter expression from a condition that filters trace data before processing.
    /// </summary>
    /// <param name="condition">The condition to filter trace data.</param>
    /// <param name="label">An optional label for identifying this filter in validation results.</param>
    /// <param name="listChild">Child expressions to evaluate for trace data that passes the filter.</param>
    /// <returns>A filter expression for use in validators.</returns>
    public static FilterExpression FilterExpression(
        this IExpressionCondition condition,
        string? label = default,
        params IValidatorExpression[] listChild) {
        return new(label, condition, listChild);
    }

    /// <summary>
    /// Creates a filter expression that filters trace data before processing.
    /// </summary>
    /// <param name="condition">The condition to filter trace data.</param>
    /// <param name="label">An optional label for identifying this filter in validation results.</param>
    /// <param name="listChild">Child expressions to evaluate for trace data that passes the filter.</param>
    /// <returns>A filter expression for use in validators.</returns>
    public static FilterExpression Filter(
        IExpressionCondition condition,
        string? label = default,
        params IValidatorExpression[] listChild) {
        return new(label, condition, listChild);
    }
}

#if false
public static partial class TracorExtension {

    /// <summary>
    /// Sets a property value in the dictionary, using the property name as the key.
    /// </summary>
    /// <param name="that">The dictionary to add the property to.</param>
    /// <param name="property">The property to add or update in the dictionary.</param>
    /// <returns>The dictionary for fluent chaining.</returns>
    public static Dictionary<string, TracorDataProperty> SetValue(
        this Dictionary<string, TracorDataProperty> that,
        TracorDataProperty property) {
        that[property.Name] = property;
        return that;
    }
}
#endif