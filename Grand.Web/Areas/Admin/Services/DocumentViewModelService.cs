using Grand.Core.Domain.Documents;
using Grand.Services.Documents;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Documents;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Services
{
    public class DocumentViewModelService : IDocumentViewModelService
    {
        private readonly IDocumentService _documentService;
        private readonly IDocumentTypeService _documentTypeService;

        public DocumentViewModelService(IDocumentService documentService, IDocumentTypeService documentTypeService)
        {
            _documentService = documentService;
            _documentTypeService = documentTypeService;
        }

        public virtual async Task<(IEnumerable<DocumentModel> documetListModel, int totalCount)> PrepareDocumentListModel(DocumentListModel model, int pageIndex, int pageSize)
        {
            var documents = await _documentService.GetAll("", model.SearchName, model.SearchNumber, model.SearchEmail, model.StatusId, pageIndex - 1, pageSize);

            var documentListModel = new List<DocumentModel>();
            foreach (var x in documents)
            {
                var docModel = x.ToModel();
                documentListModel.Add(docModel);
            }
            return (documentListModel, documents.TotalCount);
        }

        public virtual async Task<DocumentModel> PrepareDocumentModel(DocumentModel documentModel, Document document, SimpleDocumentModel simpleModel)
        {
            var model = documentModel == null ? new DocumentModel() { Published = true } : documentModel;
            if (document != null)
                model = document.ToModel();
            else
            {
                model.CustomerId = simpleModel?.CustomerId;
            }
            var types = await _documentTypeService.GetAll();
            foreach (var item in types)
            {
                model.AvailableDocumentTypes.Add(new SelectListItem { Text = item.Name, Value = item.Id });
            }
            return model;
        }


    }
}
