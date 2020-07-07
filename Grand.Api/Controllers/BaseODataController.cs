using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("odata/[controller]")]
    [ApiExplorerSettings(IgnoreApi = false)]
    public abstract partial class BaseODataController : ODataController
    {
        public override ForbidResult Forbid()
        {
            return new ForbidResult(JwtBearerDefaults.AuthenticationScheme);
        }
    }
}
