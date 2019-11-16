using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Security;
using Grand.Services.Customers;
using Grand.Services.Localization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Security
{
    /// <summary>
    /// Permission service
    /// </summary>
    public partial class PermissionService : IPermissionService
    {
        #region Constants
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer role ID
        /// {1} : permission system name
        /// </remarks>
        private const string PERMISSIONS_ALLOWED_KEY = "Grand.permission.allowed-{0}-{1}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PERMISSIONS_PATTERN_KEY = "Grand.permission.";
        #endregion

        #region Fields

        private readonly IRepository<PermissionRecord> _permissionRecordRepository;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="permissionRecordRepository">Permission repository</param>
        /// <param name="customerService">Customer service</param>
        /// <param name="workContext">Work context</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="languageService">Language service</param>
        /// <param name="cacheManager">Cache manager</param>
        public PermissionService(IRepository<PermissionRecord> permissionRecordRepository,
            ICustomerService customerService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            ILanguageService languageService,
            IEnumerable<ICacheManager> cacheManager)
        {
            _permissionRecordRepository = permissionRecordRepository;
            _customerService = customerService;
            _workContext = workContext;
            _localizationService = localizationService;
            _languageService = languageService;
            _cacheManager = cacheManager.First(o => o.GetType() == typeof(MemoryCacheManager));
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Authorize permission
        /// </summary>
        /// <param name="permissionRecordSystemName">Permission record system name</param>
        /// <param name="customerRole">Customer role</param>
        /// <returns>true - authorized; otherwise, false</returns>
        protected virtual async Task<bool> Authorize(string permissionRecordSystemName, CustomerRole customerRole)
        {
            if (String.IsNullOrEmpty(permissionRecordSystemName))
                return false;

            string key = string.Format(PERMISSIONS_ALLOWED_KEY, customerRole.Id, permissionRecordSystemName);
            return await _cacheManager.GetAsync(key, async () =>
            {
                var permissionRecord = await _permissionRecordRepository.Table.Where(x => x.CustomerRoles.Contains(customerRole.Id) && x.SystemName== permissionRecordSystemName).ToListAsync();
                if (permissionRecord.Any())
                    return true;

                return false;
            });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete a permission
        /// </summary>
        /// <param name="permission">Permission</param>
        public virtual async Task DeletePermissionRecord(PermissionRecord permission)
        {
            if (permission == null)
                throw new ArgumentNullException("permission");

            await _permissionRecordRepository.DeleteAsync(permission);

            await _cacheManager.RemoveByPattern(PERMISSIONS_PATTERN_KEY);
        }

        /// <summary>
        /// Gets a permission
        /// </summary>
        /// <param name="permissionId">Permission identifier</param>
        /// <returns>Permission</returns>
        public virtual Task<PermissionRecord> GetPermissionRecordById(string permissionId)
        {
            return _permissionRecordRepository.GetByIdAsync(permissionId);
        }

        /// <summary>
        /// Gets a permission
        /// </summary>
        /// <param name="systemName">Permission system name</param>
        /// <returns>Permission</returns>
        public virtual async Task<PermissionRecord> GetPermissionRecordBySystemName(string systemName)
        {
            if (String.IsNullOrWhiteSpace(systemName))
                return await Task.FromResult<PermissionRecord>(null);

            var query = from pr in _permissionRecordRepository.Table
                        where  pr.SystemName == systemName
                        orderby pr.Id
                        select pr;

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets all permissions
        /// </summary>
        /// <returns>Permissions</returns>
        public virtual async Task<IList<PermissionRecord>> GetAllPermissionRecords()
        {
            var query = from pr in _permissionRecordRepository.Table
                        orderby pr.Name
                        select pr;
            return await query.ToListAsync();
        }

        /// <summary>
        /// Inserts a permission
        /// </summary>
        /// <param name="permission">Permission</param>
        public virtual async Task InsertPermissionRecord(PermissionRecord permission)
        {
            if (permission == null)
                throw new ArgumentNullException("permission");

            await _permissionRecordRepository.InsertAsync(permission);

            await _cacheManager.RemoveByPattern(PERMISSIONS_PATTERN_KEY);
        }

        /// <summary>
        /// Updates the permission
        /// </summary>
        /// <param name="permission">Permission</param>
        public virtual async Task UpdatePermissionRecord(PermissionRecord permission)
        {
            if (permission == null)
                throw new ArgumentNullException("permission");

            await _permissionRecordRepository.UpdateAsync(permission);

            await _cacheManager.RemoveByPattern(PERMISSIONS_PATTERN_KEY);
        }

        /// <summary>
        /// Install permissions
        /// </summary>
        /// <param name="permissionProvider">Permission provider</param>
        public virtual async Task InstallPermissions(IPermissionProvider permissionProvider)
        {
            //install new permissions
            var permissions = permissionProvider.GetPermissions();
            foreach (var permission in permissions)
            {
                var permission1 = await GetPermissionRecordBySystemName(permission.SystemName);
                if (permission1 == null)
                {
                    //new permission (install it)
                    permission1 = new PermissionRecord
                    {
                        Name = permission.Name,
                        SystemName = permission.SystemName,
                        Category = permission.Category,
                    };


                    //default customer role mappings
                    var defaultPermissions = permissionProvider.GetDefaultPermissions();
                    foreach (var defaultPermission in defaultPermissions)
                    {
                        var customerRole = await _customerService.GetCustomerRoleBySystemName(defaultPermission.CustomerRoleSystemName);
                        if (customerRole == null)
                        {
                            //new role (save it)
                            customerRole = new CustomerRole
                            {
                                Name = defaultPermission.CustomerRoleSystemName,
                                Active = true,
                                SystemName = defaultPermission.CustomerRoleSystemName
                            };
                            await _customerService.InsertCustomerRole(customerRole);
                        }


                        var defaultMappingProvided = (from p in defaultPermission.PermissionRecords
                                                      where p.SystemName == permission1.SystemName
                                                      select p).Any();
                        if (defaultMappingProvided)
                        {
                            permission1.CustomerRoles.Add(customerRole.Id);
                        }
                    }

                    //save new permission
                    await InsertPermissionRecord(permission1);

                    //save localization
                    await permission1.SaveLocalizedPermissionName(_localizationService, _languageService);
                }
            }
        }

        /// <summary>
        /// Install missing permissions
        /// </summary>
        /// <param name="permissionProvider">Permission provider</param>
        public virtual async Task InstallNewPermissions(IPermissionProvider permissionProvider)
        {
            //install new permissions
            var permissions = permissionProvider.GetPermissions();
            foreach (var permission in permissions)
            {
                var permission1 = await GetPermissionRecordBySystemName(permission.SystemName);
                if (permission1 == null)
                {
                    //new permission (install it)
                    permission1 = new PermissionRecord {
                        Name = permission.Name,
                        SystemName = permission.SystemName,
                        Category = permission.Category,
                    };

                    //save new permission
                    await InsertPermissionRecord(permission1);

                    //save localization
                    await permission1.SaveLocalizedPermissionName(_localizationService, _languageService);
                }
            }
        }

        /// <summary>
        /// Uninstall permissions
        /// </summary>
        /// <param name="permissionProvider">Permission provider</param>
        public virtual async Task UninstallPermissions(IPermissionProvider permissionProvider)
        {
            var permissions = permissionProvider.GetPermissions();
            foreach (var permission in permissions)
            {
                var permission1 = await GetPermissionRecordBySystemName(permission.SystemName);
                if (permission1 != null)
                {
                    await DeletePermissionRecord(permission1);

                    //delete permission locales
                    await permission1.DeleteLocalizedPermissionName(_localizationService, _languageService);
                }
            }

        }
        
        /// <summary>
        /// Authorize permission
        /// </summary>
        /// <param name="permission">Permission record</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual async Task<bool> Authorize(PermissionRecord permission)
        {
            return await Authorize(permission, _workContext.CurrentCustomer);
        }

        /// <summary>
        /// Authorize permission
        /// </summary>
        /// <param name="permission">Permission record</param>
        /// <param name="customer">Customer</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual async Task<bool> Authorize(PermissionRecord permission, Customer customer)
        {
            if (permission == null)
                return false;

            if (customer == null)
                return false;
           
            return await Authorize(permission.SystemName, customer);
        }

        /// <summary>
        /// Authorize permission
        /// </summary>
        /// <param name="permissionRecordSystemName">Permission record system name</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual async Task<bool> Authorize(string permissionRecordSystemName)
        {
            return await Authorize(permissionRecordSystemName, _workContext.CurrentCustomer);
        }

        /// <summary>
        /// Authorize permission
        /// </summary>
        /// <param name="permissionRecordSystemName">Permission record system name</param>
        /// <param name="customer">Customer</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual async Task<bool> Authorize(string permissionRecordSystemName, Customer customer)
        {
            if (String.IsNullOrEmpty(permissionRecordSystemName))
                return false;

            var customerRoles = customer.CustomerRoles.Where(cr => cr.Active);            
            foreach (var role in customerRoles)
                if (await Authorize(permissionRecordSystemName, role))
                    //yes, we have such permission
                    return true;

            //no permission found
            return false;
        }

        #endregion
    }
}