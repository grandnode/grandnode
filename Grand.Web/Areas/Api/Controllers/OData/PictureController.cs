using Grand.Api.DTOs.Common;
using Grand.Api.Interfaces;
using Grand.Services.Security;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
        public IActionResult Get(string key)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Files))
                return Forbid();

            var picture = _commonApiService.GetPictures().FirstOrDefault(x => x.Id == key);
            if (picture == null)
                return NotFound();

            return Ok(picture);
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            if (!_permissionService.Authorize(PermissionSystemName.Files))
                return Forbid();

            return Ok(_commonApiService.GetPictures());
        }

        [HttpPost]
        public IActionResult Post([FromBody] PictureDto model)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Files))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = _commonApiService.InsertPicture(model);
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [HttpDelete]
        public IActionResult Delete(string key)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Files))
                return Forbid();

            var picture = _commonApiService.GetPictures().FirstOrDefault(x => x.Id == key);
            if (picture == null)
            {
                return NotFound();
            }
            _commonApiService.DeletePicture(picture);
            return Ok();
        }
    }
}
