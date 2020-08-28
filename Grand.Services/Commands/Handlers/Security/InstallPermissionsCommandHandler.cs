using Grand.Domain.Customers;
using Grand.Domain.Security;
using Grand.Services.Commands.Models.Security;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Security;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace Grand.Services.Commands.Handlers.Security
{
    public class InstallPermissionsCommandHandler : IRequestHandler<InstallPermissionsCommand, bool>
    {
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;

        public InstallPermissionsCommandHandler(
            IPermissionService permissionService,
            ICustomerService customerService,
            ILocalizationService localizationService,
            ILanguageService languageService)
        {
            _permissionService = permissionService;
            _customerService = customerService;
            _localizationService = localizationService;
            _languageService = languageService;
        }

        public async Task<bool> Handle(InstallPermissionsCommand request, CancellationToken cancellationToken)
        {
            //install new permissions
            var permissions = request.PermissionProvider.GetPermissions();
            foreach (var permission in permissions)
            {
                var permission1 = await _permissionService.GetPermissionRecordBySystemName(permission.SystemName);
                if (permission1 == null)
                {
                    //new permission (install it)
                    permission1 = new PermissionRecord {
                        Name = permission.Name,
                        SystemName = permission.SystemName,
                        Category = permission.Category,
                        Actions = permission.Actions
                    };


                    //default customer role mappings
                    var defaultPermissions = request.PermissionProvider.GetDefaultPermissions();
                    foreach (var defaultPermission in defaultPermissions)
                    {
                        var customerRole = await _customerService.GetCustomerRoleBySystemName(defaultPermission.CustomerRoleSystemName);
                        if (customerRole == null)
                        {
                            //new role (save it)
                            customerRole = new CustomerRole {
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
                    await _permissionService.InsertPermissionRecord(permission1);

                    //save localization
                    await permission1.SaveLocalizedPermissionName(_localizationService, _languageService);
                }
            }
            return true;
        }
    }
}
