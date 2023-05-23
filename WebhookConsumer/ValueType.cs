using System.Text.Json.Serialization;

namespace PayrollEngine.Client.Tutorial.WebhookConsumer;

/// <summary>The payroll value types for cases</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ValueType
{
    /// <summary>String (base type string)</summary>
    String,
    /// <summary>Boolean (base type boolean)</summary>
    Boolean,
    /// <summary>Integer (base type numeric)</summary>
    Integer,
    /// <summary>Numeric boolean, any non-zero value means true (base type numeric)</summary>
    NumericBoolean,
    /// <summary>Decimal (base type numeric)</summary>
    Decimal,
    /// <summary>Date and time (base type string)</summary>
    DateTime,
    /// <summary>No value type (base type null)</summary>
    None,

    /// <summary>Date (base type string)</summary>
    Date,
    /// <summary>Web Resource e.g. Url (base type string)</summary>
    WebResource,

    /// <summary>Money (base type numeric)</summary>
    Money,
    /// <summary>Percentage (base type numeric)</summary>
    Percent,
    /// <summary>Hour (base type numeric)</summary>
    Hour,
    /// <summary>Day (base type numeric)</summary>
    Day,
    /// <summary>Week (base type numeric)</summary>
    Week,
    /// <summary>Month (base type numeric)</summary>
    Month,
    /// <summary>Year (base type numeric)</summary>
    Year,
    /// <summary>Distance (base type numeric)</summary>
    Distance
}