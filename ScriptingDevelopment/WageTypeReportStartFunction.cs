using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Runtime;

namespace PayrollEngine.Client.Tutorial.ScriptingDevelopment;

[ReportStartFunction(
    tenantIdentifier: "SimplePayroll",
    userIdentifier: "peter.schmid@foo.com",
    regulationName: "SimplePayroll")]
public class WageTypeReportStartFunction : Scripting.Function.ReportStartFunction
{
    public WageTypeReportStartFunction(IReportStartRuntime runtime) :
        base(runtime)
    {
    }

    [ReportStartScript(
        reportName: "WageTypesReport",
        culture: "de-CH")]
    public object Execute()
    {
        return null;
    }
}
