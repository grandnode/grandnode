using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using System.Web.Mvc;

namespace Grand.Core.Domain.Customers
{
    /// <summary>
    /// Represents a customer reminder history
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class CustomerReminderHistory : BaseEntity
    {
        private ICollection<HistoryLevel> _level;

        public string CustomerReminderId { get; set; }
        public string CustomerId { get; set; }

        public int ReminderRuleId { get; set; }

        [BsonIgnoreAttribute]
        public CustomerReminderRuleEnum ReminderRule
        {
            get { return (CustomerReminderRuleEnum)ReminderRuleId; }
            set { this.ReminderRuleId = (int)value; }
        }

        public int Status { get; set; }

        [BsonIgnoreAttribute]
        public CustomerReminderHistoryStatusEnum HistoryStatus
        {
            get { return (CustomerReminderHistoryStatusEnum)Status; }
            set { this.Status = (int)value; }
        }
        public string OrderId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        
        /// <summary>
        /// Gets or sets the reminder level history
        /// </summary>
        public virtual ICollection<HistoryLevel> Levels
        {
            get { return _level ?? (_level = new List<HistoryLevel>()); }
            protected set { _level = value; }
        }


        [BsonIgnoreExtraElements]
        public partial class HistoryLevel : SubBaseEntity
        {
            public string ReminderLevelId { get; set; }
            public int Level { get; set; }
            public DateTime SendDate { get; set; }

        }

    }
}
