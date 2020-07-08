using Grand.Domain.Data;
using Grand.Domain.Documents;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Documents
{
    public partial class DocumentTypeService : IDocumentTypeService
    {
        private readonly IRepository<DocumentType> _documentTypeRepository;
        private readonly IMediator _mediator;

        public DocumentTypeService(IRepository<DocumentType> documentTypeRepository, IMediator mediator)
        {
            _documentTypeRepository = documentTypeRepository;
            _mediator = mediator;
        }

        public virtual async Task Delete(DocumentType documentType)
        {
            if (documentType == null)
                throw new ArgumentNullException("documentType");

            await _documentTypeRepository.DeleteAsync(documentType);

            //event notification
            await _mediator.EntityDeleted(documentType);
        }

        public virtual async Task<IList<DocumentType>> GetAll()
        {
            var query = from t in _documentTypeRepository.Table
                        orderby t.DisplayOrder
                        select t;
            return await query.ToListAsync();
        }

        public virtual Task<DocumentType> GetById(string id)
        {
            return _documentTypeRepository.GetByIdAsync(id);
        }

        public virtual async Task Insert(DocumentType documentType)
        {
            if (documentType == null)
                throw new ArgumentNullException("documentType");

            await _documentTypeRepository.InsertAsync(documentType);

            //event notification
            await _mediator.EntityInserted(documentType);
        }

        public virtual async Task Update(DocumentType documentType)
        {
            if (documentType == null)
                throw new ArgumentNullException("documentType");

            await _documentTypeRepository.UpdateAsync(documentType);

            //event notification
            await _mediator.EntityUpdated(documentType);

        }
    }
}
