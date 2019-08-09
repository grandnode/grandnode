using Grand.Core;
using Grand.Core.Domain.Documents;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Documents
{
    public partial interface IDocumentService
    {
        /// <summary>
        /// Gets a document 
        /// </summary>
        /// <param name="id">document identifier</param>
        /// <returns>Document</returns>
        Task<Document> GetById(string id);

        /// <summary>
        /// Gets all documents 
        /// </summary>
        /// <returns>Documents</returns>
        Task<IList<Document>> GetAll();

        /// <summary>
        /// Gets all documents for customer id
        /// </summary>
        /// <returns>Documents</returns>
        Task<IPagedList<Document>> GetByCustomerId(string customerId = "", int pageIndex = 0, int pageSize = 2147483647);

        /// <summary>
        /// Insert a document
        /// </summary>
        /// <param name="document">Document</param>
        Task Insert(Document document);

        /// <summary>
        /// Update a document type
        /// </summary>
        /// <param name="document">Document</param>
        Task Update(Document document);

        /// <summary>
        /// Delete a document type
        /// </summary>
        /// <param name="document">Document</param>
        Task Delete(Document document);
    }
}
