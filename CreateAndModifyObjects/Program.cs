using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PayrollEngine.Client.Model;
using PayrollEngine.Client.Service;
using PayrollEngine.Client.Service.Api;
using PayrollEngine.Serialization;
using Tasks = System.Threading.Tasks;

namespace PayrollEngine.Client.Tutorial.CreateAndModifyObjects;

/// <summary>The create and modify objects tutorial program</summary>
internal class Program : ConsoleProgram<Program>
{
    /// <summary>Mandatory argument: tenant</summary>
    protected override int MandatoryArgumentCount => 1;

    /// <inheritdoc />
    protected override async Tasks.Task RunAsync()
    {
        // tenant
        var tenant = await GetTenantAsync(ConsoleArguments.Get(1));
        if (tenant == null)
        {
            return;
        }

        // users
        var users = await GetUsersAsync(tenant.Id);
        if (!users.Any())
        {
            WriteInfoLine($"Missing users for tenant {tenant.Identifier}");
            return;
        }

        // open tasks
        var openTasks = await GetOpenTasksAsync(tenant.Id);

        // updated tasks from CSV
        var updateTasks = GetTasksFromCsv(users, ConsoleArguments.Get(2));

        // merge open tasks with update tasks
        var tasks = await UpdateTasksAsync(tenant.Id, openTasks, updateTasks);

        // user notification
        DisplayTasks(tenant, tasks, users);
        PressAnyKey();
    }

    #region Domain

    /// <summary>Update tasks by merging open tasks with update tasks</summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="openTasks">The open tasks</param>
    /// <param name="updateTasks">The update tasks</param>
    /// <returns>The updated tasks, including the open tasks</returns>
    private async Tasks.Task<List<Task>> UpdateTasksAsync(int tenantId, List<Task> openTasks, List<Task> updateTasks)
    {
        var tasks = new List<Task>();
        var taskService = new TaskService(HttpClient);
        var serviceContext = new TenantServiceContext(tenantId);
        var remainingOpenTasks = new List<Task>(openTasks);

        // create or update task
        foreach (var updateTask in updateTasks)
        {
            var task = remainingOpenTasks.FirstOrDefault(
                                            x => x.ScheduledUserId == updateTask.ScheduledUserId &&
                                            string.Equals(x.Name, updateTask.Name));
            // create new task
            if (task == null)
            {
                // ignore previous updates
                if (string.IsNullOrWhiteSpace(updateTask.Instruction))
                {
                    continue;
                }

                // new task
                task = await taskService.CreateAsync(serviceContext, updateTask);
                tasks.Add(task);
            }
            else
            {
                // update open task
                var update = false;
                if (updateTask.Completed.HasValue)
                {
                    if (task.Completed != updateTask.Completed)
                    {
                        // complete task
                        task.Completed = updateTask.Completed;
                        task.CompletedUserId = updateTask.CompletedUserId;
                        update = true;
                    }
                }
                else if (task.Scheduled != updateTask.Scheduled)
                {
                    // reschedule task
                    task.Scheduled = updateTask.Scheduled;
                    update = true;
                }

                // update task
                if (update)
                {
                    await taskService.UpdateAsync(serviceContext, task);
                }
                tasks.Add(task);

                // update remaining open tasks
                remainingOpenTasks.Remove(task);
            }
        }

        // remaining open tasks
        tasks.AddRange(remainingOpenTasks);

        return tasks;
    }

    /// <summary>Read task from CVS file</summary>
    /// <param name="users">The tenant users</param>
    /// <param name="fileName">The CSV file name</param>
    private static List<Task> GetTasksFromCsv(List<User> users, string fileName)
    {
        var tasks = new List<Task>();
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return tasks;
        }

        // deserialize CSV file to user tasks
        var csvTasks = CsvSerializer.FromFile(fileName, ignoreFirstLine: true);

        // process CSV rows
        var now = Date.Now;
        foreach (var csvTask in csvTasks)
        {
            if (csvTask.Count < 5)
            {
                throw new PayrollException($"Invalid task csv line: {string.Join(',', csvTask)}.");
            }

            // user
            var userIdentifier = csvTask[0];
            var user = users.FirstOrDefault(x => string.Equals(x.Identifier, userIdentifier));
            if (user == null)
            {
                throw new PayrollException($"Invalid task user {userIdentifier}.");
            }

            // name
            var name = csvTask[1];
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new PayrollException("Missing task name.");
            }

            // instruction, mandatory for new tasks
            var instruction = csvTask[2];

            // category (optional)
            var category = csvTask[3];

            // date in UTC
            if (!DateTime.TryParse(csvTask[4], null, DateTimeStyles.AdjustToUniversal, out var date))
            {
                throw new PayrollException($"Invalid task date {csvTask[4]}.");
            }

            // create new scheduled task
            var task = new Task
            {
                Name = name,
                Instruction = instruction,
                Category = category,
                Scheduled = date,
                ScheduledUserId = user.Id
            };

            // treat past date as task completed date
            if (date < now)
            {
                task.Completed = date;
                task.CompletedUserId = user.Id;
            }
            tasks.Add(task);
        }

        return tasks;
    }

    /// <summary>Get open tasks for all active tenant users</summary>
    /// <param name="tenantId">The tenant id</param>
    private async Tasks.Task<List<Task>> GetOpenTasksAsync(int tenantId) =>
        await new TaskService(HttpClient).QueryAsync<Task>(new(tenantId), new()
        {
            // active tasks
            Status = ObjectStatus.Active,
            // uncompleted tasks
            Filter = $"{nameof(Task.Completed)} eq null"
        });

    /// <summary>Get active tenant users</summary>
    /// <param name="tenantId">The tenant id</param>
    private async Tasks.Task<List<User>> GetUsersAsync(int tenantId) =>
        await new UserService(HttpClient).QueryAsync<User>(new(tenantId), new()
        {
            // active only
            Status = ObjectStatus.Active
        });

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

    /// <summary>Display tasks grouped by user</summary>
    /// <param name="tenant">The tenant</param>
    /// <param name="tasks">The tasks to display</param>
    /// <param name="users">The tenant users</param>
    private static void DisplayTasks(Tenant tenant, List<Task> tasks, List<User> users)
    {
        if (!tasks.Any())
        {
            WriteInfoLine($"{tenant.Identifier}: no tasks available");
            return;
        }

        // title
        WriteTitleLine($"User tasks of {tenant.Identifier}");

        // table
        var line = new string('-', 25 + 20 + 25 + 20 + 25 + 25);
        WriteLine();
        WriteLine(line);
        WriteLine($"{"User",-25}{"Task",-20}{"Instruction",-25}{"Category",-20}{"Scheduled",-25}{"Completed",-25}");
        WriteLine(line);

        // open user tasks
        var now = Date.Now;
        var sortedTasks = tasks.OrderBy(x => x.ScheduledUserId).ThenByDescending(x => x.Scheduled);
        foreach (var task in sortedTasks)
        {
            // user
            var user = users.FirstOrDefault(user => user.Id == task.ScheduledUserId);
            if (user == null)
            {
                continue;
            }

            // task display
            Write($"{user.Identifier,-25}{task.Name,-20}{task.Instruction,-25}{task.Category,-20}");
            // task schedule date
            var scheduleText = task.Scheduled.ToCompactString();
            if (!task.Completed.HasValue)
            {
                if (task.Scheduled < now)
                {
                    WriteError($"{scheduleText,-25}");
                }
                else
                {
                    WriteSuccess($"{scheduleText,-25}");
                }
            }
            else
            {
                Write($"{scheduleText,-25}");
            }
            // task complete date
            if (task.Completed.HasValue)
            {
                var completedText = task.Completed.Value.ToCompactString();
                WriteSuccess($"{completedText,-20}");
            }
            WriteLine();
        }
        WriteLine(line);

        // statistics
        WriteLine();
        WriteLine($"Total tasks     {tasks.Count}");
        var pendingCount = tasks.Count(x => !x.Completed.HasValue && x.Scheduled > now);
        if (pendingCount > 0)
        {
            WriteLine($"Pending tasks   {pendingCount}");
        }
        var completeCount = tasks.Count(x => x.Completed.HasValue);
        if (completeCount > 0)
        {
            WriteLine($"Complete tasks  {completeCount}");
        }
        var openCount = tasks.Count(x => !x.Completed.HasValue && x.Scheduled < now);
        if (openCount > 0)
        {
            WriteErrorLine($"Open tasks      {openCount}");
        }
    }

    /// <inheritdoc />
    protected override Tasks.Task HelpAsync()
    {
        WriteLine("Usage: CreateAndModifyObjects Tenant [Tasks.csv]");
        WriteLine();
        WriteLine("Arguments:");
        WriteLine("  1. Tenant identifier");
        WriteLine("  2. CSV file containing the user tasks (optional)");
        WriteLine();
        WriteLine("CSV columns:");
        WriteLine("  1. User         the user identifier");
        WriteLine("  2. Name         the task name");
        WriteLine("  3. Instruction  the task instruction (mandatory for new task)");
        WriteLine("  4. Category     the task category (optional for new task)");
        WriteLine("  5. Date         the schedule date (future) or complete date (past) in UTC");
        WriteLine();
        WriteLine("Examples:");
        WriteLine("  CreateAndModifyObjects MyTenant");
        WriteLine("  CreateAndModifyObjects MyTenant MyTasks.csv");
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
