using Grand.Framework.Mvc.Models;
using System;
using System.Collections.Generic;

namespace Grand.Web.Models.Customer
{
    public class DocumentsModel : BaseGrandModel
    {
        public DocumentsModel()
        {
            DocumentList = new List<Document>();
        }

        public List<Document> DocumentList { get; set; }
        public string CustomerId { get; set; }
    }

    public class Document: BaseGrandEntityModel
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DownloadId { get; set; }
        public string Status { get; set; }
        public string DocumentType { get; set; }
        public string Link { get; set; }
        public decimal Amount { get; set; }
        public decimal OutstandAmount { get; set; }
        public int Quantity { get; set; }
        public DateTime? DocDate { get; set; }
        public DateTime? DueDate { get; set; }

    }
}