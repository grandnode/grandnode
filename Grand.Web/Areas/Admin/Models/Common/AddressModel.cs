using Grand.Domain.Catalog;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Common
{
    public partial class AddressModel : BaseGrandEntityModel
    {
        public AddressModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            CustomAddressAttributes = new List<AddressAttributeModel>();
        }

        [GrandResourceDisplayName("Admin.Address.Fields.FirstName")]

        public string FirstName { get; set; }

        [GrandResourceDisplayName("Admin.Address.Fields.LastName")]

        public string LastName { get; set; }

        [GrandResourceDisplayName("Admin.Address.Fields.Email")]

        public string Email { get; set; }

        [GrandResourceDisplayName("Admin.Address.Fields.Company")]

        public string Company { get; set; }

        [GrandResourceDisplayName("Admin.Address.Fields.VatNumber")]
        public string VatNumber { get; set; }

        [GrandResourceDisplayName("Admin.Address.Fields.Country")]
        public string CountryId { get; set; }

        [GrandResourceDisplayName("Admin.Address.Fields.Country")]

        public string CountryName { get; set; }

        [GrandResourceDisplayName("Admin.Address.Fields.StateProvince")]
        public string StateProvinceId { get; set; }

        [GrandResourceDisplayName("Admin.Address.Fields.StateProvince")]

        public string StateProvinceName { get; set; }

        [GrandResourceDisplayName("Admin.Address.Fields.City")]

        public string City { get; set; }

        [GrandResourceDisplayName("Admin.Address.Fields.Address1")]

        public string Address1 { get; set; }

        [GrandResourceDisplayName("Admin.Address.Fields.Address2")]

        public string Address2 { get; set; }

        [GrandResourceDisplayName("Admin.Address.Fields.ZipPostalCode")]

        public string ZipPostalCode { get; set; }

        [GrandResourceDisplayName("Admin.Address.Fields.PhoneNumber")]

        public string PhoneNumber { get; set; }

        [GrandResourceDisplayName("Admin.Address.Fields.FaxNumber")]

        public string FaxNumber { get; set; }

        //address in HTML format (usually used in grids)
        [GrandResourceDisplayName("Admin.Address")]
        public string AddressHtml { get; set; }

        //formatted custom address attributes
        public string FormattedCustomAddressAttributes { get; set; }
        public IList<AddressAttributeModel> CustomAddressAttributes { get; set; }


        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }



        public bool FirstNameEnabled { get; set; }
        public bool FirstNameRequired { get; set; }
        public bool LastNameEnabled { get; set; }
        public bool LastNameRequired { get; set; }
        public bool EmailEnabled { get; set; }
        public bool EmailRequired { get; set; }
        public bool CompanyEnabled { get; set; }
        public bool CompanyRequired { get; set; }
        public bool VatNumberEnabled { get; set; }
        public bool VatNumberRequired { get; set; }
        public bool CountryEnabled { get; set; }
        public bool StateProvinceEnabled { get; set; }
        public bool CityEnabled { get; set; }
        public bool CityRequired { get; set; }
        public bool StreetAddressEnabled { get; set; }
        public bool StreetAddressRequired { get; set; }
        public bool StreetAddress2Enabled { get; set; }
        public bool StreetAddress2Required { get; set; }
        public bool ZipPostalCodeEnabled { get; set; }
        public bool ZipPostalCodeRequired { get; set; }
        public bool PhoneEnabled { get; set; }
        public bool PhoneRequired { get; set; }
        public bool FaxEnabled { get; set; }
        public bool FaxRequired { get; set; }


        #region Nested classes

        public partial class AddressAttributeModel : BaseGrandEntityModel
        {
            public AddressAttributeModel()
            {
                Values = new List<AddressAttributeValueModel>();
            }

            public string Name { get; set; }

            public bool IsRequired { get; set; }

            /// <summary>
            /// Selected value for textboxes
            /// </summary>
            public string DefaultValue { get; set; }

            public AttributeControlType AttributeControlType { get; set; }

            public IList<AddressAttributeValueModel> Values { get; set; }
        }

        public partial class AddressAttributeValueModel : BaseGrandEntityModel
        {
            public string Name { get; set; }

            public bool IsPreSelected { get; set; }
        }

        #endregion
    }
}