using System;

// ReSharper disable once CheckNamespace
namespace PayrollEngine.Client.Scripting.Function;

public partial class CaseChangeFunction
{
    // Issue (from PayrollEngine.Client.Scripting.Function.CaseValidateFunction.cs)
    public bool HasIssues() => throw new NotSupportedException();
    public void AddCaseIssue(string message) => throw new NotSupportedException();
    public void AddCaseFieldIssue(string caseFieldName, string message) => throw new NotSupportedException();
    public void AddCaseAttributeIssue(string attributeName, params object[] parameters) => throw new NotSupportedException();
    public void AddFieldAttributeIssue(string caseFieldName, string attributeName, params object[] parameters) =>
        throw new NotSupportedException();

    public DateTime? GetStart(string caseFieldName) => throw new NotSupportedException();
    public DateTime? GetEnd(string caseFieldName) => throw new NotSupportedException();
    public void SetValue(string caseFieldName, object value) => throw new NotSupportedException();
}
