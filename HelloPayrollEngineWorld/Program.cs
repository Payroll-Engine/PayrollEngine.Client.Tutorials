using System;

namespace PayrollEngine.Client.Tutorial.HelloPayrollEngineWorld;

/// <summary>The Payroll Engine hello world tutorial program</summary>
internal class Program
{
    /// <summary>Program entry point</summary>
    static void Main()
    {
        // http client
        using var httpClient = new PayrollHttpClient("https://localhost", 44354);

        // connection test
        if (!httpClient.IsConnectionAvailable())
        {
            Console.WriteLine($"Backend connection {httpClient.Address} is not available.");
            Console.ReadKey();
            return;
        }

        // connection available
        Console.WriteLine("Hello, Payroll Engine World!");
        Console.ReadKey();
    }
}
