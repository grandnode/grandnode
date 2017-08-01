using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using Grand.Web.Areas.Admin.Models.Directory;
using Grand.Framework.Mvc;
using Grand.Web.Areas.Admin.Models.Customers;

namespace Grand.Web.Areas.Admin.Models.Shipping
{
    public partial class ShippingMethodRestrictionModel : BaseGrandModel
    {
        public ShippingMethodRestrictionModel()
        {
            AvailableShippingMethods = new List<ShippingMethodModel>();
            AvailableCountries = new List<CountryModel>();
            AvailableCustomerRoles = new List<CustomerRoleModel>();
            Restricted = new Dictionary<string, IDictionary<string, bool>>();
            RestictedRole = new Dictionary<string, IDictionary<string, bool>>();
        }
        public IList<ShippingMethodModel> AvailableShippingMethods { get; set; }
        public IList<CountryModel> AvailableCountries { get; set; }
        public IList<CustomerRoleModel> AvailableCustomerRoles { get; set; }

        //[country id] / [shipping method id] / [restricted]
        public IDictionary<string, IDictionary<string, bool>> Restricted { get; set; }
        public IDictionary<string, IDictionary<string, bool>> RestictedRole { get; set; }
    }
}