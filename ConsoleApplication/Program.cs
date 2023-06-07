using System.Threading.Tasks;

namespace PayrollEngine.Client.Tutorial.ConsoleApplication;

/// <summary>The console tutorial program</summary>
internal class Program : ConsoleProgram<Program>
{
    /// <inheritdoc />
    protected override Task RunAsync()
    {
        WriteSuccessLine("Hello, Payroll Engine World!");
        PressAnyKey();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override async Task<bool> InitializeAsync() =>
        await HttpClient.IsConnectionAvailableAsync() && await base.InitializeAsync();

    /// <summary>Override the program with <see cref="ProgramConfiguration"/></summary>
    /// <returns>The program culture</returns>
    protected override Task<string> GetProgramCultureAsync()
    {
        var configuration = Configuration.GetConfiguration<ProgramConfiguration>();
        var culture = configuration?.StartupCulture;
        if (!string.IsNullOrWhiteSpace(culture))
        {
            return Task.FromResult(culture);
        }
        return base.GetProgramCultureAsync();
    }

    /// <summary>Override the program http configuration with command line arguments</summary>
    /// <returns>The program http configuration</returns>
    protected override Task<PayrollHttpConfiguration> GetHttpConfigurationAsync()
    {
        var baseUrl = ConsoleArguments.Get(1, "BaseUrl");
        var port = ConsoleArguments.GetInt(2, "Port");
        if (!string.IsNullOrWhiteSpace(baseUrl) && port != default)
        {
            // configuration by arguments
            return Task.FromResult(new PayrollHttpConfiguration(baseUrl, port.Value));
        }
        return base.GetHttpConfigurationAsync();
    }

    /// <summary>Program entry point</summary>
    private static async Task Main()
    {
        // init logger
        Log.SetLogger(new Serilog.PayrollLog());

        // execute program
        using var program = new Program();
        await program.ExecuteAsync();
    }
}
