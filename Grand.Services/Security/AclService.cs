using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Security;
using Grand.Services.Events;
using MediatR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Security
{
    /// <summary>
    /// ACL service
    /// </summary>
    public partial class AclService : IAclService
    {
        #region Constants
        
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : entity ID
        /// {1} : entity name
        /// </remarks>
        private const string ACLRECORD_BY_ENTITYID_NAME_KEY = "Grand.aclrecord.entityid-name-{0}-{1}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string ACLRECORD_PATTERN_KEY = "Grand.aclrecord.";

        #endregion

        #region Fields

        private readonly IRepository<AclRecord> _aclRecordRepository;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;
        private readonly IMediator _mediator;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="workContext">Work context</param>
        /// <param name="aclRecordRepository">ACL record repository</param>
        /// <param name="catalogSettings">Catalog settings</param>
        /// <param name="eventPublisher">Event publisher</param>
        public AclService(ICacheManager cacheManager, 
            IWorkContext workContext,
            IRepository<AclRecord> aclRecordRepository,
            IMediator mediator,
            CatalogSettings catalogSettings)
        {
            this._cacheManager = cacheManager;
            this._workContext = workContext;
            this._aclRecordRepository = aclRecordRepository;
            this._mediator = mediator;
            this._catalogSettings = catalogSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes an ACL record
        /// </summary>
        /// <param name="aclRecord">ACL record</param>
        public virtual async Task DeleteAclRecord(AclRecord aclRecord)
        {
            if (aclRecord == null)
                throw new ArgumentNullException("aclRecord");

            await _aclRecordRepository.DeleteAsync(aclRecord);

            //cache
            await _cacheManager.RemoveByPattern(ACLRECORD_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(aclRecord);
        }

        /// <summary>
        /// Gets an ACL record
        /// </summary>
        /// <param name="aclRecordId">ACL record identifier</param>
        /// <returns>ACL record</returns>
        public virtual Task<AclRecord> GetAclRecordById(string aclRecordId)
        {
            return _aclRecordRepository.GetByIdAsync(aclRecordId);
        }

        
        /// <summary>
        /// Inserts an ACL record
        /// </summary>
        /// <param name="aclRecord">ACL record</param>
        public virtual async Task InsertAclRecord(AclRecord aclRecord)
        {
            if (aclRecord == null)
                throw new ArgumentNullException("aclRecord");

            await _aclRecordRepository.InsertAsync(aclRecord);

            //cache
            await _cacheManager.RemoveByPattern(ACLRECORD_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(aclRecord);
        }

        /// <summary>
        /// Updates the ACL record
        /// </summary>
        /// <param name="aclRecord">ACL record</param>
        public virtual async Task UpdateAclRecord(AclRecord aclRecord)
        {
            if (aclRecord == null)
                throw new ArgumentNullException("aclRecord");

            await _aclRecordRepository.UpdateAsync(aclRecord);

            //cache
            await _cacheManager.RemoveByPattern(ACLRECORD_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(aclRecord);
        }

        /// <summary>
        /// Authorize ACL permission
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize<T>(T entity) where T : BaseEntity, IAclSupported
        {
            return Authorize(entity, _workContext.CurrentCustomer);
        }

        /// <summary>
        /// Authorize ACL permission
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <param name="customer">Customer</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize<T>(T entity, Customer customer) where T : BaseEntity, IAclSupported
        {
            if (entity == null)
                return false;

            if (customer == null)
                return false;

            if (!entity.SubjectToAcl)
                return true;

            if (_catalogSettings.IgnoreAcl)
                return true;

            foreach (var role1 in customer.CustomerRoles.Where(cr => cr.Active))
                foreach (var role2Id in entity.CustomerRoles)
                    if (role1.Id == role2Id)
                        //yes, we have such permission
                        return true;

            //no permission found
            return false;
        }
        #endregion
    }
}