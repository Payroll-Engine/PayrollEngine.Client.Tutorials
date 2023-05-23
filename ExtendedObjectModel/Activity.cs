using System;

namespace PayrollEngine.Client.Tutorial.ExtendedObjectModel
{
    /// <summary>Activity</summary>
    public class Activity
    {
        /// <summary>Activity state</summary>
        public ActivityStateCode State { get; set; }

        /// <summary>Activity id</summary>
        public Guid ActivityId { get; set; }

        /// <summary>Activity name</summary>
        public string Name { get; set; }
    }
}
