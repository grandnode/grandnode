using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Topics;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Security.Authorization;
using Grand.Services.Catalog;
using Grand.Services.Security;
using Grand.Services.Topics;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Templates;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Maintenance)]
    public partial class TemplateController : BaseAdminController
    {
        #region Fields

        private readonly ICategoryTemplateService _categoryTemplateService;
        private readonly IManufacturerTemplateService _manufacturerTemplateService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly ITopicTemplateService _topicTemplateService;

        #endregion

        #region Constructors
        public TemplateController(ICategoryTemplateService categoryTemplateService,
            IManufacturerTemplateService manufacturerTemplateService,
            IProductTemplateService productTemplateService,
            ITopicTemplateService topicTemplateService)
        {
            this._categoryTemplateService = categoryTemplateService;
            this._manufacturerTemplateService = manufacturerTemplateService;
            this._productTemplateService = productTemplateService;
            this._topicTemplateService = topicTemplateService;
        }

        #endregion

        #region Category templates

        public IActionResult CategoryTemplates() => View();

        [HttpPost]
        public IActionResult CategoryTemplates(DataSourceRequest command)
        {
            var templatesModel = _categoryTemplateService.GetAllCategoryTemplates()
                .Select(x => x.ToModel())
                .ToList();
            var gridModel = new DataSourceResult
            {
                Data = templatesModel,
                Total = templatesModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult CategoryTemplateUpdate(CategoryTemplateModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var template = _categoryTemplateService.GetCategoryTemplateById(model.Id);
            if (template == null)
                throw new ArgumentException("No template found with the specified id");
            if (ModelState.IsValid)
            {
                template = model.ToEntity(template);
                _categoryTemplateService.UpdateCategoryTemplate(template);

                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public IActionResult CategoryTemplateAdd(CategoryTemplateModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }
            if (ModelState.IsValid)
            {
                var template = new CategoryTemplate();
                template = model.ToEntity(template);
                _categoryTemplateService.InsertCategoryTemplate(template);

                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public IActionResult CategoryTemplateDelete(string id)
        {
            var template = _categoryTemplateService.GetCategoryTemplateById(id);
            if (template == null)
                throw new ArgumentException("No template found with the specified id");
            if (ModelState.IsValid)
            {
                _categoryTemplateService.DeleteCategoryTemplate(template);

                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Manufacturer templates
        public IActionResult ManufacturerTemplates() => View();

        [HttpPost]
        public IActionResult ManufacturerTemplates(DataSourceRequest command)
        {
            var templatesModel = _manufacturerTemplateService.GetAllManufacturerTemplates()
                .Select(x => x.ToModel())
                .ToList();
            var gridModel = new DataSourceResult
            {
                Data = templatesModel,
                Total = templatesModel.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ManufacturerTemplateUpdate(ManufacturerTemplateModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var template = _manufacturerTemplateService.GetManufacturerTemplateById(model.Id);
            if (template == null)
                throw new ArgumentException("No template found with the specified id");
            if (ModelState.IsValid)
            {
                template = model.ToEntity(template);
                _manufacturerTemplateService.UpdateManufacturerTemplate(template);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public IActionResult ManufacturerTemplateAdd(ManufacturerTemplateModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }
            if (ModelState.IsValid)
            {
                var template = new ManufacturerTemplate();
                template = model.ToEntity(template);
                _manufacturerTemplateService.InsertManufacturerTemplate(template);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public IActionResult ManufacturerTemplateDelete(string id)
        {
            var template = _manufacturerTemplateService.GetManufacturerTemplateById(id);
            if (template == null)
                throw new ArgumentException("No template found with the specified id");
            if (ModelState.IsValid)
            {
                _manufacturerTemplateService.DeleteManufacturerTemplate(template);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Product templates

        public IActionResult ProductTemplates() => View();

        [HttpPost]
        public IActionResult ProductTemplates(DataSourceRequest command)
        {
            var templatesModel = _productTemplateService.GetAllProductTemplates()
                .Select(x => x.ToModel())
                .ToList();
            var gridModel = new DataSourceResult
            {
                Data = templatesModel,
                Total = templatesModel.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductTemplateUpdate(ProductTemplateModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }
            var template = _productTemplateService.GetProductTemplateById(model.Id);
            if (template == null)
                throw new ArgumentException("No template found with the specified id");
            if (ModelState.IsValid)
            {
                template = model.ToEntity(template);
                _productTemplateService.UpdateProductTemplate(template);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public IActionResult ProductTemplateAdd(ProductTemplateModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }
            if (ModelState.IsValid)
            {
                var template = new ProductTemplate();
                template = model.ToEntity(template);
                _productTemplateService.InsertProductTemplate(template);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public IActionResult ProductTemplateDelete(string id)
        {
            var template = _productTemplateService.GetProductTemplateById(id);
            if (template == null)
                throw new ArgumentException("No template found with the specified id");
            if (ModelState.IsValid)
            {
                _productTemplateService.DeleteProductTemplate(template);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Topic templates

        public IActionResult TopicTemplates() => View();

        [HttpPost]
        public IActionResult TopicTemplates(DataSourceRequest command)
        {
            var templatesModel = _topicTemplateService.GetAllTopicTemplates()
                .Select(x => x.ToModel())
                .ToList();
            var gridModel = new DataSourceResult
            {
                Data = templatesModel,
                Total = templatesModel.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult TopicTemplateUpdate(TopicTemplateModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var template = _topicTemplateService.GetTopicTemplateById(model.Id);
            if (template == null)
                throw new ArgumentException("No template found with the specified id");
            if (ModelState.IsValid)
            {
                template = model.ToEntity(template);
                _topicTemplateService.UpdateTopicTemplate(template);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public IActionResult TopicTemplateAdd(TopicTemplateModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }
            if (ModelState.IsValid)
            {
                var template = new TopicTemplate();
                template = model.ToEntity(template);
                _topicTemplateService.InsertTopicTemplate(template);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public IActionResult TopicTemplateDelete(string id)
        {
            var template = _topicTemplateService.GetTopicTemplateById(id);
            if (template == null)
                throw new ArgumentException("No template found with the specified id");
            if (ModelState.IsValid)
            {
                _topicTemplateService.DeleteTopicTemplate(template);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion
    }
}
