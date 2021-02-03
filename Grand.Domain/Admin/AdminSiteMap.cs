using System.Collections.Generic;

namespace Grand.Domain.Admin
{
    /// <summary>
    /// Represents an admin menu
    /// </summary>
    public partial class AdminSiteMap : BaseEntity
    {
        public AdminSiteMap()
        {
            ChildNodes = new List<AdminSiteMap>();
            PermissionNames = new List<string>();
        }

        /// <summary>
        /// Gets or sets the system name.
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// Gets or sets the resource name.
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the name of the controller.
        /// </summary>
        public string ControllerName { get; set; }

        /// <summary>
        /// Gets or sets the name of the action.
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the child nodes.
        /// </summary>
        public virtual IList<AdminSiteMap> ChildNodes { get; set; }

        /// <summary>
        /// Gets or sets the icon class 
        /// </summary>
        public string IconClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to open url in new tab (window) or not
        /// </summary>
        public bool OpenUrlInNewTab { get; set; }

        /// <summary>
        /// Gets or sets permissions
        /// </summary>
        public IList<string> PermissionNames { get; set; }

        /// <summary>
        /// Gets or sets all permissions
        /// </summary>
        public bool AllPermissions { get; set; }
    }
}
