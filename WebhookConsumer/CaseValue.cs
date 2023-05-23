using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Client.Tutorial.WebhookConsumer;

/// <summary>
/// The payroll case value API object (immutable)
/// </summary>
public class CaseValue
{
    /// <summary>
    /// The division id (immutable)
    /// </summary>
    public int? DivisionId { get; set; }

    /// <summary>
    /// The employee id, mandatory for employee case changes
    /// </summary>
    public int? EmployeeId { get; set; }

    /// <summary>
    /// The associated case name
    /// </summary>
    [StringLength(128)]
    public string CaseName { get; set; }

    /// <summary>
    /// The localized case names
    /// </summary>
    public Dictionary<string, string> CaseNameLocalizations { get; set; }

    /// <summary>
    /// The associated case field name
    /// </summary>
    [Required]
    [StringLength(128)]
    public string CaseFieldName { get; set; }

    /// <summary>
    /// The localized case field names
    /// </summary>
    public Dictionary<string, string> CaseFieldNameLocalizations { get; set; }

    /// <summary>
    /// The case slot
    /// </summary>
    [StringLength(128)]
    public string CaseSlot { get; set; }

    /// <summary>
    /// The localized case slots
    /// </summary>
    public Dictionary<string, string> CaseSlotLocalizations { get; set; }

    /// <summary>
    /// The type of the value
    /// </summary>
    public ValueType ValueType { get; set; }

    /// <summary>
    /// The case value (JSON format)
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// The case relation
    /// </summary>
    public CaseRelationReference CaseRelation { get; set; }

    /// <summary>
    /// Cancellation date
    /// </summary>
    public DateTime? CancellationDate { get; set; }

    /// <summary>
    /// The starting date for the value
    /// </summary>
    public DateTime? Start { get; set; }

    /// <summary>
    /// The ending date for the value
    /// </summary>
    public DateTime? End { get; set; }

    /// <summary>
    /// The forecast name
    /// </summary>
    [StringLength(128)]
    public string Forecast { get; set; }

    /// <summary>
    /// The case value key
    /// </summary>
    [StringLength(128)]
    public string Key { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() => string.IsNullOrWhiteSpace(CaseSlot)
        ? $"{CaseName}.{CaseFieldName}={Value}"
        : $"{CaseName}.{CaseFieldName}[{CaseSlot}]={Value}";
}