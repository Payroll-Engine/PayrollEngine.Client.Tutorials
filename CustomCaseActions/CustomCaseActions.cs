
// ReSharper disable once CheckNamespace
namespace PayrollEngine.Client.Scripting.Function;

public partial class CaseChangeFunction
{

    #region Validate

    [ActionIssue("MissingUId", "Missing value (0)", 1)]
    [ActionIssue("InvalidUId", "(0) with invalid UID: (1)", 2)]
    [ActionParameter("caseFieldName", "The case field name")]
    [ActionParameter("uid", "The UID text")]
    [CaseValidateAction("CheckUId", "Validate for the Swiss company id (UID)")]
    public void CheckUId(string caseFieldName, string uid)
    {
        if (string.IsNullOrWhiteSpace(uid))
        {
            AddCaseAttributeIssue("MissingUId", caseFieldName);
            return;
        }

        // extract check value
        var checkValue = uid.RemoveFromStart("CHE-").Replace(".", "");

        try
        {
            // ISO 7064 digit check with modulus, radix, character-set and double-check-digit option
            new CheckDigit(11, 1, "0123456789", false).Check(checkValue);

            // predefined digit checks: Mod11Radix2, Mod37Radix2, Mod97Radix10, Mod661Radix26, Mod1271Radix36
            // CheckDigit.Mod11Radix2.Check(checkValue);
        }
        catch (CheckDigitException)
        {
            AddCaseAttributeIssue("InvalidUId", caseFieldName, uid);
        }
    }

    #endregion

    #region Build

    [ActionParameter("factor", "Day factor (def=1)")]
    [ActionParameter("caseFieldName", "The field name")]
    [ActionParameter("roundStep", "The rounding step size (def=1)")]
    [CaseBuildAction("DayFactor", "Value by day factor (def=1)")]
    public void DayFactor(string caseFieldName, decimal factor = 1, decimal roundStep = 1)
    {
        // start and end date
        var start = GetStart(caseFieldName);
        var end = GetEnd(caseFieldName);
        if (!start.HasValue || !end.HasValue)
        {
            return;
        }

        // update factor value
        var days = (decimal)end.Value.Subtract(start.Value).TotalDays;
        if (days == 0)
        {
            return;
        }
        var value = (days * factor).RoundDown(roundStep);
        SetValue(caseFieldName, value);
    }

    #endregion

}
