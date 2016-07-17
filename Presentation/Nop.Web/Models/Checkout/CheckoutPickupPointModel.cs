using Nop.Core.Domain.Common;
using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Checkout
{
    public partial class CheckoutPickupPointModel : BaseNopModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Address Address { get; set; }

        public string PickupFee { get; set; }

    }
}