using Grand.Api.Controllers;
using Grand.Api.DTOs.Common;
using Grand.Api.Interfaces;
using Grand.Services.Security;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Api.Controllers.OData
{
    public partial class PictureController : BaseODataController
    {
        private readonly ICommonApiService _commonApiService;
        private readonly IPermissionService _permissionService;
        public PictureController(ICommonApiService commonApiService, IPermissionService permissionService)
        {
            _commonApiService = commonApiService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Files))
                return Forbid();

            var picture = _commonApiService.GetPictures().FirstOrDefault(x => x.Id == key);
            if (picture == null)
                return NotFound();

            return Ok(picture);
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Files))
                return Forbid();

            return Ok(_commonApiService.GetPictures());
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PictureDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Files))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _commonApiService.InsertPicture(model);
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Files))
                return Forbid();

            var picture = _commonApiService.GetPictures().FirstOrDefault(x => x.Id == key);
            if (picture == null)
            {
                return NotFound();
            }
            await _commonApiService.DeletePicture(picture);
            return Ok();
        }
    }
}
