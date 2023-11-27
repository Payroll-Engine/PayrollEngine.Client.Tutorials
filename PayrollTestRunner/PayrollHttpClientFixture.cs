using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PayrollEngine.Client.Tutorial.PayrollTestRunner
{
    public class PayrollHttpClientFixture : IDisposable
    {
        private readonly HttpClientHandler clientHandler;
        public PayrollHttpClient HttpClient { get; }

        public PayrollHttpClientFixture()
        {
            clientHandler = new HttpClientHandler();
            HttpClient = GetHttpClientAsync(clientHandler).Result;
        }

        private static async Task<PayrollHttpClient> GetHttpClientAsync(HttpClientHandler clientHandler)
        {
            var config = await SharedHttpConfiguration.GetHttpConfigurationAsync();
            return config == null ? null : new PayrollHttpClient(clientHandler, config);
        }

        public void Dispose()
        {
            clientHandler?.Dispose();
            HttpClient?.Dispose();
        }
    }
}
