using Grand.Domain.Security;
using Grand.Services.Commands.Models.Security;
using Grand.Services.Localization;
using Grand.Services.Security;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Security
{
    public class InstallNewPermissionsCommandHandler : IRequestHandler<InstallNewPermissionsCommand, bool>
    {
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;

        public InstallNewPermissionsCommandHandler(
            IPermissionService permissionService,
            ILocalizationService localizationService,
            ILanguageService languageService)
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
            _languageService = languageService;
        }

        public async Task<bool> Handle(InstallNewPermissionsCommand request, CancellationToken cancellationToken)
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
