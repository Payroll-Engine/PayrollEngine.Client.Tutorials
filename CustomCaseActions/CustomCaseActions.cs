using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Function;
using PayrollEngine.Client.Scripting.Local;

namespace PayrollEngine.Client.Tutorial.Scripting.CustomCaseAction;

[ActionProvider("CustomActions", typeof(CaseChangeFunction))]
public class MyCaseBuildActions : CaseChangeActionsBase
{

    #region Validate

    [ActionIssue("MissingCaseValue", "Missing value (0)", 1)]
    [ActionIssue("InvalidUidBfs", "(0) with invalid UID/BFS (1)", 2)]
    [CaseValidateAction("UidBfs", "Validate for the swiss UID/BFS")]
    public void UidBfs(CaseChangeActionContext context)
    {
        var sourceValue = NewCaseFieldActionValue<string>(context);
        if (sourceValue?.ResolvedValue == null)
        {
            AddIssue(context, "MissingCaseValue", context.CaseFieldName);
            return;
        }

        try
        {
            if (!Switzerland.IsValidUidBfs(sourceValue.ResolvedValue))
            {
                AddIssue(context, "InvalidUidBfs", context.CaseFieldName, sourceValue.ResolvedValue);
            }
        }
        catch (CheckDigitException exception)
        {
            AddIssue(context, "InvalidUidBfs", context.CaseFieldName, exception.Message);
        }
    }

    #endregion

    #region Build

    [ActionParameter("factor", "Day factor (def=1)",
        valueTypes: new[] { DecimalType })]
    [ActionParameter("roundStep", "The rounding step size (def=1)",
        valueTypes: new[] { DecimalType })]
    [CaseBuildAction("DayFactor", "Value by day factor")]
    public void DayFactor(CaseChangeActionContext context, object factor = null, object roundStep = null)
    {
        // start and end date
        var start = context.Function.GetStart(context.CaseFieldName);
        var end = context.Function.GetEnd(context.CaseFieldName);
        if (!start.HasValue || !end.HasValue)
        {
            return;
        }

        // factor
        decimal resolvedFactor = 1;
        factor ??= 1;
        var factorValue = NewActionValue<decimal>(context, factor);
        if (factorValue != null && factorValue.IsFulfilled)
        {
            resolvedFactor = factorValue.ResolvedValue;
            if (resolvedFactor == default)
            {
                return;
            }
        }

        // decimals
        decimal resolvedRoundStep = 1;
        var roundStepValue = NewActionValue<decimal>(context, roundStep ?? 1);
        if (roundStepValue != null && roundStepValue.IsFulfilled)
        {
            resolvedRoundStep = roundStepValue.ResolvedValue;
            if (resolvedRoundStep == default)
            {
                return;
            }
        }

        // update factor value
        var days = (decimal)end.Value.Subtract(start.Value).TotalDays;
        if (days == 0)
        {
            return;
        }
        var value = (days * resolvedFactor).RoundDown(resolvedRoundStep);
        context.Function.SetValue(context.CaseFieldName, value);
    }

    #endregion

}
