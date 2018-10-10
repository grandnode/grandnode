using FluentValidation.Attributes;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Validators.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Web.Models.Common
{
    [Validator(typeof(AddressValidator))]
    public partial class AddressModel : BaseGrandEntityModel
    {
        public AddressModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            CustomAddressAttributes = new List<AddressAttributeModel>();
        }

        [GrandResourceDisplayName("Address.Fields.FirstName")]
        public string FirstName { get; set; }
        [GrandResourceDisplayName("Address.Fields.LastName")]
        public string LastName { get; set; }
        [GrandResourceDisplayName("Address.Fields.Email")]
        public string Email { get; set; }


        public bool CompanyEnabled { get; set; }
        public bool CompanyRequired { get; set; }
        [GrandResourceDisplayName("Address.Fields.Company")]
        public string Company { get; set; }

        public bool VatNumberEnabled { get; set; }
        public bool VatNumberRequired { get; set; }
        [GrandResourceDisplayName("Address.Fields.VatNumber")]
        public string VatNumber { get; set; }

        public bool CountryEnabled { get; set; }
        [GrandResourceDisplayName("Address.Fields.Country")]
        public string CountryId { get; set; }
        [GrandResourceDisplayName("Address.Fields.Country")]
        public string CountryName { get; set; }

        public bool StateProvinceEnabled { get; set; }
        [GrandResourceDisplayName("Address.Fields.StateProvince")]
        public string StateProvinceId { get; set; }
        [GrandResourceDisplayName("Address.Fields.StateProvince")]
        public string StateProvinceName { get; set; }

        public bool CityEnabled { get; set; }
        public bool CityRequired { get; set; }
        [GrandResourceDisplayName("Address.Fields.City")]
        public string City { get; set; }

        public bool StreetAddressEnabled { get; set; }
        public bool StreetAddressRequired { get; set; }
        [GrandResourceDisplayName("Address.Fields.Address1")]
        public string Address1 { get; set; }

        public bool StreetAddress2Enabled { get; set; }
        public bool StreetAddress2Required { get; set; }
        [GrandResourceDisplayName("Address.Fields.Address2")]
        public string Address2 { get; set; }

        public bool ZipPostalCodeEnabled { get; set; }
        public bool ZipPostalCodeRequired { get; set; }
        [GrandResourceDisplayName("Address.Fields.ZipPostalCode")]
        public string ZipPostalCode { get; set; }

        public bool PhoneEnabled { get; set; }
        public bool PhoneRequired { get; set; }
        [GrandResourceDisplayName("Address.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }

        public bool FaxEnabled { get; set; }
        public bool FaxRequired { get; set; }
        [GrandResourceDisplayName("Address.Fields.FaxNumber")]
        public string FaxNumber { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }


        public string FormattedCustomAddressAttributes { get; set; }
        public IList<AddressAttributeModel> CustomAddressAttributes { get; set; }
    }
}