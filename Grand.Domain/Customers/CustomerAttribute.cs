using Grand.Domain.Catalog;
using Grand.Domain.Localization;
using System.Collections.Generic;

namespace Grand.Domain.Customers
{
    /// <summary>
    /// Represents a customer attribute
    /// </summary>
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
