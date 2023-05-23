﻿using System.Data;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Runtime;

namespace PayrollEngine.Client.Tutorial.ScriptingDevelopment;

[ReportEndFunction(
    tenantIdentifier: "SimplePayroll",
    userIdentifier: "peter.schmid@foo.com",
    regulationName: "SimplePayroll")]
public class WageTypeReportEndFunction : Scripting.Function.ReportEndFunction
{
    public WageTypeReportEndFunction(IReportEndRuntime runtime) :
        base(runtime)
    {
    }

    [ReportEndScript(
        reportName: "WageTypesReport",
        language: Scripting.Language.German)]
    public object ReportEndScript()
    {
        // wage types
        var regulations = Tables["Regulations"];
        if (regulations == null || regulations.Rows.Count != 1)
        {
            throw new ScriptException("Missing regulation");
        }

        DataRow regulation = regulations.Rows[0];
        if (regulation["Id"] is not int id || id <= 0)
        {
            throw new ScriptException("Missing regulation");
        }

        // query regulation wage types
        // ...

        return default;
    }
}

