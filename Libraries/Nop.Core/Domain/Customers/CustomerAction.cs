using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// Represents a customer action
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class CustomerAction : BaseEntity
    {
        private ICollection<ActionCondition> _condition;
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets active action
        /// </summary>
        public bool Active { get; set; }


        /// <summary>
        /// Gets or sets the action Type
        /// </summary>
        public int ActionTypeId { get; set; }

        /// <summary>
        /// Gets or sets the action conditions
        /// </summary>
        public int ConditionId { get; set; }

        [BsonIgnoreAttribute]
        public CustomerActionConditionEnum Condition
        {
            get { return (CustomerActionConditionEnum)ConditionId; }
            set { this.ConditionId = (int)value; }
        }


        /// <summary>
        /// Gets or sets the action conditions
        /// </summary>
        public int ReactionTypeId { get; set; }

        [BsonIgnoreAttribute]
        public CustomerReactionTypeEnum ReactionType
        {
            get { return (CustomerReactionTypeEnum)ReactionTypeId; }
            set { this.ReactionTypeId = (int)value; }
        }

        public int BannerId { get; set; }

        public int MessageTemplateId { get; set; }

        public int CustomerRoleId { get; set; }

        public int CustomerTagId { get; set; }
        /// <summary>
        /// Gets or sets the start date 
        /// </summary>
        public DateTime StartDateTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the end date
        /// </summary>
        public DateTime EndDateTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the customer roles
        /// </summary>
        public virtual ICollection<ActionCondition> Conditions
        {
            get { return _condition ?? (_condition = new List<ActionCondition>()); }
            protected set { _condition = value; }
        }

        [BsonIgnoreExtraElements]
        public partial class ActionCondition
        {
            private ICollection<int> _products;
            private ICollection<int> _categories;
            private ICollection<int> _manufacturers;
            private ICollection<int> _vendors;
            private ICollection<int> _customerRoles;
            private ICollection<int> _customerTags;
            private ICollection<ProductAttributeValue> _productAttribute;
            private ICollection<ProductSpecification> _productSpecification;
            private ICollection<CustomerRegister> _customerRegister;
            private ICollection<CustomerRegister> _customCustomerAttributes;
            private ICollection<Url> _urlReferrer;
            private ICollection<Url> _urlCurrent;

            public int Id { get; set; }
            public string Name { get; set; }

            public int CustomerActionConditionTypeId { get; set; }

            [BsonIgnoreAttribute]
            public CustomerActionConditionTypeEnum CustomerActionConditionType
            {
                get { return (CustomerActionConditionTypeEnum)CustomerActionConditionTypeId; }
                set { this.CustomerActionConditionTypeId = (int)value; }
            }            

            public int ConditionId { get; set; }

            [BsonIgnoreAttribute]
            public CustomerActionConditionEnum Condition
            {
                get { return (CustomerActionConditionEnum)ConditionId; }
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

            public virtual ICollection<int> Vendors
            {
                get { return _vendors ?? (_vendors = new List<int>()); }
                protected set { _vendors = value; }
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
            public virtual ICollection<ProductAttributeValue> ProductAttribute
            {
                get { return _productAttribute ?? (_productAttribute = new List<ProductAttributeValue>()); }
                protected set { _productAttribute = value; }
            }

            public virtual ICollection<ProductSpecification> ProductSpecifications
            {
                get { return _productSpecification ?? (_productSpecification = new List<ProductSpecification>()); }
                protected set { _productSpecification = value; }
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

            public virtual ICollection<Url> UrlReferrer
            {
                get { return _urlReferrer ?? (_urlReferrer = new List<Url>()); }
                protected set { _urlReferrer = value; }
            }

            public virtual ICollection<Url> UrlCurrent
            {
                get { return _urlCurrent ?? (_urlCurrent = new List<Url>()); }
                protected set { _urlCurrent = value; }
            }

            [BsonIgnoreExtraElements]
            public partial class ProductAttributeValue
            {
                public int Id { get; set; }
                public int ProductAttributeId { get; set; }
                public string Name { get; set; }
            }

            public partial class Url
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }

            [BsonIgnoreExtraElements]
            public partial class ProductSpecification
            {
                public int Id { get; set; }
                public int ProductSpecyficationId { get; set; }
                public int ProductSpecyficationValueId { get; set; }
            }

            [BsonIgnoreExtraElements]
            public partial class CustomerRegister
            {
                public int Id { get; set; }
                public string RegisterField { get; set; }
                public string RegisterValue { get; set; }
            }

        }

    }
}
