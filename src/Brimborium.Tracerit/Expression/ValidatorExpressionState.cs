
namespace Brimborium.Tracerit.Expression;

public abstract class ValidatorExpressionState {
    protected ValidatorExpressionState() { 
    }

    protected ValidatorExpressionState(TracorValidatorOnTraceResult result) {
        this.Result = result;
    }

    public TracorValidatorOnTraceResult Result = TracorValidatorOnTraceResult.None;

    internal protected abstract ValidatorExpressionState Copy();
}