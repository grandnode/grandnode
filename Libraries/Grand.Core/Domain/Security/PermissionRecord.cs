using System.Collections.Generic;
using Grand.Core.Domain.Customers;

namespace Grand.Core.Domain.Security
{
    /// <summary>
    /// Represents a permission record
    /// </summary>
    public partial class PermissionRecord : BaseEntity
    {
        private ICollection<string> _customerRoles;

        /// <summary>
        /// Gets or sets the permission name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the permission system name
        /// </summary>
        public string SystemName { get; set; }
        
        /// <summary>
        /// Gets or sets the permission category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets discount usage history
        /// </summary>
        public virtual ICollection<string> CustomerRoles
        {
            get { return _customerRoles ?? (_customerRoles = new List<string>()); }
            protected set { _customerRoles = value; }
        }
    }
}
