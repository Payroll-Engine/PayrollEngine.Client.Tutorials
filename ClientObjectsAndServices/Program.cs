using PayrollEngine.Client.Model;
using PayrollEngine.Client.Service.Api;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Client.Tutorial.ClientObjectsAndServices;

/// <summary>The client objects and services tutorial program</summary>
internal class Program : ConsoleProgram<Program>
{
    /// <inheritdoc />
    protected override async Task RunAsync()
    {
        // tenant argument
        var tenantIdentifier = ConsoleArguments.Get(1);
        if (string.IsNullOrWhiteSpace(tenantIdentifier))
        {
            WriteErrorLine("Missing argument tenant identifier.");
            PressAnyKey();
            return;
        }

        // tenant request
        var tenantService = new TenantService(HttpClient);
        var tenant = await tenantService.GetAsync<Tenant>(new(), tenantIdentifier);
        if (tenant == null)
        {
            WriteErrorLine($"Invalid tenant identifier {tenantIdentifier}.");
            PressAnyKey();
            return;
        }

        // employee query
        DivisionQuery query = new()
        {
            // list order
            OrderBy = $"{nameof(Employee.FirstName)} DESC",
            // name filter example (OData)
            //Filter = $"Contains({nameof(Employee.LastName)}, 'a')",
            // top argument
            Top = ConsoleArguments.GetInt(2)
        };

        // employees request
        var employeeService = new EmployeeService(HttpClient);
        var employees = await employeeService.QueryAsync<Employee>(new(tenant.Id), query);

        // employee list
        WriteTitleLine($"{tenantIdentifier} employees");
        foreach (var employee in employees)
        {
            WriteLine($"{employee.FirstName} {employee.LastName} - {employee.Identifier} [#{employee.Id}]");
        }
        WriteLine();
        WriteSuccessLine($"Total {employees.Count} employees");
        WriteLine();

        PressAnyKey();
    }

    /// <summary>Program entry point</summary>
    static async Task Main()
    {
        // init logger
        Log.SetLogger(new Serilog.PayrollLog());

        // execute program
        using var program = new Program();
        await program.ExecuteAsync();
    }
}
