using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Serilog;
using Tasks = System.Threading.Tasks;

namespace PayrollEngine.Client.Tutorial.WebhookConsumer;

/// <summary>
/// Verifies the signature of the webhook requests before the model binding.
/// Without configured secret (<c>Webhook:Secret</c>) the requests are accepted unsigned.
/// </summary>
public class WebhookSignatureFilter(IConfiguration configuration) : IAsyncResourceFilter
{
    /// <summary>Configuration key of the webhook secret</summary>
    public const string SecretKey = "Webhook:Secret";

    /// <inheritdoc />
    public async Tasks.Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var secret = configuration[SecretKey];
        var request = context.HttpContext.Request;
        if (string.IsNullOrWhiteSpace(secret) || !HttpMethods.IsPost(request.Method))
        {
            await next();
            return;
        }

        // the signature covers the raw body: read it and rewind the stream for the model binding
        request.EnableBuffering();
        string body;
        using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
        {
            body = await reader.ReadToEndAsync();
        }
        request.Body.Position = 0;

        var error = WebhookSignature.VerifyRequest(secret,
            request.Headers[WebhookSignature.TimestampHeader],
            request.Headers[WebhookSignature.SignatureHeader], body);
        if (error != null)
        {
            Log.Warning($"Rejected webhook request {request.Path}: {error}");
            context.Result = new UnauthorizedObjectResult(error);
            return;
        }
        await next();
    }
}
