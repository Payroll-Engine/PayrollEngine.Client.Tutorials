using System.Text.Json.Serialization;

namespace PayrollEngine.Client.Tutorial.WebhookConsumer;

/// <summary>The object status</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ObjectStatus
{
    /// <summary>Object is active</summary>
    Active,

    /// <summary>Object is inactive</summary>
    Inactive
}