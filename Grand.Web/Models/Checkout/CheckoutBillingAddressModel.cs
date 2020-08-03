using Grand.Framework.Mvc.Models;
using Grand.Web.Models.Common;
using System.Collections.Generic;

namespace Grand.Web.Models.Checkout
{
    public partial class CheckoutBillingAddressModel : BaseGrandModel
    {
        public CheckoutBillingAddressModel()
        {
            ExistingAddresses = new List<AddressModel>();
            NewAddress = new AddressModel();
        }

        public IList<AddressModel> ExistingAddresses { get; set; }

        public AddressModel NewAddress { get; set; }

        public bool ShipToSameAddress { get; set; }
        public bool ShipToSameAddressAllowed { get; set; }
        /// <summary>
        /// Used on one-page checkout page
        /// </summary>
        public bool NewAddressPreselected { get; set; }
    }
}