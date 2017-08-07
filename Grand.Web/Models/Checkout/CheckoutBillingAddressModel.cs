using System.Collections.Generic;
using Grand.Framework.Mvc.Models;
using Grand.Web.Models.Common;
using FluentValidation.Attributes;
using Grand.Web.Validators.Customer;

namespace Grand.Web.Models.Checkout
{
    [Validator(typeof(CheckoutBillingAddressValidator))]
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