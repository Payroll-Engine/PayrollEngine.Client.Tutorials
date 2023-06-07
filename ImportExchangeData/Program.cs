using System;
using System.Collections.Generic;
using PayrollEngine.Client.Exchange;
using PayrollEngine.Client.Model;
using PayrollEngine.Client.Scripting.Script;
using PayrollEngine.Client.Service.Api;
using Tasks = System.Threading.Tasks;

namespace PayrollEngine.Client.Tutorial.ImportExchangeData;

/// <summary>Import exchange data tutorial program</summary>
internal class Program : ConsoleProgram<Program>
{
    private const string MonthWageCaseFieldName = "Monatslohn";
    private const string MonthWageMinRequestAttribute = "MonthWageMinRequest";
    private const decimal MonthWageMin = 4000;

    /// <summary>Mandatory arguments: tenant and absences json file</summary>
    protected override int MandatoryArgumentCount => 2;

    /// <inheritdoc />
    protected override async Tasks.Task RunAsync()
    {
        // tenant
        var tenant = await GetTenantAsync(ConsoleArguments.Get(1));
        if (tenant == null)
        {
            return;
        }

        // exchange
        var exchange = await ExchangeReader.ReadAsync(ConsoleArguments.Get(2));
        if (exchange == null)
        {
            return;
        }

        // case values
        var caseValues = await SetupCaseValuesAsync(tenant, exchange);

        // import case changes
        await ImportAsync(exchange);

        // user notification
        DisplayCaseChanges(tenant, caseValues);
        PressAnyKey();
    }

    #region Domain

    /// <summary>Import exchange to backend</summary>
    /// <param name="exchange">The exchange model</param>
    private async Tasks.Task ImportAsync(Model.Exchange exchange) =>
        await new ExchangeImport(HttpClient, exchange, new ScriptParser()).ImportAsync();

    /// <summary>Setup case values</summary>
    /// <param name="tenant">The tenant</param>
    /// <param name="exchange">The exchange</param>
    /// <returns>List with case change setup and case value setup</returns>
    private static async Tasks.Task<List<Tuple<ICaseChangeSetup, ICaseValueSetup>>> SetupCaseValuesAsync(Tenant tenant, Model.Exchange exchange)
    {
        var caseValueSetups = new List<Tuple<ICaseChangeSetup, ICaseValueSetup>>();

        // visitor
        await new Visitor(exchange)
        {
            VisitCaseValue = (exchangeTenant, _, caseChangeSetup, _, caseValueSetup) =>
            {
                if (!string.Equals(exchangeTenant.Identifier, tenant.Identifier))
                {
                    return;
                }

                // min month wage
                if (string.Equals(MonthWageCaseFieldName, caseValueSetup.CaseFieldName))
                {
                    // month wage from case value
                    var wage = ValueConvert.ToDecimal(caseValueSetup.Value);
                    if (wage < MonthWageMin)
                    {
                        // store min wage request value as case value attribute
                        caseValueSetup.SetAttribute(MonthWageMinRequestAttribute, wage);
                        // update case value
                        caseValueSetup.Value = ValueConvert.ToJson(MonthWageMin);
                    }
                }

                // collect case value setup
                caseValueSetups.Add(new(caseChangeSetup, caseValueSetup));
            }
        }.ExecuteAsync();

        return caseValueSetups;
    }

    /// <summary>Get tenant by console argument</summary>
    /// <param name="tenantIdentifier">The tenant identifier</param>
    private async Tasks.Task<Tenant> GetTenantAsync(string tenantIdentifier)
    {
        // tenant argument
        if (string.IsNullOrWhiteSpace(tenantIdentifier))
        {
            WriteErrorLine("Missing argument tenant identifier.");
            PressAnyKey();
            return null;
        }

        // tenant request
        var tenant = await new TenantService(HttpClient).GetAsync<Tenant>(new(), tenantIdentifier);
        if (tenant == null)
        {
            WriteErrorLine($"Invalid tenant identifier {tenantIdentifier}.");
            PressAnyKey();
            return null;
        }
        return tenant;
    }

    #endregion

    #region Output

    /// <summary>Display the case changes</summary>
    /// <param name="tenant">The tenant</param>
    /// <param name="caseValueSetups">The case value setups</param>
    private static void DisplayCaseChanges(Tenant tenant, List<Tuple<ICaseChangeSetup, ICaseValueSetup>> caseValueSetups)
    {
        // title
        WriteTitleLine($"Case changes {tenant.Identifier}");

        // table
        var line = new string('-', 25 + 20 + 20 + 15 + 15 + 20 + 30);
        WriteLine();
        WriteLine(line);
        WriteLine($"{"Employee",-25}{"Case",-20}{"Case field",-20}{"Start",-15}{"End",-15}{"Value",-20}Notes");
        WriteLine(line);

        foreach (var setup in caseValueSetups)
        {
            var caseSetup = setup.Item1;
            var valueSetup = setup.Item2;

            // compact start/end date
            var start = valueSetup.Start.HasValue ?
                valueSetup.Start.Value.ToCompactString() : string.Empty;
            var end = valueSetup.End.HasValue ?
                valueSetup.End.Value.ToCompactString() : string.Empty;
            // case value display
            Write($"{caseSetup.EmployeeIdentifier,-25}{caseSetup.Case.CaseName,-20}{valueSetup.CaseFieldName,-20}{start,-15}{end,-15}{valueSetup.Value,-20}");

            // wage
            if (valueSetup.ContainsAttribute(MonthWageMinRequestAttribute))
            {
                WriteError($"min. month wage > {valueSetup.GetAttribute(MonthWageMinRequestAttribute)}");
            }
            WriteLine();
        }
        WriteLine(line);
    }

    /// <inheritdoc />
    protected override Tasks.Task HelpAsync()
    {
        WriteLine("Usage: ImportExchangeData Tenant CaseValues.json");
        WriteLine();
        WriteLine("Arguments:");
        WriteLine("  1. Tenant identifier");
        WriteLine("  2. JSON file containing the case values");
        WriteLine();
        WriteLine("Examples:");
        WriteLine("  ImportExchangeData MyTenant");
        WriteLine("  ImportExchangeData MyTenant MyCaseValues.json");
        return Tasks.Task.CompletedTask;
    }

    #endregion

    /// <summary>Program entry point</summary>
    static async Tasks.Task Main()
    {
        // init logger
        Log.SetLogger(new Serilog.PayrollLog());

        // execute program
        using var program = new Program();
        await program.ExecuteAsync();
    }
}
