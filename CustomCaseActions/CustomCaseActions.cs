using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Function;

namespace PayrollEngine.Client.Tutorial.Scripting.CustomCaseAction;

[ActionProvider("CustomActions", typeof(CaseChangeFunction))]
public class MyCaseBuildActions : CaseChangeActionsBase
{

    #region Validate

    [ActionIssue("MissingUId", "Missing value (0)", 1)]
    [ActionIssue("InvalidUId", "(0) with invalid UID (1)", 2)]
    [CaseValidateAction("CheckUId", "Validate for the Swiss UID")]
    public void CheckUId(CaseChangeActionContext context)
    {
        var sourceValue = GetActionValue<string>(context);
        if (sourceValue?.ResolvedValue == null)
        {
            AddIssue(context, "MissingUId", context.CaseFieldName);
            return;
        }

        try
        {
            // ISO 7064 digit check with modulus, radix, character-set and double-check-digit option
            new CheckDigit(11, 1, "0123456789", false).Check(sourceValue.ResolvedValue);

            // predefined digit checks: Mod11Radix2, Mod37Radix2, Mod97Radix10, Mod661Radix26, Mod1271Radix36
            // CheckDigit.Mod11Radix2.Check(sourceValue.ResolvedValue);
        }
        catch (CheckDigitException exception)
        {
            AddIssue(context, "InvalidUId", context.CaseFieldName, exception.CheckValue);
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
        var factorValue = GetActionValue<decimal>(context, factor);
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
        var roundStepValue = GetActionValue<decimal>(context, roundStep ?? 1);
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
