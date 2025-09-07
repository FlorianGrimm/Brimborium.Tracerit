namespace Brimborium.Tracerit;

public static class TracorExtension {
    public static WrapFunc<TValue, TProperty> Wrap<TValue, TProperty>(Func<TValue, TProperty> fn) {
        return new WrapFunc<TValue, TProperty>(fn);
    }
    public static WrapFunc<T1, T2, TProperty> Wrap<T1, T2, TProperty>(Func<T1, T2, TProperty> fn) {
        return new WrapFunc<T1, T2, TProperty>(fn);
    }


    public static PredicateTracorDataCondition Predicate(
        this WrapFunc<ITracorData, bool> condition
    ) => new PredicateTracorDataCondition(condition.Value);


    public static PredicateTracorDataCondition<TTracorData> PredicateTracorData<TTracorData>(
        this WrapFunc<TTracorData, bool> condition
        )
        where TTracorData : ITracorData
        => new PredicateTracorDataCondition<TTracorData>(condition.Value);

    public static PredicateValueCondition<TValue> PredicateValue<TValue>(
        this WrapFunc<TValue, bool> condition
        ) => new PredicateValueCondition<TValue>(condition.Value);

    public static PredicateValueGlobalStateCondition<TValue> PredicateValue<TValue>(
        this WrapFunc<TValue, TracorGlobalState, bool> condition
        ) => new PredicateValueGlobalStateCondition<TValue>(condition.Value);

    public static PredicatePropertyCondition<TValue, TProperty> PredicateProperty<TValue, TProperty>(
        this WrapFunc<TValue, TProperty> fnGetProperty,
        TProperty expectedValue,
        Func<TProperty, TProperty, bool>? fnEquality = default,
        string? setGlobalState = default
        ) => new PredicatePropertyCondition<TValue, TProperty>(
            fnGetProperty.Value, expectedValue, fnEquality, setGlobalState);


    public static PredicateTracorDataCondition Predicate(
        Func<ITracorData, bool> condition
        ) => new PredicateTracorDataCondition(condition);

    public static PredicateTracorDataCondition<TTracorData> PredicateTracorData<TTracorData>(
        Func<TTracorData, bool> condition
        ) where TTracorData : ITracorData
        => new PredicateTracorDataCondition<TTracorData>(condition);

    public static PredicateValueCondition<TValue> PredicateValue<TValue>(
        Func<TValue, bool> condition
        ) => new PredicateValueCondition<TValue>(condition);

    public static PredicateValueGlobalStateCondition<TValue> PredicateValue<TValue>(
        Func<TValue, TracorGlobalState, bool> condition
        ) => new PredicateValueGlobalStateCondition<TValue>(condition);

    public static PredicatePropertyCondition<TValue, TProperty> PredicateProperty<TValue, TProperty>(
        Func<TValue, TProperty> fnGetProperty,
        TProperty expectedValue,
        Func<TProperty, TProperty, bool>? fnEquality = default,
        string? setGlobalState = default
        ) => new PredicatePropertyCondition<TValue, TProperty>(
            fnGetProperty, expectedValue, fnEquality, setGlobalState);



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
}

public record struct WrapFunc<T, TResult>(Func<T, TResult> Value);
public record struct WrapFunc<T1, T2, TResult>(Func<T1, T2, TResult> Value);

