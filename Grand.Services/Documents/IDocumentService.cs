using Grand.Domain;
using Grand.Domain.Documents;
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
        Task<IPagedList<Document>> GetAll(string customerId = "", string name = "", string number = "", string email = "", 
            int reference = 0, string objectId = "", int status = -1, int pageIndex = 0, int pageSize = int.MaxValue);

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
