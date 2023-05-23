using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Client.Tutorial.WebhookConsumer;

public class WebhookRuntimeMessage : WebhookMessage
{
    /// <summary>
    /// The tenant identifier
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Tenant { get; set; }

    /// <summary>
    /// The user identifier
    /// </summary>
    [Required]
    [StringLength(128)]
    public string User { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Tenant} {User}: {ActionName} {base.ToString()}";
}