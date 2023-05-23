using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Client.Model;
using PayrollEngine.Client.QueryExpression;
using PayrollEngine.Client.Service.Api;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Client.Tutorial.BuildDataQuery;

/// <summary>The build data query tutorial program</summary>
internal class Program : ConsoleProgram<Program>
{
    /// <inheritdoc />
    protected override async Task RunAsync()
    {
        var tenant = await GetTenantAsync();
        if (tenant == null)
        {
            return;
        }

        // all employees query
        await QueryEmployeesAsync(tenant.Id, "All query", new());

        // top 2 employees query
        await QueryEmployeesAsync(tenant.Id, "Top 2 query", BuildTopQuery(2));

        // employees erp filter query
        var erpId = ConsoleArguments.Get(3);
        if (!string.IsNullOrWhiteSpace(erpId))
        {
            await QueryEmployeesAsync(tenant.Id, $"Erp id query - {erpId}",
                    BuildErpFilterQuery(erpId));
        }

        // employees name query
        var nameFilter = ConsoleArguments.Get(2);
        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            await QueryEmployeesAsync(tenant.Id, $"Name query - {nameFilter}",
                NameQuery(nameFilter));
        }

        PressAnyKey();
    }

    private static DivisionQuery BuildTopQuery(int? top = null) =>
        new()
        {
            // order by first name descending
            OrderBy = new OrderByDescending(nameof(Employee.FirstName)),
            Top = top
        };

    private static DivisionQuery BuildErpFilterQuery(string erpId) =>
        new()
        {
            // compare attribute value
            Filter = string.IsNullOrWhiteSpace(erpId) ? null :
                new Equals("ErpId".ToTextAttributeField(), erpId)
        };

    private static DivisionQuery NameQuery(string nameFilter) =>
        new()
        {
            // active employees only
            Filter = new ActiveStatus().And(
                        // employees created in 2019
                        new Equals(new Year(nameof(Employee.Created)), 2019).And(
                            // employees containing the search criteria in the first or last name
                            new Contains(nameof(Employee.FirstName), nameFilter).Or(
                            new Contains(nameof(Employee.LastName), nameFilter))))
        };

    private async Task QueryEmployeesAsync(int tenantId, string title, DivisionQuery query)
    {
        WriteTitleLine(title);

        // receive employees with query
        var employeeService = new EmployeeService(HttpClient);
        var employees = await employeeService.QueryAsync<Employee>(new(tenantId), query);
        DisplayEmployees(employees);
    }

    private async Task<Tenant> GetTenantAsync()
    {
        // tenant argument
        var tenantIdentifier = ConsoleArguments.Get(1);
        if (string.IsNullOrWhiteSpace(tenantIdentifier))
        {
            WriteErrorLine("Missing tenant identifier.");
            PressAnyKey();
            return null;
        }

        // tenant request
        var tenantService = new TenantService(HttpClient);
        var tenant = await tenantService.GetAsync<Tenant>(new(), tenantIdentifier);
        if (tenant == null)
        {
            WriteErrorLine($"Invalid tenant identifier {tenantIdentifier}.");
            PressAnyKey();
            return null;
        }

        return tenant;
    }

    #region Output

    private static void DisplayEmployees(List<Employee> employees)
    {
        if (employees.Any())
        {
            // table
            var line = new string('-', 15 + 15 + 15 + 25 + 10);
            WriteLine(line);
            WriteLine($"{"Created",-15}{"First name",-15}{"Last name",-15}{"Identifier",-25}{"Id",-10}");
            WriteLine(line);
            foreach (var employee in employees)
            {
                WriteLine($"{employee.Created.ToCompactString(),-15}" +
                          $"{employee.FirstName,-15}{employee.LastName,-15}" +
                          $"{employee.Identifier,-25}{employee.Id,-10}");
            }
            WriteLine(line);
            WriteInfoLine($"Total {employees.Count} employees");
        }
        else
        {
            WriteInfoLine("No employees found");
        }
        WriteLine();
    }

    #endregion

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
