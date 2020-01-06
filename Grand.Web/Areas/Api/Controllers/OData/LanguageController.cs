using Grand.Api.Controllers;
using Grand.Api.Interfaces;
using Grand.Services.Security;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Api.Controllers.OData
{
    public partial class LanguageController : BaseODataController
    {
        private readonly ICommonApiService _commonApiService;
        private readonly IPermissionService _permissionService;

        public LanguageController(ICommonApiService commonApiService, IPermissionService permissionService)
        {
            _commonApiService = commonApiService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Languages))
                return Forbid();

            var language = _commonApiService.GetLanguages().FirstOrDefault(x => x.Id == key);
            if (language == null)
                return NotFound();

            return Ok(language);
        }

        [HttpGet]
        [EnableQuery(HandleNullPropagation = HandleNullPropagationOption.False)]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Languages))
                return Forbid();

            return Ok(_commonApiService.GetLanguages());
        }
    }
}
