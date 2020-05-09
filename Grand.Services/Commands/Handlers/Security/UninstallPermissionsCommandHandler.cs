using Grand.Services.Commands.Models.Security;
using Grand.Services.Localization;
using Grand.Services.Security;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Security
{
    public class UninstallPermissionsCommandHandler : IRequestHandler<UninstallPermissionsCommand, bool>
    {
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;

        public UninstallPermissionsCommandHandler(
            IPermissionService permissionService,
            ILocalizationService localizationService,
            ILanguageService languageService)
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
            _languageService = languageService;
        }

        public async Task<bool> Handle(UninstallPermissionsCommand request, CancellationToken cancellationToken)
        {
            var permissions = request.PermissionProvider.GetPermissions();
            foreach (var permission in permissions)
            {
                var permission1 = await _permissionService.GetPermissionRecordBySystemName(permission.SystemName);
                if (permission1 != null)
                {
                    await _permissionService.DeletePermissionRecord(permission1);

                    //delete permission locales
                    await permission1.DeleteLocalizedPermissionName(_localizationService, _languageService);
                }
            }
            return true;
        }
    }
}
