using System.Text.Json.Serialization;

namespace PayrollEngine.Client.Tutorial.WebhookConsumer;

/// <summary>The Webhook message type</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WebhookAction
{
    /// <summary>No message</summary>
    None,

    /// <summary>Case function request</summary>
    CaseFunctionRequest,

    /// <summary>Case change added</summary>
    CaseChangeAdded,

    /// <summary>Payrun function request</summary>
    PayrunFunctionRequest,

    /// <summary>Process payrun job</summary>
    PayrunJobProcess,

    /// <summary>Payrun job finished</summary>
    PayrunJobFinish,

    /// <summary>Report function request</summary>
    ReportFunctionRequest
}