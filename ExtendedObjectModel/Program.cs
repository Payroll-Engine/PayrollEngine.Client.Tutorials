using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using PayrollEngine.Client.Model;
using PayrollEngine.Client.Service.Api;
using Tasks = System.Threading.Tasks;

namespace PayrollEngine.Client.Tutorial.ExtendedObjectModel;

/// <summary>Extended object model tutorial program</summary>
internal class Program : ConsoleProgram<Program>
{
    /// <inheritdoc />
    protected override async Tasks.Task RunAsync()
    {
        // tenant
        var tenant = await GetTenantAsync(ConsoleArguments.Get(1));
        if (tenant == null)
        {
            return;
        }

        // employees
        var employees = await GetEmployeesAsync(tenant.Id);
        if (employees.Any())
        {
            DisplayEmployees(tenant, employees);
            WriteLine();
        }
        else
        {
            WriteInfoLine($"Missing employees for tenant {tenant.Identifier}");
        }

        // activities
        var activities = await GetActivitiesAsync(tenant.Id);
        if (activities.Any())
        {
            DisplayActivities(tenant, activities);
            WriteLine();
        }
        else
        {
            WriteInfoLine($"Missing activities for tenant {tenant.Identifier}");
        }

        // employee details
        var employeeErpId = ConsoleArguments.Get(2);
        if (!string.IsNullOrWhiteSpace(employeeErpId))
        {
            var employee = await GetEmployeeAsync(tenant.Id, employeeErpId);
            if (employee != null)
            {
                DisplayEmployee(tenant, employee);
            }
            else
            {
                WriteInfoLine($"Unknown employee with Erp id {employeeErpId}");
            }
        }

        PressAnyKey();
    }

    #region Domain

    /// <summary>Get employee by Erp id</summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeErpId">The employee Erp id</param>
    private async Tasks.Task<MyEmployee> GetEmployeeAsync(int tenantId, string employeeErpId) =>
        (await new EmployeeService(HttpClient).QueryAsync<MyEmployee>(new(tenantId), new()
        {
            // Erp id
            Filter = $"TA_ErpId eq '{employeeErpId}'",
            // active only
            Status = ObjectStatus.Active
        })).FirstOrDefault();

    /// <summary>Get active employees</summary>
    /// <param name="tenantId">The tenant id</param>
    private async Tasks.Task<List<MyEmployee>> GetEmployeesAsync(int tenantId) =>
        await new EmployeeService(HttpClient).QueryAsync<MyEmployee>(new(tenantId), new()
        {
            // active only
            Status = ObjectStatus.Active
        });

    /// <summary>Get active tasks</summary>
    /// <param name="tenantId">The tenant id</param>
    private async Tasks.Task<List<Activity>> GetActivitiesAsync(int tenantId) =>
        (await new TaskService(HttpClient).QueryAsync<Task>(new(tenantId), new()
        {
            // active only
            Status = ObjectStatus.Active
        })).Select(MapTaskToActivity).ToList();

    /// <summary>Map a payroll task to a custom activity</summary>
    /// <param name="task">The payroll task</param>
    /// <returns>The custom activity</returns>
    private static Activity MapTaskToActivity(Task task)
    {
        // mapper configuration
        var config = new MapperConfiguration(cfg =>
            cfg.CreateMap<Task, Activity>()
                .ForMember(dest => dest.ActivityId, act => act.MapFrom(
                            src => src.GetAttributeGuid("ErpId")))
                .ForMember(dest => dest.State, act => act.MapFrom(
                            src => src.Completed.HasValue ? ActivityStateCode.Completed : ActivityStateCode.Scheduled)));

        // map task to activity
        var mapper = new Mapper(config);
        var activity = mapper.Map<Activity>(task);
        return activity;
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

    /// <summary>Display the activities</summary>
    /// <param name="tenant">The tenant</param>
    /// <param name="activities">The activities</param>
    private static void DisplayActivities(Tenant tenant, List<Activity> activities)
    {
        // title
        WriteTitleLine($"{tenant.Identifier}: Activities");

        // table
        var line = new string('-', 25 + 20 + 40);
        WriteLine(line);
        WriteLine($"{"Name",-25}{"State",-20}{"Activity Id",-40}");
        WriteLine(line);

        foreach (var activity in activities)
        {
            WriteLine($"{activity.Name,-25}{activity.State,-20}{activity.ActivityId}");
        }
        WriteLine(line);
    }

    /// <summary>Display the employees</summary>
    /// <param name="tenant">The tenant</param>
    /// <param name="employee">The employee</param>
    private static void DisplayEmployee(Tenant tenant, MyEmployee employee)
    {
        // title
        WriteTitleLine($"{tenant.Identifier}: employee {employee.Identifier}");

        // table
        var line = new string('-', 25 + 40);
        WriteLine(line);
        WriteLine($"{"Attribute",-25}{"Value",-40}");
        WriteLine(line);
        WriteLine("Identifier".PadRight(25) + employee.Identifier);
        WriteLine("FirstName".PadRight(25) + employee.FirstName);
        WriteLine("LastName".PadRight(25) + employee.LastName);
        WriteLine("Created".PadRight(25) + employee.Created);
        WriteLine("Updated".PadRight(25) + employee.Updated);
        WriteLine("Status".PadRight(25) + employee.Status);
        WriteLine("Id".PadRight(25) + employee.Id);
        WriteLine("ErpId".PadRight(25) + employee.ErpId);
        WriteLine(line);
    }

    /// <summary>Display the employees</summary>
    /// <param name="tenant">The tenant</param>
    /// <param name="employees">The employees</param>
    private static void DisplayEmployees(Tenant tenant, List<MyEmployee> employees)
    {
        // title
        WriteTitleLine($"{tenant.Identifier}: Employees");

        // table
        var line = new string('-', 25 + 20 + 20 + 15 + 40);
        WriteLine(line);
        WriteLine($"{"Identifier",-25}{"First name",-20}{"Last name",-20}{"Payroll Id",-15}{"Erp Id",-40}");
        WriteLine(line);

        foreach (var employee in employees)
        {
            WriteLine($"{employee.Identifier,-25}{employee.FirstName,-20}{employee.LastName,-20}{employee.Id,-15}{employee.ErpId,-40}");
        }
        WriteLine(line);
    }

    /// <inheritdoc />
    protected override Tasks.Task HelpAsync()
    {
        WriteLine("Usage: ExtendedObjectModel Tenant [EmployeeErpId]");
        WriteLine();
        WriteLine("Arguments:");
        WriteLine("  1. Tenant identifier");
        WriteLine("  2. Employee Erp Guid (optional)");
        WriteLine();
        WriteLine("Examples:");
        WriteLine("  ExtendedObjectModel MyTenant");
        WriteLine("  ExtendedObjectModel MyTenant f37d1b9b-944f-44fa-bf4c-61f4ee502205");
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
