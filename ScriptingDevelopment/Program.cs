using System.Data;
using PayrollEngine.Client.Scripting.Function.Api;
using Tasks = System.Threading.Tasks;

namespace PayrollEngine.Client.Tutorial.ScriptingDevelopment;

/// <summary>Scripting development tutorial program</summary>
internal class Program : ConsoleProgram<Program>
{
    /// <inheritdoc />
    protected override int MandatoryArgumentCount => 2;

    // arguments
    private ReportScriptMode ScriptMode { get; } = ConsoleArguments.GetEnum<ReportScriptMode>(1);
    private string ReportName { get; } = ConsoleArguments.Get(2);
    private string QueryFileName  { get; }= ConsoleArguments.Get(3);
    private string ResultFileName { get; } = ConsoleArguments.Get(4);

    /// <summary>The scripting configuration</summary>
    private ScriptingConfiguration ScriptingConfiguration =>
        Configuration.GetConfiguration<ScriptingConfiguration>();

    /// <inheritdoc />
    protected override Tasks.Task RunAsync()
    {
        switch (ScriptMode)
        {
            case ReportScriptMode.Start:
                Start();
                break;
            case ReportScriptMode.End:
                End();
                break;
            case ReportScriptMode.StartEnd:
                StartEnd();
                break;
        }
        return Tasks.Task.CompletedTask;
    }

    private void Start()
    {
        // invoke start
        new ReportStartFunctionInvoker<WageTypeReportStartFunction>(
            HttpClient,
            ScriptingConfiguration,
            QueryFileName).Start(ReportName);
        WriteSuccessLine($"Report {ReportName} start function executed.");
    }

    private void End()
    {
        var invoker = new ReportEndFunctionInvoker<WageTypeReportEndFunction>(
            HttpClient,
            ScriptingConfiguration,
            QueryFileName,
            ResultFileName);
        var reportRequest = invoker.BuildReportRequest(ReportName);

        // manual query
        DataSet dataSet = null;
        if (string.IsNullOrWhiteSpace(QueryFileName))
        {
            dataSet = new QueryInvoker(HttpClient).InvokeQueriesAsync(
                ReportName, reportRequest).Result;
        }
        // invoke end
        invoker.End(ReportName, reportRequest, dataSet);
        WriteSuccessLine($"Report {ReportName} end function executed.");
    }

    private void StartEnd()
    {
        // invoke start
        new ReportStartFunctionInvoker<WageTypeReportStartFunction>(
            HttpClient,
            ScriptingConfiguration,
            QueryFileName).Start(ReportName);

        // invoke end
        new ReportEndFunctionInvoker<WageTypeReportEndFunction>(
            HttpClient,
            ScriptingConfiguration,
            QueryFileName,
            ResultFileName).End(ReportName);
        WriteSuccessLine($"Report {ReportName} start and end function executed.");
    }

    /// <inheritdoc />
    protected override Tasks.Task HelpAsync()
    {
        WriteLine("Usage: ScriptingDevelopment ScriptMode ReportName [QueryFileName] [ResultFileName]");
        WriteLine();
        WriteTitleLine("Arguments");
        WriteLine("  1. ScriptMode: the script mode Start, End or StartEnd");
        WriteLine("  2. ReportName: the report name");
        WriteLine("  3. QueryFileName: query file name (optional)" +
                  "       report start: write query file" +
                  "       report end: read query file");
        WriteLine("  4. ResultFileName: result file name (optional)" +
                  "       report end: write result file");
        WriteLine();
        WriteTitleLine("Examples");
        WriteLine("  ScriptingDevelopment Start MyReportName");
        WriteLine("  ScriptingDevelopment Start MyReportName MyQueries.json");
        WriteLine("  ScriptingDevelopment End MyReportName");
        WriteLine("  ScriptingDevelopment End MyReportName MyQueries.json");
        WriteLine("  ScriptingDevelopment End MyReportName MyQueries.json MyResults.json");
        WriteLine("  ScriptingDevelopment StartEnd MyReportName MyQueries.json");
        WriteLine("  ScriptingDevelopment StartEnd MyReportName MyQueries.json MyResults.json");
        return Tasks.Task.CompletedTask;
    }

    /// <summary>Program entry point</summary>
    private static async Tasks.Task Main()
    {
        // init logger
        Log.SetLogger(new Serilog.PayrollLog());

        // execute program
        using var program = new Program();
        await program.ExecuteAsync();
    }
}
