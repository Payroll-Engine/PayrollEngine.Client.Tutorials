﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Client.Tutorial.WebhookConsumer;

/// <summary>
/// Payroll task
/// </summary>
public class Task : ApiObject
{
    /// <summary>
    /// The task name (immutable)
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    /// <summary>
    /// The task category
    /// </summary>
    [StringLength(128)]
    public string Category { get; set; }

    /// <summary>
    /// The task instruction
    /// </summary>
    [Required]
    public string Instruction { get; set; }

    /// <summary>
    /// The scheduled user id
    /// </summary>
    [Required]
    public int ScheduledUserId { get; set; }

    /// <summary>
    /// The task schedule date
    /// </summary>
    [Required]
    public DateTime Scheduled { get; set; }


    /// <summary>
    /// The completed user id
    /// </summary>
    public int? CompletedUserId { get; set; }

    /// <summary>
    /// The task completed date
    /// </summary>
    public DateTime? Completed { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name}: {Scheduled} {base.ToString()}";
}