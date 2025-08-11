using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PayrollEngine.Client.Tutorial.HelloPayrollEngineWorld;

/// <summary>The Payroll Engine hello world tutorial program</summary>
internal class Program
{
    /// <summary>Program entry point</summary>
    static async Task Main()
    {
        // http client
        using var clientHandler = new HttpClientHandler();
        using var payrollHttpClient = new PayrollHttpClient(clientHandler, "https://localhost", 44354);

        // connection test
        if (! await payrollHttpClient.IsConnectionAvailableAsync(TenantApiEndpoints.TenantsUrl()))
        {
            Console.WriteLine($"Backend connection {payrollHttpClient.Address} is not available.");
        }
        else
        {
            // connection available
            Console.WriteLine("Hello, Payroll Engine World!");
        }
        Console.Read();
    }
}
