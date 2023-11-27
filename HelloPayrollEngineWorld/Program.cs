using System;
using System.Net.Http;

namespace PayrollEngine.Client.Tutorial.HelloPayrollEngineWorld;

/// <summary>The Payroll Engine hello world tutorial program</summary>
internal class Program
{
    /// <summary>Program entry point</summary>
    static void Main()
    {
        // http client
        using var clientHandler = new HttpClientHandler();
        using var payrollHttpClient = new PayrollHttpClient(clientHandler, "https://localhost", 44354);

        // connection test
        if (!payrollHttpClient.IsConnectionAvailable())
        {
            Console.WriteLine($"Backend connection {payrollHttpClient.Address} is not available.");
            Console.ReadKey();
            return;
        }

        // connection available
        Console.WriteLine("Hello, Payroll Engine World!");
        Console.ReadKey();
    }
}
