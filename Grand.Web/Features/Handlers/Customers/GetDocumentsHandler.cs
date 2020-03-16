using Grand.Services.Documents;
using Grand.Services.Localization;
using Grand.Web.Features.Models.Customers;
using Grand.Web.Models.Customer;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Customers
{
    public class GetDocumentsHandler : IRequestHandler<GetDocuments, DocumentsModel>
    {
        private readonly IDocumentService _documentService;
        private readonly IDocumentTypeService _documentTypeService;
        private readonly ILocalizationService _localizationService;

        public GetDocumentsHandler(IDocumentService documentService,
            IDocumentTypeService documentTypeService,
            ILocalizationService localizationService)
        {
            _documentService = documentService;
            _documentTypeService = documentTypeService;
            _localizationService = localizationService;
        }

        public async Task<DocumentsModel> Handle(GetDocuments request, CancellationToken cancellationToken)
        {
            var model = new DocumentsModel();
            model.CustomerId = request.Customer.Id;
            var documents = await _documentService.GetAll(request.Customer.Id);
            foreach (var item in documents.Where(x => x.Published).OrderBy(x => x.DisplayOrder))
            {
                var doc = new Document();
                doc.Id = item.Id;
                doc.Amount = item.TotalAmount;
                doc.OutstandAmount = item.OutstandAmount;
                doc.Link = item.Link;
                doc.Name = item.Name;
                doc.Number = item.Number;
                doc.Quantity = item.Quantity;
                doc.Status = item.DocumentStatus.GetLocalizedEnum(_localizationService, request.Language.Id);
                doc.Description = item.Description;
                doc.DocDate = item.DocDate;
                doc.DueDate = item.DueDate;
                doc.DocumentType = (await _documentTypeService.GetById(item.DocumentTypeId))?.Name;
                doc.DownloadId = item.DownloadId;
                model.DocumentList.Add(doc);
            }
            return model;
        }
    }
}
