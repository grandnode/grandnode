using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.Documents;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Documents
{
    public partial class DocumentService : IDocumentService
    {
        private readonly IRepository<Document> _documentRepository;
        private readonly IMediator _mediator;

        public DocumentService(IRepository<Document> documentRepository, IMediator mediator)
        {
            _documentRepository = documentRepository;
            _mediator = mediator;
        }

        public async Task Delete(Document document)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            await _documentRepository.DeleteAsync(document);

            //event notification
            await _mediator.EntityDeleted(document);
        }

        public virtual async Task<IPagedList<Document>> GetAll(string customerId = "", string name = "", string number = "", string email = "", 
            int reference = 0, string objectId = "", int status = -1, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from d in _documentRepository.Table
                        select d;

            if (!string.IsNullOrEmpty(customerId))
                query = query.Where(d => d.CustomerId == customerId);

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(m => m.Name != null && m.Name.ToLower().Contains(name.ToLower()));

            if (!string.IsNullOrWhiteSpace(number))
                query = query.Where(m => m.Number != null && m.Number.ToLower().Contains(number.ToLower()));

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(m => m.CustomerEmail != null && m.CustomerEmail.ToLower().Contains(email.ToLower()));

            if (!string.IsNullOrWhiteSpace(objectId))
                query = query.Where(m => m.ObjectId == objectId);

            if (reference > 0)
                query = query.Where(m => m.ReferenceId == reference);

            if (status >= 0)
                query = query.Where(d => d.StatusId == status);

            return await PagedList<Document>.Create(query, pageIndex, pageSize);
        }


        public Task<Document> GetById(string id)
        {
            return _documentRepository.GetByIdAsync(id);
        }

        public async Task Insert(Document document)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            await _documentRepository.InsertAsync(document);

            //event notification
            await _mediator.EntityInserted(document);

        }

        public async Task Update(Document document)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            await _documentRepository.UpdateAsync(document);

            //event notification
            await _mediator.EntityUpdated(document);

        }
    }
}
