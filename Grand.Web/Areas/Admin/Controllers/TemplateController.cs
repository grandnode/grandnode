using Grand.Domain.Catalog;
using Grand.Domain.Topics;
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
using System.Threading.Tasks;

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
            _categoryTemplateService = categoryTemplateService;
            _manufacturerTemplateService = manufacturerTemplateService;
            _productTemplateService = productTemplateService;
            _topicTemplateService = topicTemplateService;
        }

        #endregion

        #region Category templates

        public IActionResult CategoryTemplates() => View();

        [HttpPost]
        public async Task<IActionResult> CategoryTemplates(DataSourceRequest command)
        {
            var templatesModel = (await _categoryTemplateService.GetAllCategoryTemplates())
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
        public async Task<IActionResult> CategoryTemplateUpdate(CategoryTemplateModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var template = await _categoryTemplateService.GetCategoryTemplateById(model.Id);
            if (template == null)
                throw new ArgumentException("No template found with the specified id");
            if (ModelState.IsValid)
            {
                template = model.ToEntity(template);
                await _categoryTemplateService.UpdateCategoryTemplate(template);

                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> CategoryTemplateAdd(CategoryTemplateModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }
            if (ModelState.IsValid)
            {
                var template = new CategoryTemplate();
                template = model.ToEntity(template);
                await _categoryTemplateService.InsertCategoryTemplate(template);

                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> CategoryTemplateDelete(string id)
        {
            var template = await _categoryTemplateService.GetCategoryTemplateById(id);
            if (template == null)
                throw new ArgumentException("No template found with the specified id");
            if (ModelState.IsValid)
            {
                await _categoryTemplateService.DeleteCategoryTemplate(template);

                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Manufacturer templates
        public IActionResult ManufacturerTemplates() => View();

        [HttpPost]
        public async Task<IActionResult> ManufacturerTemplates(DataSourceRequest command)
        {
            var templatesModel = (await _manufacturerTemplateService.GetAllManufacturerTemplates())
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
        public async Task<IActionResult> ManufacturerTemplateUpdate(ManufacturerTemplateModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var template = await _manufacturerTemplateService.GetManufacturerTemplateById(model.Id);
            if (template == null)
                throw new ArgumentException("No template found with the specified id");
            if (ModelState.IsValid)
            {
                template = model.ToEntity(template);
                await _manufacturerTemplateService.UpdateManufacturerTemplate(template);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> ManufacturerTemplateAdd(ManufacturerTemplateModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }
            if (ModelState.IsValid)
            {
                var template = new ManufacturerTemplate();
                template = model.ToEntity(template);
                await _manufacturerTemplateService.InsertManufacturerTemplate(template);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> ManufacturerTemplateDelete(string id)
        {
            var template = await _manufacturerTemplateService.GetManufacturerTemplateById(id);
            if (template == null)
                throw new ArgumentException("No template found with the specified id");
            if (ModelState.IsValid)
            {
                await _manufacturerTemplateService.DeleteManufacturerTemplate(template);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Product templates

        public IActionResult ProductTemplates() => View();

        [HttpPost]
        public async Task<IActionResult> ProductTemplates(DataSourceRequest command)
        {
            var templatesModel = (await _productTemplateService.GetAllProductTemplates())
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
        public async Task<IActionResult> ProductTemplateUpdate(ProductTemplateModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }
            var template = await _productTemplateService.GetProductTemplateById(model.Id);
            if (template == null)
                throw new ArgumentException("No template found with the specified id");
            if (ModelState.IsValid)
            {
                template = model.ToEntity(template);
                await _productTemplateService.UpdateProductTemplate(template);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> ProductTemplateAdd(ProductTemplateModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }
            if (ModelState.IsValid)
            {
                var template = new ProductTemplate();
                template = model.ToEntity(template);
                await _productTemplateService.InsertProductTemplate(template);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> ProductTemplateDelete(string id)
        {
            var template = await _productTemplateService.GetProductTemplateById(id);
            if (template == null)
                throw new ArgumentException("No template found with the specified id");
            if (ModelState.IsValid)
            {
                await _productTemplateService.DeleteProductTemplate(template);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Topic templates

        public IActionResult TopicTemplates() => View();

        [HttpPost]
        public async Task<IActionResult> TopicTemplates(DataSourceRequest command)
        {
            var templatesModel = (await _topicTemplateService.GetAllTopicTemplates())
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
        public async Task<IActionResult> TopicTemplateUpdate(TopicTemplateModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var template = await _topicTemplateService.GetTopicTemplateById(model.Id);
            if (template == null)
                throw new ArgumentException("No template found with the specified id");
            if (ModelState.IsValid)
            {
                template = model.ToEntity(template);
                await _topicTemplateService.UpdateTopicTemplate(template);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> TopicTemplateAdd(TopicTemplateModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }
            if (ModelState.IsValid)
            {
                var template = new TopicTemplate();
                template = model.ToEntity(template);
                await _topicTemplateService.InsertTopicTemplate(template);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> TopicTemplateDelete(string id)
        {
            var template = await _topicTemplateService.GetTopicTemplateById(id);
            if (template == null)
                throw new ArgumentException("No template found with the specified id");
            if (ModelState.IsValid)
            {
                await _topicTemplateService.DeleteTopicTemplate(template);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion
    }
}
