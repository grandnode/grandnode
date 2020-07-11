using Grand.Domain.Stores;
using System;
using System.Collections.Generic;

namespace Grand.Domain.Discounts
{
    /// <summary>
    /// Represents a discount
    /// </summary>
    public partial class Discount : BaseEntity, IStoreMappingSupported
    {
        private ICollection<DiscountRequirement> _discountRequirements;

        public Discount()
        {
            Stores = new List<string>();
        }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the discount type identifier
        /// </summary>
        public int DiscountTypeId { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to use percentage
        /// </summary>
        public bool UsePercentage { get; set; }

        /// <summary>
        /// Gets or sets the discount percentage
        /// </summary>
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Gets or sets the discount amount
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to calculate by plugin
        /// </summary>
        public bool CalculateByPlugin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to calculate by plugin
        /// </summary>
        public string DiscountPluginName { get; set; }

        /// <summary>
        /// Gets or sets the maximum discount amount (used with "UsePercentage")
        /// </summary>
        public decimal? MaximumDiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets the discount start date and time
        /// </summary>
        public DateTime? StartDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the discount end date and time
        /// </summary>
        public DateTime? EndDateUtc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether discount requires coupon code
        /// </summary>
        public bool RequiresCouponCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether coupon code can be reused
        /// </summary>
        public bool Reused { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether discount can be used simultaneously with other discounts (with the same discount type)
        /// </summary>
        public bool IsCumulative { get; set; }

        /// <summary>
        /// Gets or sets the discount limitation identifier
        /// </summary>
        public int DiscountLimitationId { get; set; }

        /// <summary>
        /// Gets or sets the discount limitation times (used when Limitation is set to "N Times Only" or "N Times Per Customer")
        /// </summary>
        public int LimitationTimes { get; set; }

        /// <summary>
        /// Gets or sets the maximum product quantity which could be discounted
        /// Used with "Assigned to products" or "Assigned to categories" type
        /// </summary>
        public int? MaximumDiscountedQuantity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether discount is enabled
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        public bool LimitedToStores { get; set; }

        public IList<string> Stores { get; set; }

        /// <summary>
        /// Gets or sets the discount type
        /// </summary>
        public DiscountType DiscountType
        {
            get
            {
                return (DiscountType)this.DiscountTypeId;
            }
            set
            {
                this.DiscountTypeId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the discount limitation
        /// </summary>
        public DiscountLimitationType DiscountLimitation
        {
            get
            {
                return (DiscountLimitationType)this.DiscountLimitationId;
            }
            set
            {
                this.DiscountLimitationId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the discount requirement
        /// </summary>
        public virtual ICollection<DiscountRequirement> DiscountRequirements
        {
            get { return _discountRequirements ?? (_discountRequirements = new List<DiscountRequirement>()); }
            protected set { _discountRequirements = value; }
        }

    }
}
