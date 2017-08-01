using Grand.Core.Domain.Common;
using Grand.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Models.Checkout
{
    public partial class CheckoutPickupPointModel : BaseGrandModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Address Address { get; set; }

        public string PickupFee { get; set; }

    }
}