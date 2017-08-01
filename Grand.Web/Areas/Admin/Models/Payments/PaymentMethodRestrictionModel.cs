using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using Grand.Web.Areas.Admin.Models.Directory;
using Grand.Framework.Mvc;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Models.Shipping;

namespace Grand.Web.Areas.Admin.Models.Payments
{
    public partial class PaymentMethodRestrictionModel : BaseGrandModel
    {
        public PaymentMethodRestrictionModel()
        {
            AvailablePaymentMethods = new List<PaymentMethodModel>();
            AvailableCountries = new List<CountryModel>();
            AvailableCustomerRoles = new List<CustomerRoleModel>();
            AvailableShippingMethods = new List<ShippingMethodModel>();
            Resticted = new Dictionary<string, IDictionary<string, bool>>();
            RestictedRole = new Dictionary<string, IDictionary<string, bool>>();
            RestictedShipping = new Dictionary<string, IDictionary<string, bool>>();
        }
        public IList<PaymentMethodModel> AvailablePaymentMethods { get; set; }
        public IList<CountryModel> AvailableCountries { get; set; }
        public IList<CustomerRoleModel> AvailableCustomerRoles { get; set; }
        public IList<ShippingMethodModel> AvailableShippingMethods { get; set; }

        //[payment method system name] / [customer role id] / [resticted]
        public IDictionary<string, IDictionary<string, bool>> Resticted { get; set; }
        public IDictionary<string, IDictionary<string, bool>> RestictedRole { get; set; }
        public IDictionary<string, IDictionary<string, bool>> RestictedShipping { get; set; }
    }

}