using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SystemData = System.Data;
using PayrollEngine.Client.Model;
using PayrollEngine.Client.QueryExpression;
using PayrollEngine.Client.Service.Api;
using PayrollEngine.Data;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Client.Tutorial.TransformReportData;

/// <summary>The build data query tutorial program</summary>
internal class Program : ConsoleProgram<Program>
{
    // ReSharper disable StringLiteralTypo
    const string monthlyWageCaseFieldName = "Monatslohn";
    // ReSharper restore StringLiteralTypo
    const string MonthlyWageColumnName = "MonthlyWage";

    /// <inheritdoc />
    protected override async Task RunAsync()
    {
        // tenant
        var tenant = await GetTenantAsync(ConsoleArguments.Get(1));
        if (tenant == null)
        {
            return;
        }

        // payroll
        var payroll = await GetPayrollAsync(tenant.Id, ConsoleArguments.Get(2));
        if (payroll == null)
        {
            return;
        }

        // employees
        var employees = await QueryEmployeesAsync(tenant.Id);

        // employee erp id
        ApplyErpId(employees);
        DisplayEmployeeErpId(employees);

        // employee wage
        var employeesWages = await QueryWagesAsync(tenant.Id, payroll.Id,
            employees.AsEnumerable().Select(x => x.GetValue<int>("Id")), DateTime.Now);
        ApplyWage(employees, employeesWages);
        DisplayEmployeeWage(employees);

        PressAnyKey();
    }

    private static void ApplyWage(SystemData.DataTable employees, Dictionary<int, SystemData.DataTable> employeesWages)
    {
        foreach (SystemData.DataRow employee in employees.Rows)
        {
            // employee wages
            var employeeId = employee.GetValue<int>("Id");
            if (employeeId == 0 || !employeesWages.TryGetValue(employeeId, out var wages))
            {
                continue;
            }

            // transpose multiple case values
            employee.TransposeFrom(wages.AsEnumerable(),
                // case field name with mapping
                columnName: wage => GetWageTypeColumnName(wage.GetValue<string>("CaseFieldName")),
                // case value type
                columnType: wage => wage.GetPayrollValueType().GetSystemType(),
                // case value
                itemValue: wage => wage.GetPayrollValue());

            // alternative: set single case value
            //employee.SetValue(MonthlyWageColumnName, wages.SingleRow().GetPayrollValue());
        }
        employees.AcceptChanges();
    }

    private static string GetWageTypeColumnName(string caseFieldName)
    {
        // name mapping
        return caseFieldName switch
        {
            monthlyWageCaseFieldName => MonthlyWageColumnName,
            _ => caseFieldName
        };
    }

    private async Task<Dictionary<int, SystemData.DataTable>> QueryWagesAsync(int tenantId, int payrollId,
        IEnumerable<int> employeeIds, DateTime valueDate)
    {
        // receive employees with query
        var service = new TenantService(HttpClient);

        var employeesWages = new Dictionary<int, SystemData.DataTable>();
        foreach (var employeeId in employeeIds)
        {
            // query end point: /api/tenants/{tenantId}/payrolls/{payrollId}/cases/values/time
            var wages = (await service.ExecuteReportQueryAsync(tenantId, "GetPayrollTimeCaseValues", Language.German,
                new QueryParameters()
                    .Parameter(nameof(tenantId), tenantId)
                    .Parameter(nameof(payrollId), payrollId)
                    .Parameter(nameof(employeeId), employeeId)
                    .Parameter("CaseType", "Employee")
                    .Parameter("CaseFieldNames", new[] { monthlyWageCaseFieldName })
                    .Parameter(nameof(valueDate), valueDate)))
                .ToSystemDataTable();

            // ignore empty wages
            if (wages.IsSingleRow())
            {
                employeesWages.Add(employeeId, wages);
            }
        }

        return employeesWages;
    }

    private static void ApplyErpId(SystemData.DataTable employees)
    {
        foreach (SystemData.DataRow employee in employees.Rows)
        {
            // employee attributes
            var attributes = employee.GetAttributes();

            // transpose erp id
            employee.TransposeFrom(attributes);
        }
    }

    private async Task<SystemData.DataTable> QueryEmployeesAsync(int tenantId)
    {
        // receive employees with query
        var service = new TenantService(HttpClient);
        // query end point: /api/tenants/{tenantId}/employees
        var employees = await service.ExecuteReportQueryAsync(tenantId, "QueryEmployees", Language.German,
            new QueryParameters()
                .ActiveStatus());
        // convert payroll data table to system data table
        return employees.ToSystemDataTable();
    }

    private async Task<Payroll> GetPayrollAsync(int tenantId, string payrollName)
    {
        if (string.IsNullOrWhiteSpace(payrollName))
        {
            WriteErrorLine("Missing payroll name.");
            PressAnyKey();
            return null;
        }

        // payroll request
        var service = new PayrollService(HttpClient);
        var payroll = await service.GetAsync<Payroll>(new(tenantId), payrollName);
        if (payroll == null)
        {
            WriteErrorLine($"Unknown payroll {payrollName}.");
            PressAnyKey();
        }
        return payroll;
    }

    private async Task<Tenant> GetTenantAsync(string tenantIdentifier)
    {
        if (string.IsNullOrWhiteSpace(tenantIdentifier))
        {
            WriteErrorLine("Missing tenant identifier.");
            PressAnyKey();
            return null;
        }

        // tenant request
        var service = new TenantService(HttpClient);
        var tenant = await service.GetAsync<Tenant>(new(), tenantIdentifier);
        if (tenant == null)
        {
            WriteErrorLine($"Unknown tenant {tenantIdentifier}.");
            PressAnyKey();
        }
        return tenant;
    }

    #region Output

    private static void DisplayEmployeeErpId(SystemData.DataTable employees)
    {
        WriteTitleLine("Employee Erp Id");
        WriteLine();

        if (employees.HasRows())
        {
            // table
            var line = new string('-', 15 + 15 + 15 + 25 + 40);
            WriteLine(line);
            WriteLine($"{"Created",-15}{"First name",-15}{"Last name",-15}{"Identifier",-25}{"Erp Id",-40}");
            WriteLine(line);
            foreach (SystemData.DataRow employee in employees.Rows)
            {
                WriteLine($"{employee.GetValue<DateTime>(nameof(Employee.Created)).ToCompactString(),-15}" +
                          $"{employee[nameof(Employee.FirstName)],-15}" +
                          $"{employee[nameof(Employee.LastName)],-15}" +
                          $"{employee[nameof(Employee.Identifier)],-25}" +
                          $"{employee["ErpId"],-40}");
            }
            WriteLine(line);
            WriteInfoLine($"Total {employees.Rows.Count} employees");
        }
        else
        {
            WriteInfoLine("No employees found");
        }
        WriteLine();
    }

    private static void DisplayEmployeeWage(SystemData.DataTable allEmployees)
    {
        WriteTitleLine("Employee wages");
        WriteLine();

        // filter out employees without monthly wage
        var employees = allEmployees.AsEnumerable().Where(
            x => x.GetValue<decimal>(MonthlyWageColumnName) != 0).ToList();
        if (employees.Any())
        {
            // table
            var line = new string('-', 15 + 15 + 15 + 25 + 20);
            WriteLine(line);
            WriteLine($"{"Created",-15}{"First name",-15}{"Last name",-15}{"Identifier",-25}{MonthlyWageColumnName,-20}");
            WriteLine(line);
            foreach (var employee in employees)
            {
                WriteLine($"{employee.GetValue<DateTime>(nameof(Employee.Created)).ToCompactString(),-15}" +
                          $"{employee[nameof(Employee.FirstName)],-15}" +
                          $"{employee[nameof(Employee.LastName)],-15}" +
                          $"{employee[nameof(Employee.Identifier)],-25}" +
                          $"{employee[MonthlyWageColumnName],-20}");
            }
            WriteLine(line);
            WriteInfoLine($"Total {employees.Count} employees");
        }
        else
        {
            WriteInfoLine("No employee with monthly wage found");
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
