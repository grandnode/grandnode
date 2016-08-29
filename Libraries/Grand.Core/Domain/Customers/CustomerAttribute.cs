using System.Collections.Generic;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Localization;
using MongoDB.Bson.Serialization.Attributes;

namespace Grand.Core.Domain.Customers
{
    /// <summary>
    /// Represents a customer attribute
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class CustomerAttribute : BaseEntity, ILocalizedEntity
    {
        private ICollection<CustomerAttributeValue> _customerAttributeValues;

        public CustomerAttribute()
        {
            Locales = new List<LocalizedProperty>();
        }
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the attribute is required
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the attribute control type identifier
        /// </summary>
        public int AttributeControlTypeId { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the collection of locales
        /// </summary>
        public IList<LocalizedProperty> Locales { get; set; }


        /// <summary>
        /// Gets the attribute control type
        /// </summary>
        [BsonIgnoreAttribute]
        public AttributeControlType AttributeControlType
        {
            get
            {
                return (AttributeControlType)this.AttributeControlTypeId;
            }
            set
            {
                this.AttributeControlTypeId = (int)value;
            }
        }
        /// <summary>
        /// Gets the customer attribute values
        /// </summary>
        public virtual ICollection<CustomerAttributeValue> CustomerAttributeValues
        {
            get { return _customerAttributeValues ?? (_customerAttributeValues = new List<CustomerAttributeValue>()); }
            protected set { _customerAttributeValues = value; }
        }
    }

}
