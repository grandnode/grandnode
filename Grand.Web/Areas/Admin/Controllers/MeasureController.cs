using Grand.Domain.Directory;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Security.Authorization;
using Grand.Services.Configuration;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Directory;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Measures)]
    public partial class MeasureController : BaseAdminController
    {
        #region Fields

        private readonly IMeasureService _measureService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly MeasureSettings _measureSettings;

        #endregion

        #region Constructors

        public MeasureController(IMeasureService measureService,
            ISettingService settingService,
            ILocalizationService localizationService,
            MeasureSettings measureSettings)
        {
            _measureService = measureService;
            _settingService = settingService;
            _localizationService = localizationService;
            _measureSettings = measureSettings;
        }

        #endregion

        #region Methods

        #region Weights

        public IActionResult Weights() => View();

        [HttpPost]
        public async Task<IActionResult> Weights(DataSourceRequest command)
        {
            var weightsModel = (await _measureService.GetAllMeasureWeights())
                .Select(x => x.ToModel())
                .ToList();
            foreach (var wm in weightsModel)
                wm.IsPrimaryWeight = wm.Id == _measureSettings.BaseWeightId;
            var gridModel = new DataSourceResult
            {
                Data = weightsModel,
                Total = weightsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> WeightUpdate(MeasureWeightModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var weight = await _measureService.GetMeasureWeightById(model.Id);
            weight = model.ToEntity(weight);
            await _measureService.UpdateMeasureWeight(weight);

            return new NullJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> WeightAdd(MeasureWeightModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var weight = new MeasureWeight();
            weight = model.ToEntity(weight);
            await _measureService.InsertMeasureWeight(weight);

            return new NullJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> WeightDelete(string id)
        {
            var weight = await _measureService.GetMeasureWeightById(id);
            if (weight == null)
                throw new ArgumentException("No weight found with the specified id");

            if (weight.Id == _measureSettings.BaseWeightId)
            {
                return Json(new DataSourceResult { Errors = _localizationService.GetResource("Admin.Configuration.Measures.Weights.CantDeletePrimary") });
            }

            await _measureService.DeleteMeasureWeight(weight);

            return new NullJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsPrimaryWeight(string id)
        {
            var primaryWeight = await _measureService.GetMeasureWeightById(id);
            if (primaryWeight != null)
            {
                _measureSettings.BaseWeightId = primaryWeight.Id;
                await _settingService.SaveSetting(_measureSettings);
            }

            return Json(new { result = true });
        }

        #endregion

        #region Dimensions

        public IActionResult Dimensions() => View();

        [HttpPost]
        public async Task<IActionResult> Dimensions(DataSourceRequest command)
        {
            var dimensionsModel = (await _measureService.GetAllMeasureDimensions())
                .Select(x => x.ToModel())
                .ToList();
            foreach (var wm in dimensionsModel)
                wm.IsPrimaryDimension = wm.Id == _measureSettings.BaseDimensionId;
            var gridModel = new DataSourceResult
            {
                Data = dimensionsModel,
                Total = dimensionsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> DimensionUpdate(MeasureDimensionModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var dimension = await _measureService.GetMeasureDimensionById(model.Id);
            dimension = model.ToEntity(dimension);
            await _measureService.UpdateMeasureDimension(dimension);

            return new NullJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> DimensionAdd(MeasureDimensionModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var dimension = new MeasureDimension();
            dimension = model.ToEntity(dimension);
            await _measureService.InsertMeasureDimension(dimension);

            return new NullJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> DimensionDelete(string id)
        {
            var dimension = await _measureService.GetMeasureDimensionById(id);
            if (dimension == null)
                throw new ArgumentException("No dimension found with the specified id");

            if (dimension.Id == _measureSettings.BaseDimensionId)
            {
                return Json(new DataSourceResult { Errors = _localizationService.GetResource("Admin.Configuration.Measures.Dimensions.CantDeletePrimary") });
            }

            await _measureService.DeleteMeasureDimension(dimension);

            return new NullJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsPrimaryDimension(string id)
        {
            var primaryDimension = await _measureService.GetMeasureDimensionById(id);
            if (primaryDimension != null)
            {
                _measureSettings.BaseDimensionId = id;
                await _settingService.SaveSetting(_measureSettings);
            }
            return Json(new { result = true });
        }
        #endregion


        #region Units

        public IActionResult Units() => View();

        [HttpPost]
        public async Task<IActionResult> Units(DataSourceRequest command)
        {
            var unitsModel = (await _measureService.GetAllMeasureUnits())
                .Select(x => x.ToModel())
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = unitsModel,
                Total = unitsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> UnitUpdate(MeasureUnitModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var unit = await _measureService.GetMeasureUnitById(model.Id);
            unit = model.ToEntity(unit);
            await _measureService.UpdateMeasureUnit(unit);

            return new NullJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> UnitAdd(MeasureUnitModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var unit = new MeasureUnit();
            unit = model.ToEntity(unit);
            await _measureService.InsertMeasureUnit(unit);

            return new NullJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> UnitDelete(string id)
        {
            var unit = await _measureService.GetMeasureUnitById(id);
            if (unit == null)
                throw new ArgumentException("No unit found with the specified id");

            await _measureService.DeleteMeasureUnit(unit);

            return new NullJsonResult();
        }

        #endregion

        #endregion
    }
}
