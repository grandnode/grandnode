using FluentValidation.Attributes;
using Grand.Framework.Mvc.Models;
using Grand.Web.Models.Common;
using Grand.Web.Validators.Customer;
using System.Collections.Generic;

namespace Grand.Web.Models.Checkout
{
    [Validator(typeof(CheckoutShippingAddressValidator))]
    public partial class CheckoutShippingAddressModel : BaseGrandModel
    {
        public CheckoutShippingAddressModel()
        {
            ExistingAddresses = new List<AddressModel>();
            NewAddress = new AddressModel();
            Warnings = new List<string>();
            PickupPoints = new List<CheckoutPickupPointModel>();
        }

        public IList<AddressModel> ExistingAddresses { get; set; }
        public IList<string> Warnings { get; set; }
        public AddressModel NewAddress { get; set; }

        public bool NewAddressPreselected { get; set; }
        public IList<CheckoutPickupPointModel> PickupPoints { get; set; }
        public bool AllowPickUpInStore { get; set; }

        public bool PickUpInStore { get; set; }
        public bool PickUpInStoreOnly { get; set; }
    }
}