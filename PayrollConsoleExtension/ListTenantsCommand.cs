using System;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Client.Command;
using PayrollEngine.Client.Model;
using PayrollEngine.Client.Service.Api;

namespace PayrollEngine.Client.Tutorial.PayrollConsoleExtension;

[Command("ListTenants")]
// ReSharper disable once UnusedType.Global
internal sealed class ListTenantsCommand : CommandBase<ListTenantsParameters>
{
    /// <summary>Process the variable</summary>
    protected override async Task<int> Execute(CommandContext context, ListTenantsParameters parameters)
    {
        var console = context.Console;

        // header
        console.DisplayTitleLine("List Tenants");
        if (context.DisplayLevel == DisplayLevel.Full)
        {
            console.DisplayTextLine($"Order            {parameters.Order}");
        }

        // tenants query
        var service = new TenantService(context.HttpClient);
        var tenants = await service.QueryAsync<Tenant>(new(), new()
        {
            OrderBy = GetOrderQuery(parameters)
        });
        if (!tenants.Any())
        {
            console.DisplayErrorLine("No tenants available.");
            return 0;
        }

        // tenant display
        var line = new string('-', 1 + 30 + 25 + 15 + 10 + 1);
        console.DisplayNewLine();
        console.DisplayTextLine(line);
        console.DisplayTextLine($" {"Identifier",-30}{"Created",-25}{"Status",-15}{"Id",10} ");
        console.DisplayTextLine(line);
        foreach (var tenant in tenants)
        {
            console.DisplayTextLine(
                $" {tenant.Identifier,-30}{tenant.Created,-25:g}{tenant.Status,-15}{tenant.Id,10} ");
        }
        console.DisplayTextLine(line);

        console.DisplayNewLine();
        console.DisplaySuccessLine($"Total {tenants.Count} tenants.");

        return 0;
    }

    /// <summary>
    /// Build query parameter by command parameters.
    /// </summary>
    /// <param name="parameters">Command parameters.</param>
    private static string GetOrderQuery(ListTenantsParameters parameters)
    {
        var ascending = " ASC";
        var descending = " DESC";
        switch (parameters.Order)
        {
            case Order.CreatedAscending:return nameof(Tenant.Created) + ascending;
            case Order.CreatedDescending:return nameof(Tenant.Created) + descending;
            case Order.IdentifierAscending:return nameof(Tenant.Identifier) + ascending;
            case Order.IdentifierDescending:return nameof(Tenant.Identifier) + descending;
            case Order.StatusAscending:return nameof(Tenant.Status) + ascending;
            case Order.StatusDescending:return nameof(Tenant.Status) + descending;
            case Order.IdAscending: return nameof(Tenant.Id) + ascending;
            case Order.IdDescending: return nameof(Tenant.Id) + descending;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override ICommandParameters GetParameters(CommandLineParser parser) =>
        ListTenantsParameters.ParserFrom(parser);

    public override void ShowHelp(ICommandConsole console)
    {
        console.DisplayTitleLine("- ListTenants");
        console.DisplayTextLine("      List payroll tenants");
        console.DisplayTextLine("      Arguments:");
        console.DisplayTextLine("          1. text to write [Text] (optional)");
        console.DisplayTextLine("      Toggles:");
        console.DisplayTextLine("          order by: /identifierAscending, /identifierAscending");
        console.DisplayTextLine("                    /createdAscending, /createdAscending");
        console.DisplayTextLine("                    /statusAscending, /statusAscending");
        console.DisplayTextLine("                    /idAscending, /idAscending (default: identifierAscending)");
        console.DisplayTextLine("      Examples:");
        console.DisplayTextLine("          ListTenants");
        console.DisplayTextLine("          ListTenants /byCreatedDescending");
    }
}