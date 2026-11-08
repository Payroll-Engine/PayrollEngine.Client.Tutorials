using System;
using System.Security.Cryptography;
using System.Text;

namespace PayrollEngine.Client.Tutorial.WebhookConsumer;

/// <summary>
/// HMAC-SHA256 signatures of the Payroll Engine webhooks.
/// Request: <c>X-PE-Signature</c> over <c>{timestamp}.{body}</c>.
/// Response of a function request: <c>X-PE-Response-Signature</c> over <c>{timestamp}.{requestSignature}.{responseBody}</c>.
/// </summary>
public static class WebhookSignature
{
    /// <summary>Request header with the unix timestamp in seconds</summary>
    public const string TimestampHeader = "X-PE-Timestamp";

    /// <summary>Request header with the request signature</summary>
    public const string SignatureHeader = "X-PE-Signature";

    /// <summary>Response header with the response signature</summary>
    public const string ResponseSignatureHeader = "X-PE-Response-Signature";

    private const string Version = "v1=";

    /// <summary>Maximum age of a request, protection against replayed requests</summary>
    public static readonly TimeSpan MaxRequestAge = TimeSpan.FromMinutes(5);

    /// <summary>Verify the signature and the age of a request</summary>
    /// <param name="secret">The webhook secret</param>
    /// <param name="timestamp">The timestamp header</param>
    /// <param name="signature">The signature header</param>
    /// <param name="body">The raw request body</param>
    /// <returns>Null for a valid request, otherwise the error</returns>
    public static string VerifyRequest(string secret, string timestamp, string signature, string body)
    {
        if (string.IsNullOrWhiteSpace(timestamp) || string.IsNullOrWhiteSpace(signature))
        {
            return "missing webhook signature";
        }
        if (!long.TryParse(timestamp, out var seconds))
        {
            return "invalid webhook timestamp";
        }
        var age = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(seconds);
        if (age.Duration() > MaxRequestAge)
        {
            return "expired webhook timestamp";
        }
        return Equals(Compute(secret, $"{timestamp}.{body}"), signature.Trim())
            ? null
            : "invalid webhook signature";
    }

    /// <summary>Sign the response of a function request</summary>
    /// <param name="secret">The webhook secret</param>
    /// <param name="timestamp">The timestamp header of the request</param>
    /// <param name="requestSignature">The signature header of the request</param>
    /// <param name="body">The response body</param>
    /// <returns>The response signature</returns>
    public static string SignResponse(string secret, string timestamp, string requestSignature, string body) =>
        Compute(secret, $"{timestamp}.{requestSignature}.{body}");

    private static string Compute(string secret, string content)
    {
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(content));
        return Version + Convert.ToHexStringLower(hash);
    }

    private static bool Equals(string expected, string actual) =>
        CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(expected), Encoding.ASCII.GetBytes(actual));
}
