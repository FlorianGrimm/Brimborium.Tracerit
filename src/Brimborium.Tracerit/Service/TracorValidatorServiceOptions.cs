namespace Brimborium.Tracerit.Service;

public class TracorValidatorServiceOptions {
    public bool? EnableFinished { get; set; } = true;

    public List<IValidatorExpression> ListValidator { get; } = new();

    public TracorValidatorServiceOptions AddValidator(IValidatorExpression validator) { 
        this.ListValidator.Add(validator);
        return this;
    }

    /*
    public List<Type> ListValidatorType { get; } = new();
    public TracorValidatorServiceOptions AddValidator<T>()
        where T: IValidatorExpression {
        this.ListValidatorType.Add(typeof(T));
        return this;
    }
    */
}

