using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Documents;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
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

        public async Task<IList<Document>> GetAll()
        {
            var query = _documentRepository.Table;
            return await query.ToListAsync();
        }

        public async Task<IPagedList<Document>> GetByCustomerId(string customerId = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from c in _documentRepository.Table
                        where c.CustomerId == customerId
                        select c;
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
