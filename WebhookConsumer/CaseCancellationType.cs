using System.Text.Json.Serialization;

namespace PayrollEngine.Client.Tutorial.WebhookConsumer;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CaseCancellationType
{
    /// <summary>No cancellation support</summary>
    None,

    /// <summary>Cancellation by case</summary>
    Case
}