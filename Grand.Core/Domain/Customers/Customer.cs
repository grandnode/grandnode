using System;
using System.Collections.Generic;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Orders;

namespace Grand.Core.Domain.Customers
{
    /// <summary>
    /// Represents a customer
    /// </summary>
    public partial class Customer : BaseEntity
    {
        private ICollection<CustomerRole> _customerRoles;
        private ICollection<ShoppingCartItem> _shoppingCartItems;
        private ICollection<Address> _addresses;
        private ICollection<string> _customerTags;

        /// <summary>
        /// Ctor
        /// </summary>
        public Customer()
        {
            this.CustomerGuid = Guid.NewGuid();
            this.PasswordFormat = PasswordFormat.Clear;
        }

        /// <summary>
        /// Gets or sets the customer Guid
        /// </summary>
        public Guid CustomerGuid { get; set; }

        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Gets or sets the email
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Gets or sets the password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the password format
        /// </summary>
        public int PasswordFormatId { get; set; }
        /// <summary>
        /// Gets or sets the password format
        /// </summary>
        public PasswordFormat PasswordFormat
        {
            get { return (PasswordFormat)PasswordFormatId; }
            set { this.PasswordFormatId = (int)value; }
        }
        /// <summary>
        /// Gets or sets the password salt
        /// </summary>
        public string PasswordSalt { get; set; }

        /// <summary>
        /// Gets or sets the admin comment
        /// </summary>
        public string AdminComment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer is tax exempt
        /// </summary>
        public bool IsTaxExempt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer has a free shipping to the next a order
        /// </summary>
        public bool FreeShipping { get; set; }

        /// <summary>
        /// Gets or sets the affiliate identifier
        /// </summary>
        public string AffiliateId { get; set; }

        /// <summary>
        /// Gets or sets the vendor identifier with which this customer is associated (maganer)
        /// </summary>
        public string VendorId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this customer has some products in the shopping cart
        /// <remarks>The same as if we run this.ShoppingCartItems.Count > 0
        /// We use this property for performance optimization:
        /// if this property is set to false, then we do not need to load "ShoppingCartItems" navigation property for each page load
        /// It's used only in a couple of places in the presenation layer
        /// </remarks>
        /// </summary>
        public bool HasShoppingCartItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer is active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer has been deleted
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer account is system
        /// </summary>
        public bool IsSystemAccount { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the customer add news
        /// </summary>
        public bool IsNewsItem { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the customer has a orders
        /// </summary>
        public bool IsHasOrders { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer has a blog comments
        /// </summary>
        public bool IsHasBlogComments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer has a product review
        /// </summary>
        public bool IsHasProductReview { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the customer has a product review help
        /// </summary>
        public bool IsHasProductReviewH { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer has a Vendor review
        /// </summary>
        public bool IsHasVendorReview { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer has a Vendor review help
        /// </summary>
        public bool IsHasVendorReviewH { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer has a pool voting
        /// </summary>
        public bool IsHasPoolVoting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer has a forum post
        /// </summary>
        public bool IsHasForumPost { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer has a forum topic
        /// </summary>
        public bool IsHasForumTopic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating number of failed login attempts (wrong password)
        /// </summary>
        public int FailedLoginAttempts { get; set; }

        /// <summary>
        /// Gets or sets the date and time until which a customer cannot login (locked out)
        /// </summary>
        public DateTime? CannotLoginUntilDateUtc { get; set; }
        /// <summary>
        /// Gets or sets the customer system name
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// Gets or sets the last IP address
        /// </summary>
        public string LastIpAddress { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last login
        /// </summary>
        public DateTime? LastLoginDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last activity
        /// </summary>
        public DateTime LastActivityDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last purchase
        /// </summary>
        public DateTime? LastPurchaseDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last update cart
        /// </summary>
        public DateTime? LastUpdateCartDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last update wishlist
        /// </summary>
        public DateTime? LastUpdateWishListDateUtc { get; set; }

        /// <summary>
        /// Last date to change password
        /// </summary>
        public DateTime? PasswordChangeDateUtc { get; set; }

        #region Navigation properties

        /// <summary>
        /// Gets or sets the customer roles
        /// </summary>
        public virtual ICollection<CustomerRole> CustomerRoles
        {
            get { return _customerRoles ?? (_customerRoles = new List<CustomerRole>()); }
            protected set { _customerRoles = value; }
        }

        /// <summary>
        /// Gets or sets shopping cart items
        /// </summary>
        public virtual ICollection<ShoppingCartItem> ShoppingCartItems
        {
            get { return _shoppingCartItems ?? (_shoppingCartItems = new List<ShoppingCartItem>()); }
            protected set { _shoppingCartItems = value; }            
        }

        /// <summary>
        /// Default billing address
        /// </summary>
        public virtual Address BillingAddress { get; set; }

        /// <summary>
        /// Default shipping address
        /// </summary>
        public virtual Address ShippingAddress { get; set; }

        /// <summary>
        /// Gets or sets customer addresses
        /// </summary>
        public virtual ICollection<Address> Addresses
        {
            get { return _addresses ?? (_addresses = new List<Address>()); }
            protected set { _addresses = value; }            
        }

        /// <summary>
        /// Gets or sets the customer tags
        /// </summary>
        public virtual ICollection<string> CustomerTags
        {
            get { return _customerTags ?? (_customerTags = new List<string>()); }
            protected set { _customerTags = value; }
        }
        #endregion
    }
}