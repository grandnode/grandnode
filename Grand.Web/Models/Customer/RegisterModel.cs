using FluentValidation.Attributes;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Models.Newsletter;
using Grand.Web.Validators.Customer;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Models.Customer
{
    [Validator(typeof(RegisterValidator))]
    public partial class RegisterModel : BaseGrandModel
    {
        public RegisterModel()
        {
            this.AvailableTimeZones = new List<SelectListItem>();
            this.AvailableCountries = new List<SelectListItem>();
            this.AvailableStates = new List<SelectListItem>();
            this.CustomerAttributes = new List<CustomerAttributeModel>();
            this.NewsletterCategories = new List<NewsletterSimpleCategory>();
        }

        [GrandResourceDisplayName("Account.Fields.Email")]
        public string Email { get; set; }

        public bool UsernamesEnabled { get; set; }
        [GrandResourceDisplayName("Account.Fields.Username")]
        public string Username { get; set; }

        public bool CheckUsernameAvailabilityEnabled { get; set; }

        [DataType(DataType.Password)]
        [GrandResourceDisplayName("Account.Fields.Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [GrandResourceDisplayName("Account.Fields.ConfirmPassword")]
        public string ConfirmPassword { get; set; }

        //form fields & properties
        public bool GenderEnabled { get; set; }
        [GrandResourceDisplayName("Account.Fields.Gender")]
        public string Gender { get; set; }

        [GrandResourceDisplayName("Account.Fields.FirstName")]
        public string FirstName { get; set; }

        [GrandResourceDisplayName("Account.Fields.LastName")]
        public string LastName { get; set; }


        public bool DateOfBirthEnabled { get; set; }
        [GrandResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthDay { get; set; }
        [GrandResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthMonth { get; set; }
        [GrandResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthYear { get; set; }
        public bool DateOfBirthRequired { get; set; }
        public DateTime? ParseDateOfBirth()
        {
            if (!DateOfBirthYear.HasValue || !DateOfBirthMonth.HasValue || !DateOfBirthDay.HasValue)
                return null;

            DateTime? dateOfBirth = null;
            try
            {
                dateOfBirth = new DateTime(DateOfBirthYear.Value, DateOfBirthMonth.Value, DateOfBirthDay.Value);
            }
            catch { }
            return dateOfBirth;
        }

        public bool CompanyEnabled { get; set; }
        public bool CompanyRequired { get; set; }
        [GrandResourceDisplayName("Account.Fields.Company")]
        public string Company { get; set; }

        public bool StreetAddressEnabled { get; set; }
        public bool StreetAddressRequired { get; set; }
        [GrandResourceDisplayName("Account.Fields.StreetAddress")]
        public string StreetAddress { get; set; }

        public bool StreetAddress2Enabled { get; set; }
        public bool StreetAddress2Required { get; set; }
        [GrandResourceDisplayName("Account.Fields.StreetAddress2")]
        public string StreetAddress2 { get; set; }

        public bool ZipPostalCodeEnabled { get; set; }
        public bool ZipPostalCodeRequired { get; set; }
        [GrandResourceDisplayName("Account.Fields.ZipPostalCode")]
        public string ZipPostalCode { get; set; }

        public bool CityEnabled { get; set; }
        public bool CityRequired { get; set; }
        [GrandResourceDisplayName("Account.Fields.City")]
        public string City { get; set; }

        public bool CountryEnabled { get; set; }
        public bool CountryRequired { get; set; }
        [GrandResourceDisplayName("Account.Fields.Country")]
        public string CountryId { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }

        public bool StateProvinceEnabled { get; set; }
        public bool StateProvinceRequired { get; set; }
        [GrandResourceDisplayName("Account.Fields.StateProvince")]
        public string StateProvinceId { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }

        public bool PhoneEnabled { get; set; }
        public bool PhoneRequired { get; set; }
        [GrandResourceDisplayName("Account.Fields.Phone")]
        public string Phone { get; set; }

        public bool FaxEnabled { get; set; }
        public bool FaxRequired { get; set; }
        [GrandResourceDisplayName("Account.Fields.Fax")]
        public string Fax { get; set; }
        
        public bool NewsletterEnabled { get; set; }
        [GrandResourceDisplayName("Account.Fields.Newsletter")]
        public bool Newsletter { get; set; }
        
        public bool AcceptPrivacyPolicyEnabled { get; set; }

        //time zone
        [GrandResourceDisplayName("Account.Fields.TimeZone")]
        public string TimeZoneId { get; set; }
        public bool AllowCustomersToSetTimeZone { get; set; }
        public IList<SelectListItem> AvailableTimeZones { get; set; }

        //EU VAT
        [GrandResourceDisplayName("Account.Fields.VatNumber")]
        public string VatNumber { get; set; }
        public bool DisplayVatNumber { get; set; }

        public bool HoneypotEnabled { get; set; }
        public bool DisplayCaptcha { get; set; }

        public IList<CustomerAttributeModel> CustomerAttributes { get; set; }
        public IList<NewsletterSimpleCategory> NewsletterCategories { get; set; }
    }
}