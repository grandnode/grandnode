using System.Collections.Generic;

namespace Grand.Domain.Security
{
    /// <summary>
    /// Represents a permission record
    /// </summary>
    public partial class PermissionRecord : BaseEntity
    {
        private ICollection<string> _customerRoles;
        private ICollection<string> _actions;

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
        /// Gets or sets customer roles
        /// </summary>
        public virtual ICollection<string> CustomerRoles
        {
            get { return _customerRoles ?? (_customerRoles = new List<string>()); }
            protected set { _customerRoles = value; }
        }

        /// <summary>
        /// Gets or sets actions
        /// </summary>
        public virtual ICollection<string> Actions {
            get { return _actions ?? (_actions = new List<string>()); }
            set { _actions = value; }
        }
    }
}
