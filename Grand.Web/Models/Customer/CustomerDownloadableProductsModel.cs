using System;
using System.Collections.Generic;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Customer
{
    public partial class CustomerDownloadableProductsModel : BaseGrandModel
    {
        public CustomerDownloadableProductsModel()
        {
            Items = new List<DownloadableProductsModel>();
        }

        public IList<DownloadableProductsModel> Items { get; set; }

        #region Nested classes
        public partial class DownloadableProductsModel : BaseGrandModel
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

    public partial class UserAgreementModel : BaseGrandModel
    {
        public Guid OrderItemGuid { get; set; }
        public string UserAgreementText { get; set; }
    }
}