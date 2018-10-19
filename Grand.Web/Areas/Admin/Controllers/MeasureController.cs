using Grand.Core.Domain.Directory;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Services.Configuration;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Directory;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class MeasureController : BaseAdminController
	{
		#region Fields

        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;

		#endregion

		#region Constructors

        public MeasureController(IMeasureService measureService,
            MeasureSettings measureSettings, ISettingService settingService,
            IPermissionService permissionService, ILocalizationService localizationService)
		{
            this._measureService = measureService;
            this._measureSettings = measureSettings;
            this._settingService = settingService;
            this._permissionService = permissionService;
            this._localizationService = localizationService;
		}

		#endregion 

		#region Methods
        
        #region Weights

        public IActionResult Weights()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            return View();
		}

		[HttpPost]
        public IActionResult Weights(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            var weightsModel = _measureService.GetAllMeasureWeights()
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
        public IActionResult WeightUpdate(MeasureWeightModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();
            
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var weight = _measureService.GetMeasureWeightById(model.Id);
            weight = model.ToEntity(weight);
            _measureService.UpdateMeasureWeight(weight);

            return new NullJsonResult();
        }
        
        [HttpPost]
        public IActionResult WeightAdd( MeasureWeightModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult {Errors = ModelState.SerializeErrors()});
            }

            var weight = new MeasureWeight();
            weight = model.ToEntity(weight);
            _measureService.InsertMeasureWeight(weight);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult WeightDelete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            var weight = _measureService.GetMeasureWeightById(id);
            if (weight == null)
                throw new ArgumentException("No weight found with the specified id");

            if (weight.Id == _measureSettings.BaseWeightId)
            {
                return Json(new DataSourceResult { Errors = _localizationService.GetResource("Admin.Configuration.Measures.Weights.CantDeletePrimary") });
            }

            _measureService.DeleteMeasureWeight(weight);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult MarkAsPrimaryWeight(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            var primaryWeight = _measureService.GetMeasureWeightById(id);
            if (primaryWeight != null)
            {
                _measureSettings.BaseWeightId = primaryWeight.Id;
                _settingService.SaveSetting(_measureSettings);
            }

            return Json(new { result = true });
        }

        #endregion

        #region Dimensions

        public IActionResult Dimensions()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public IActionResult Dimensions(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            var dimensionsModel = _measureService.GetAllMeasureDimensions()
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
        public IActionResult DimensionUpdate(MeasureDimensionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var dimension = _measureService.GetMeasureDimensionById(model.Id);
            dimension = model.ToEntity(dimension);
            _measureService.UpdateMeasureDimension(dimension);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult DimensionAdd( MeasureDimensionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var dimension = new MeasureDimension();
            dimension = model.ToEntity(dimension);
            _measureService.InsertMeasureDimension(dimension);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult DimensionDelete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            var dimension = _measureService.GetMeasureDimensionById(id);
            if (dimension == null)
                throw new ArgumentException("No dimension found with the specified id");

            if (dimension.Id == _measureSettings.BaseDimensionId)
            {
                return Json(new DataSourceResult { Errors = _localizationService.GetResource("Admin.Configuration.Measures.Dimensions.CantDeletePrimary") });
            }

            _measureService.DeleteMeasureDimension(dimension);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult MarkAsPrimaryDimension(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            var primaryDimension = _measureService.GetMeasureDimensionById(id);
            if (primaryDimension != null)
            {
                _measureSettings.BaseDimensionId = id;
                _settingService.SaveSetting(_measureSettings);
            }
            return Json(new { result = true });
        }
        #endregion


        #region Units


        public IActionResult Units()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public IActionResult Units(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            var unitsModel = _measureService.GetAllMeasureUnits()
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
        public IActionResult UnitUpdate(MeasureUnitModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var unit = _measureService.GetMeasureUnitById(model.Id);
            unit = model.ToEntity(unit);
            _measureService.UpdateMeasureUnit(unit);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult UnitAdd( MeasureUnitModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var unit = new MeasureUnit();
            unit = model.ToEntity(unit);
            _measureService.InsertMeasureUnit(unit);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult UnitDelete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            var unit = _measureService.GetMeasureUnitById(id);
            if (unit == null)
                throw new ArgumentException("No unit found with the specified id");

            _measureService.DeleteMeasureUnit(unit);

            return new NullJsonResult();
        }

        #endregion

        #endregion
    }
}
