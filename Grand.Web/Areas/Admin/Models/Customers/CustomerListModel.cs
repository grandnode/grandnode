using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerListModel : BaseGrandModel
    {
        public CustomerListModel()
        {
            AvailableCustomerTags = new List<SelectListItem>();
            SearchCustomerTagIds = new List<string>();
            SearchCustomerRoleIds = new List<string>();
            AvailableCustomerRoles = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Customers.Customers.List.CustomerRoles")]
        
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }


        [GrandResourceDisplayName("Admin.Customers.Customers.List.CustomerRoles")]
        [UIHint("MultiSelect")]
        public IList<string> SearchCustomerRoleIds { get; set; }

        [GrandResourceDisplayName("Admin.Customers.Customers.List.CustomerTags")]
        public IList<SelectListItem> AvailableCustomerTags { get; set; }

        [GrandResourceDisplayName("Admin.Customers.Customers.List.CustomerTags")]
        [UIHint("MultiSelect")]
        public IList<string> SearchCustomerTagIds { get; set; }


        [GrandResourceDisplayName("Admin.Customers.Customers.List.SearchEmail")]
        
        public string SearchEmail { get; set; }

        [GrandResourceDisplayName("Admin.Customers.Customers.List.SearchUsername")]
        
        public string SearchUsername { get; set; }
        public bool UsernamesEnabled { get; set; }

        [GrandResourceDisplayName("Admin.Customers.Customers.List.SearchFirstName")]
        
        public string SearchFirstName { get; set; }
        [GrandResourceDisplayName("Admin.Customers.Customers.List.SearchLastName")]
        
        public string SearchLastName { get; set; }


        [GrandResourceDisplayName("Admin.Customers.Customers.List.SearchCompany")]
        
        public string SearchCompany { get; set; }
        public bool CompanyEnabled { get; set; }

        [GrandResourceDisplayName("Admin.Customers.Customers.List.SearchPhone")]
        
        public string SearchPhone { get; set; }
        public bool PhoneEnabled { get; set; }

        [GrandResourceDisplayName("Admin.Customers.Customers.List.SearchZipCode")]
        
        public string SearchZipPostalCode { get; set; }
        public bool ZipPostalCodeEnabled { get; set; }
    }
}