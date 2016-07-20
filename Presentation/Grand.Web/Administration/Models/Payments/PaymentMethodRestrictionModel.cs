using System.Collections.Generic;
using Grand.Admin.Models.Directory;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Payments
{
    public partial class PaymentMethodRestrictionModel : BaseNopModel
    {
        public PaymentMethodRestrictionModel()
        {
            AvailablePaymentMethods = new List<PaymentMethodModel>();
            AvailableCountries = new List<CountryModel>();
            Resticted = new Dictionary<string, IDictionary<string, bool>>();
        }
        public IList<PaymentMethodModel> AvailablePaymentMethods { get; set; }
        public IList<CountryModel> AvailableCountries { get; set; }

        //[payment method system name] / [customer role id] / [resticted]
        public IDictionary<string, IDictionary<string, bool>> Resticted { get; set; }
    }
}