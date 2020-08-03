using Grand.Framework.Mapping;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Documents
{
    public class DocumentModel : BaseGrandEntityModel, IAclMappingModel, IStoreMappingModel
    {
        public DocumentModel()
        {
            AvailableDocumentTypes = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.Number")]
        public string Number { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.Description")]
        public string Description { get; set; }

        public string ParentDocumentId { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.Picture")]
        [UIHint("Picture")]
        public string PictureId { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.Download")]
        [UIHint("Download")]
        public string DownloadId { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.Published")]
        public bool Published { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.Flag")]
        public string Flag { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.Link")]
        public string Link { get; set; }

        public string CustomerId { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.Status")]
        public int StatusId { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.Reference")]
        public int ReferenceId { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.Object")]
        public string ObjectId { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.DocumentType")]
        public string DocumentTypeId { get; set; }
        public IList<SelectListItem> AvailableDocumentTypes { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.CustomerEmail")]
        public string CustomerEmail { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.Username")]
        public string Username { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.CurrencyCode")]
        public string CurrencyCode { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.TotalAmount")]
        public decimal TotalAmount { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.OutstandAmount")]
        public decimal OutstandAmount { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.Quantity")]
        public int Quantity { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.DocDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? DocDate { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Document.Fields.DueDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? DueDate { get; set; }

        //ACL
        [GrandResourceDisplayName("Admin.Documents.Document.Fields.SubjectToAcl")]
        public bool SubjectToAcl { get; set; }
        [GrandResourceDisplayName("Admin.Documents.Document.Fields.AclCustomerRoles")]
        public List<CustomerRoleModel> AvailableCustomerRoles { get; set; }
        public string[] SelectedCustomerRoleIds { get; set; }

        //Store mapping
        [GrandResourceDisplayName("Admin.Documents.Document.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [GrandResourceDisplayName("Admin.Documents.Document.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }
    }
}
