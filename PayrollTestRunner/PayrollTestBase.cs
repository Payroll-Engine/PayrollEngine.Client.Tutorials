using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PayrollEngine.Serialization;
using PayrollEngine.Serilog;
using Xunit;

namespace PayrollEngine.Client.Tutorial.PayrollTestRunner;

public abstract class PayrollTestBase : IClassFixture<PayrollHttpClientFixture>
{
    protected PayrollHttpClientFixture Fixture { get; }
    protected PayrollHttpClient HttpClient => Fixture.HttpClient;

    protected PayrollTestBase(PayrollHttpClientFixture fixture)
    {
        Log.SetLogger(new PayrollLog());
        Fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    protected async Task<T> GetMemberResourceAsync<T>([CallerMemberName] string caller = null)
    {
        var resourceName = caller.EnsureEnd(".json");
        var result = await JsonSerializer.DeserializeFromResourceAsync<T>(GetType(), resourceName);
        return result;
    }
}