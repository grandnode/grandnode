namespace Grand.Domain.Security
{
    /// <summary>
    /// Represents permission for action denied record 
    /// </summary>
    public partial class PermissionAction : BaseEntity
    {

        /// <summary>
        /// Gets or sets the permission system name
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// Gets or sets the customer role ident
        /// </summary>
        public string CustomerRoleId { get; set; }

        /// <summary>
        /// Gets or sets the action name for denied access
        /// </summary>
        public string Action { get; set; }

    }
}
