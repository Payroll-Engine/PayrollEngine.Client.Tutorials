using System.Linq;
using System.Threading.Tasks;
using Xunit;
using PayrollEngine.Client.Test.Payrun;
using PayrollEngine.Client.Scripting.Script;

namespace PayrollEngine.Client.Tutorial.PayrollTestRunner;

public class PayrunTests(PayrollHttpClientFixture fixture) : PayrollTestBase(fixture)
{
    [Fact]
    public async Task PayrunTest()
    {
        // test data
        var exchange = await GetMemberResourceAsync<Model.Exchange>();

        // test runner
        var testRunner = new PayrunTestRunner(HttpClient, new ScriptParser());
        var results = await testRunner.TestAllAsync(exchange);
        var anyFailed = results.First().Value.Any(x => x.Failed);
        Assert.False(anyFailed);
    }

    [Fact]
    public async Task PayrunTestFailure()
    {
        // test data
        var exchange = await GetMemberResourceAsync<Model.Exchange>();

        // test runner
        var testRunner = new PayrunTestRunner(HttpClient, new ScriptParser());
        var results = await testRunner.TestAllAsync(exchange);
        var anyWageTypFailed = results.First().Value.Any(x => x.FailedWageTypeResult);
        Assert.True(anyWageTypFailed);
    }
}