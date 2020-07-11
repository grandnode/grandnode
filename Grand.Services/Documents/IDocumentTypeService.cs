using Grand.Domain.Documents;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Documents
{
    public partial interface IDocumentTypeService
    {
        /// <summary>
        /// Gets a document type
        /// </summary>
        /// <param name="id">document type identifier</param>
        /// <returns>Document type</returns>
        Task<DocumentType> GetById(string id);

        /// <summary>
        /// Gets all document types
        /// </summary>
        /// <returns>Document types</returns>
        Task<IList<DocumentType>> GetAll();


        /// <summary>
        /// Insert a document type
        /// </summary>
        /// <param name="documentType">Document type</param>
        Task Insert(DocumentType documentType);

        /// <summary>
        /// Update a document type
        /// </summary>
        /// <param name="documentType">Document type</param>
        Task Update(DocumentType documentType);

        /// <summary>
        /// Delete a document type
        /// </summary>
        /// <param name="documentType">Document type</param>
        Task Delete(DocumentType documentType);

    }
}
