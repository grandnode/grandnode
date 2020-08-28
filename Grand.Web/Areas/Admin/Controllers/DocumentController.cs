using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Customers;
using Grand.Services.Documents;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Documents;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Documents)]
    public class DocumentController : BaseAdminController
    {
        private readonly IDocumentViewModelService _documentViewModelService;
        private readonly IDocumentTypeService _documentTypeService;
        private readonly IDocumentService _documentService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;
        private readonly ICustomerActivityService _customerActivityService;

        public DocumentController(IDocumentViewModelService documentViewModelService, IDocumentService documentService, IDocumentTypeService documentTypeService,
            ILocalizationService localizationService, ICustomerService customerService, IStoreService storeService, ICustomerActivityService customerActivityService)
        {
            _documentViewModelService = documentViewModelService;
            _documentService = documentService;
            _documentTypeService = documentTypeService;
            _localizationService = localizationService;
            _customerService = customerService;
            _storeService = storeService;
            _customerActivityService = customerActivityService;
        }

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View(new DocumentListModel());

        [PermissionAuthorizeAction(PermissionActionName.List)]
        public async Task<IActionResult> ListDocuments(DataSourceRequest command, DocumentListModel model)
        {
            if (!string.IsNullOrEmpty(model.CustomerId))
                model.SearchEmail = (await _customerService.GetCustomerById(model.CustomerId))?.Email;

            var documents = await _documentViewModelService.PrepareDocumentListModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = documents.documetListModel,
                Total = documents.totalCount
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public async Task<IActionResult> CreateDocument(SimpleDocumentModel simpleModel)
        {
            var model = await _documentViewModelService.PrepareDocumentModel(null, null, simpleModel);
           
            //ACL
            await model.PrepareACLModel(null, false, _customerService);
            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, false, "");

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CreateDocument(DocumentModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.CustomerId))
                    model.CustomerId = (await _customerService.GetCustomerByEmail(model.CustomerEmail))?.Id;

                var document = model.ToEntity();
                document.CreatedOnUtc = DateTime.UtcNow;

                await _documentService.Insert(document);

                //activity log
                await _customerActivityService.InsertActivity("AddNewDocument", document.Id, _localizationService.GetResource("ActivityLog.AddNewDocument"), document.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Documents.Document.Added"));
                return continueEditing ? RedirectToAction("EditDocument", new { id = document.Id }) : RedirectToAction("List");
            }

            //ACL
            await model.PrepareACLModel(null, true, _customerService);
            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, true, "");

            model = await _documentViewModelService.PrepareDocumentModel(model, null, null);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> EditDocument(string id)
        {
            var document = await _documentService.GetById(id);
            if (document == null)
                return RedirectToAction("List");

            var model = await _documentViewModelService.PrepareDocumentModel(null, document, null);

            //ACL
            await model.PrepareACLModel(document, false, _customerService);
            //Stores
            await model.PrepareStoresMappingModel(document, _storeService, false, "");

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> EditDocument(DocumentModel model, bool continueEditing)
        {
            var document = await _documentService.GetById(model.Id);
            if (document == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.CustomerId))
                    model.CustomerId = (await _customerService.GetCustomerByEmail(model.CustomerEmail))?.Id;

                document = model.ToEntity(document);
                document.UpdatedOnUtc = DateTime.UtcNow;

                await _documentService.Update(document);

                //activity log
                await _customerActivityService.InsertActivity("EditDocument", document.Id, _localizationService.GetResource("ActivityLog.EditDocument"), document.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Documents.Document.Updated"));
                return continueEditing ? RedirectToAction("EditDocument", new { id = document.Id }) : RedirectToAction("List");
            }

            model = await _documentViewModelService.PrepareDocumentModel(model, document, null);
            //ACL
            await model.PrepareACLModel(document, true, _customerService);
            //Stores
            await model.PrepareStoresMappingModel(document, _storeService, true, "");

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> DeleteDocument(string id)
        {
            var document = await _documentService.GetById(id);
            if (document == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                await _documentService.Delete(document);

                //activity log
                await _customerActivityService.InsertActivity("DeleteDocument", document.Id, _localizationService.GetResource("ActivityLog.DeleteDocument"), document.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Documents.Document.Deleted"));
                return RedirectToAction("List");
            }
            return RedirectToAction("EditDocument", new { id = id });
        }

        #region Document type

        public IActionResult Types() => View();

        [PermissionAuthorizeAction(PermissionActionName.List)]
        public async Task<IActionResult> ListType()
        {
            var types = await _documentTypeService.GetAll();
            var gridModel = new DataSourceResult {
                Data = types,
                Total = types.Count
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public IActionResult CreateType()
        {
            var model = new DocumentTypeModel();
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CreateType(DocumentTypeModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var documenttype = model.ToEntity();
                await _documentTypeService.Insert(documenttype);

                //activity log
                await _customerActivityService.InsertActivity("AddNewDocumentType", documenttype.Id, _localizationService.GetResource("ActivityLog.AddNewDocumentType"), documenttype.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Documents.Type.Added"));
                return continueEditing ? RedirectToAction("EditType", new { id = documenttype.Id }) : RedirectToAction("Types");
            }
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> EditType(string id)
        {
            var documentType = await _documentTypeService.GetById(id);
            if (documentType == null)
                return RedirectToAction("Types");

            var model = documentType.ToModel();
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> EditType(DocumentTypeModel model, bool continueEditing)
        {
            var documentType = await _documentTypeService.GetById(model.Id);
            if (documentType == null)
                return RedirectToAction("Types");

            if (ModelState.IsValid)
            {
                documentType = model.ToEntity(documentType);
                await _documentTypeService.Update(documentType);

                //activity log
                await _customerActivityService.InsertActivity("EditDocumentType", documentType.Id, _localizationService.GetResource("ActivityLog.EditDocumentType"), documentType.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Documents.Type.Updated"));
                return continueEditing ? RedirectToAction("EditType", new { id = documentType.Id }) : RedirectToAction("Types");
            }

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> DeleteType(string id)
        {
            var documentType = await _documentTypeService.GetById(id);
            if (documentType == null)
                return RedirectToAction("Types");

            if (ModelState.IsValid)
            {
                await _documentTypeService.Delete(documentType);

                //activity log
                await _customerActivityService.InsertActivity("DeleteDocumentType", documentType.Id, _localizationService.GetResource("ActivityLog.DeleteDocumentType"), documentType.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Documents.Type.Deleted"));
                return RedirectToAction("Types");
            }
            return RedirectToAction("Edit", new { id = id });
        }
        #endregion


    }
}
