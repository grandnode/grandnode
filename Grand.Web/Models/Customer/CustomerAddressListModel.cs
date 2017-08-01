using System.Collections.Generic;
using Grand.Framework.Mvc.Models;
using Grand.Web.Models.Common;

namespace Grand.Web.Models.Customer
{
    public partial class CustomerAddressListModel : BaseGrandModel
    {
        public CustomerAddressListModel()
        {
            Addresses = new List<AddressModel>();
        }

        public IList<AddressModel> Addresses { get; set; }
    }
}