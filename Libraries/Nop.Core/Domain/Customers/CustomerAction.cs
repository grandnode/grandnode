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
            private ICollection<ProductAttributeValue> _productAttribute;
            private ICollection<ProductSpecification> _productSpecification;

            public int Id { get; set; }
            public string Name { get; set; }

            public int CustomerActionConditionTypeId { get; set; }

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

            [BsonIgnoreExtraElements]
            public partial class ProductAttributeValue
            {
                public int Id { get; set; }
                public int ProductAttributeId { get; set; }
                public string Name { get; set; }
            }

            [BsonIgnoreExtraElements]
            public partial class ProductSpecification
            {
                public int Id { get; set; }
                public int ProductSpecyficationId { get; set; }
                public int ProductSpecyficationValueId { get; set; }
            }

        }

    }
}
