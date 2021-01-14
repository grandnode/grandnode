using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Admin.Models.Documents
{
    public class DocumentListModel : BaseModel
    {
        [GrandResourceDisplayName("Admin.Documents.Document.List.SearchName")]
        public string SearchName { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.List.SearchNumber")]
        public string SearchNumber { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.List.SearchEmail")]
        public string SearchEmail { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.List.DocumentStatus")]
        public int StatusId { get; set; }

        public int Reference { get; set; }

        public string ObjectId { get; set; }
        public string CustomerId { get; set; }

    }
}
