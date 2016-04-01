using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using System.Web.Mvc;

namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// Represents a customer reminder 
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class CustomerReminder : BaseEntity
    {
        private ICollection<ReminderCondition> _condition;

        private ICollection<ReminderLevel> _level;

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        public int RenewedDay { get; set; }
        /// <summary>
        /// Gets or sets display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets active action
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the action conditions
        /// </summary>
        public int ConditionId { get; set; }

        [BsonIgnoreAttribute]
        public CustomerReminderConditionEnum Condition
        {
            get { return (CustomerReminderConditionEnum)ConditionId; }
            set { this.ConditionId = (int)value; }
        }

        /// <summary>
        /// Gets or sets the reminder rule
        /// </summary>
        public int ReminderRuleId { get; set; }

        [BsonIgnoreAttribute]
        public CustomerReminderRuleEnum ReminderRule
        {
            get { return (CustomerReminderRuleEnum)ReminderRuleId; }
            set { this.ReminderRuleId = (int)value; }
        }

        /// <summary>
        /// Gets or sets the customer condition
        /// </summary>
        public virtual ICollection<ReminderCondition> Conditions
        {
            get { return _condition ?? (_condition = new List<ReminderCondition>()); }
            protected set { _condition = value; }
        }

        /// <summary>
        /// Gets or sets the reminder level
        /// </summary>
        public virtual ICollection<ReminderLevel> Levels
        {
            get { return _level ?? (_level = new List<ReminderLevel>()); }
            protected set { _level = value; }
        }


        [BsonIgnoreExtraElements]
        public partial class ReminderCondition
        {
            private ICollection<int> _products;
            private ICollection<int> _categories;
            private ICollection<int> _manufacturers;
            private ICollection<int> _customerRoles;
            private ICollection<int> _customerTags;
            private ICollection<CustomerRegister> _customerRegister;
            private ICollection<CustomerRegister> _customCustomerAttributes;

            public int Id { get; set; }
            public string Name { get; set; }

            public int ConditionTypeId { get; set; }

            [BsonIgnoreAttribute]
            public CustomerReminderConditionTypeEnum ConditionType
            {
                get { return (CustomerReminderConditionTypeEnum)ConditionTypeId; }
                set { this.ConditionTypeId = (int)value; }
            }

            public int ConditionId { get; set; }

            [BsonIgnoreAttribute]
            public CustomerReminderConditionEnum Condition
            {
                get { return (CustomerReminderConditionEnum)ConditionId; }
                set { this.ConditionId = (int)value; }
            }


            public virtual ICollection<int> Products
            {
                get { return _products ?? (_products = new List<int>()); }
                protected set { _products = value; }
            }

            public virtual ICollection<int> Categories
            {
                get { return _categories ?? (_categories = new List<int>()); }
                protected set { _categories = value; }
            }

            public virtual ICollection<int> Manufacturers
            {
                get { return _manufacturers ?? (_manufacturers = new List<int>()); }
                protected set { _manufacturers = value; }
            }

            public virtual ICollection<int> CustomerRoles
            {
                get { return _customerRoles ?? (_customerRoles = new List<int>()); }
                protected set { _customerRoles = value; }
            }
            public virtual ICollection<int> CustomerTags
            {
                get { return _customerTags ?? (_customerTags = new List<int>()); }
                protected set { _customerTags = value; }
            }

            public virtual ICollection<CustomerRegister> CustomerRegistration
            {
                get { return _customerRegister ?? (_customerRegister = new List<CustomerRegister>()); }
                protected set { _customerRegister = value; }
            }

            public virtual ICollection<CustomerRegister> CustomCustomerAttributes
            {
                get { return _customCustomerAttributes ?? (_customCustomerAttributes = new List<CustomerRegister>()); }
                protected set { _customCustomerAttributes = value; }
            }


            [BsonIgnoreExtraElements]
            public partial class CustomerRegister
            {
                public int Id { get; set; }
                public string RegisterField { get; set; }
                public string RegisterValue { get; set; }
            }

        }

        [BsonIgnoreExtraElements]
        public partial class ReminderLevel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Level { get; set; }
            public int Day { get; set; }
            public int Hour { get; set; }
            public int EmailAccountId { get; set; }
            public string BccEmailAddresses { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
        }
       
    }
}
