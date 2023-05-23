using System;
using System.Text.Json.Serialization;
using PayrollEngine.Client.Model;

namespace PayrollEngine.Client.Tutorial.ExtendedObjectModel
{
    /// <summary>Extended employee with Erp attribute fields</summary>
    public class MyEmployee : Employee
    {
        /// <summary>The Erp id</summary>
        [JsonIgnore]
        public Guid ErpId
        {
            get => this.GetAttributeGuid(nameof(ErpId));
            set => this.SetAttributeGuid(nameof(ErpId), value);
        }
    }
}
