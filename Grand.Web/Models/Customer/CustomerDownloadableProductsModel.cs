using Grand.Core.Models;
using System;
using System.Collections.Generic;

namespace Grand.Web.Models.Customer
{
    public partial class CustomerDownloadableProductsModel : BaseModel
    {
        public CustomerDownloadableProductsModel()
        {
            Items = new List<DownloadableProductsModel>();
        }

        public IList<DownloadableProductsModel> Items { get; set; }

        #region Nested classes
        public partial class DownloadableProductsModel : BaseModel
        {
            public Guid OrderItemGuid { get; set; }

            public string OrderId { get; set; }
            public int OrderNumber { get; set; }

            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductSeName { get; set; }
            public string ProductAttributes { get; set; }

            public string DownloadId { get; set; }
            public string LicenseId { get; set; }

            public DateTime CreatedOn { get; set; }
        }
        #endregion
    }

    public partial class UserAgreementModel : BaseModel
    {
        public Guid OrderItemGuid { get; set; }
        public string UserAgreementText { get; set; }
    }
}