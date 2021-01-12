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
        private readonly Domain.Documents.DocumentSettings _documentSettings;

        public GetDocumentsHandler(IDocumentService documentService,
            IDocumentTypeService documentTypeService,
            ILocalizationService localizationService,
            Domain.Documents.DocumentSettings documentSettings)
        {
            _documentService = documentService;
            _documentTypeService = documentTypeService;
            _localizationService = localizationService;
            _documentSettings = documentSettings;
        }

        public async Task<DocumentsModel> Handle(GetDocuments request, CancellationToken cancellationToken)
        {
            if (request.Command.PageSize <= 0) request.Command.PageSize = _documentSettings.PageSize;
            if (request.Command.PageNumber <= 0) request.Command.PageNumber = 1;

            var model = new DocumentsModel {
                CustomerId = request.Customer.Id
            };
            var documents = await _documentService.GetAll(request.Customer.Id, 
                pageIndex: request.Command.PageNumber - 1, 
                pageSize: request.Command.PageSize);
            model.PagingContext.LoadPagedList(documents);
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
