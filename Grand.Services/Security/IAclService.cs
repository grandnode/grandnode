using Grand.Domain;
using Grand.Domain.Customers;
using Grand.Domain.Security;
using System.Threading.Tasks;

namespace Grand.Services.Security
{
    /// <summary>
    /// ACL service inerface
    /// </summary>
    public partial interface IAclService
    {
        /// <summary>
        /// Deletes an ACL record
        /// </summary>
        /// <param name="aclRecord">ACL record</param>
        Task DeleteAclRecord(AclRecord aclRecord);

        /// <summary>
        /// Gets an ACL record
        /// </summary>
        /// <param name="aclRecordId">ACL record identifier</param>
        /// <returns>ACL record</returns>
        Task<AclRecord> GetAclRecordById(string aclRecordId);

        /// <summary>
        /// Inserts an ACL record
        /// </summary>
        /// <param name="aclRecord">ACL record</param>
        Task InsertAclRecord(AclRecord aclRecord);

        /// <summary>
        /// Updates the ACL record
        /// </summary>
        /// <param name="aclRecord">ACL record</param>
        Task UpdateAclRecord(AclRecord aclRecord);

        /// <summary>
        /// Authorize ACL permission
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <returns>true - authorized; otherwise, false</returns>
        bool Authorize<T>(T entity) where T : BaseEntity, IAclSupported;

        /// <summary>
        /// Authorize ACL permission
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <param name="customer">Customer</param>
        /// <returns>true - authorized; otherwise, false</returns>
        bool Authorize<T>(T entity, Customer customer) where T : BaseEntity, IAclSupported;
    }
}