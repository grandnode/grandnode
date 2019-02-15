using Grand.Api.Interfaces;
using Grand.Services.Security;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Grand.Web.Areas.Api.Controllers.OData
{
    public partial class CurrencyController : BaseODataController
    {
        private readonly ICommonApiService _commonApiService;
        private readonly IPermissionService _permissionService;

        public CurrencyController(ICommonApiService commonApiService, IPermissionService permissionService)
        {
            _commonApiService = commonApiService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public IActionResult Get(string key)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Currencies))
                return Forbid();

            var currency = _commonApiService.GetCurrencies().FirstOrDefault(x => x.Id == key);
            if (currency == null)
                return NotFound();

            return Ok(currency);
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            if (!_permissionService.Authorize(PermissionSystemName.Currencies))
                return Forbid();

            return Ok(_commonApiService.GetCurrencies());
        }
    }
}
