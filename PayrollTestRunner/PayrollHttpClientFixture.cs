using System;
using System.Threading.Tasks;

namespace PayrollEngine.Client.Tutorial.PayrollTestRunner
{
    public class PayrollHttpClientFixture : IDisposable
    {
        public PayrollHttpClient HttpClient { get; }

        public PayrollHttpClientFixture()
        {
            HttpClient = GetHttpClientAsync().Result;
        }

        private static async Task<PayrollHttpClient> GetHttpClientAsync()
        {
            var config = await SharedHttpConfiguration.GetHttpConfigurationAsync();
            return config == null ? null : new PayrollHttpClient(config);
        }

        public void Dispose()
        {
            HttpClient?.Dispose();
        }
    }
}
