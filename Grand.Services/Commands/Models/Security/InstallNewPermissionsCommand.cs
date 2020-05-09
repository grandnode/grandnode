using Grand.Services.Security;
using MediatR;

namespace Grand.Services.Commands.Models.Security
{
    public class InstallNewPermissionsCommand : IRequest<bool>
    {
        public IPermissionProvider PermissionProvider { get; set; }
    }
}
