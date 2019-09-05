using Grand.Core.Domain.Courses;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Security.Authorization;
using Grand.Services.Courses;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Courses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Courses)]
    public partial class CourseController : BaseAdminController
    {

        private readonly ILocalizationService _localizationService;
        private readonly ICourseLevelService _courseLevelService;

        public CourseController(ILocalizationService localizationService, ICourseLevelService courseLevelService)
        {
            _localizationService = localizationService;
            _courseLevelService = courseLevelService;
        }


        #region Level

        public IActionResult Level() => View();

        [HttpPost]
        public async Task<IActionResult> Levels(DataSourceRequest command)
        {
            var levelModel = (await _courseLevelService.GetAll())
                .Select(x => x.ToModel());

            var gridModel = new DataSourceResult {
                Data = levelModel,
                Total = levelModel.Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> LevelUpdate(CourseLevelModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var level = await _courseLevelService.GetById(model.Id);
            level = model.ToEntity(level);
            await _courseLevelService.Update(level);

            return new NullJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> LevelAdd(CourseLevelModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var level = new CourseLevel();
            level = model.ToEntity(level);
            await _courseLevelService.Insert(level);

            return new NullJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> LevelDelete(string id)
        {
            var level = await _courseLevelService.GetById(id);
            if (level == null)
                throw new ArgumentException("No weight found with the specified id");

            await _courseLevelService.Delete(level);

            return new NullJsonResult();
        }

        #endregion

    }
}
