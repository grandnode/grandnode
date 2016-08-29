using System;
using System.Collections.Generic;
using Grand.Core;
using Grand.Core.Domain.Common;

namespace Grand.Services.Common
{
    public static class AddressExtensions
    {
        /// <summary>
        /// Find an address
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param name="phoneNumber">Phone number</param>
        /// <param name="email">Email</param>
        /// <param name="faxNumber">Fax number</param>
        /// <param name="company">Company</param>
        /// <param name="address1">Address 1</param>
        /// <param name="address2">Address 2</param>
        /// <param name="city">City</param>
        /// <param name="stateProvinceId">State/province identifier</param>
        /// <param name="zipPostalCode">Zip postal code</param>
        /// <param name="countryId">Country identifier</param>
        /// <param name="customAttributes">Custom address attributes (XML format)</param>
        /// <returns>Address</returns>
        public static Address FindAddress(this List<Address> source,
            string firstName, string lastName, string phoneNumber,
            string email, string faxNumber, string company, string address1,
            string address2, string city, string stateProvinceId,
            string zipPostalCode, string countryId, string customAttributes)
        {
            return source.Find(a => ((String.IsNullOrEmpty(a.FirstName) && String.IsNullOrEmpty(firstName)) || a.FirstName == firstName) &&
                ((String.IsNullOrEmpty(a.LastName) && String.IsNullOrEmpty(lastName)) || a.LastName == lastName) &&
                ((String.IsNullOrEmpty(a.PhoneNumber) && String.IsNullOrEmpty(phoneNumber)) || a.PhoneNumber == phoneNumber) &&
                ((String.IsNullOrEmpty(a.Email) && String.IsNullOrEmpty(email)) || a.Email == email) &&
                ((String.IsNullOrEmpty(a.FaxNumber) && String.IsNullOrEmpty(faxNumber)) || a.FaxNumber == faxNumber) &&
                ((String.IsNullOrEmpty(a.Company) && String.IsNullOrEmpty(company)) || a.Company == company) &&
                ((String.IsNullOrEmpty(a.Address1) && String.IsNullOrEmpty(address1)) || a.Address1 == address1) &&
                ((String.IsNullOrEmpty(a.Address2) && String.IsNullOrEmpty(address2)) || a.Address2 == address2) &&
                ((String.IsNullOrEmpty(a.City) && String.IsNullOrEmpty(city)) || a.City == city) &&
                ((String.IsNullOrEmpty(a.StateProvinceId) && String.IsNullOrEmpty(stateProvinceId)) || a.StateProvinceId == stateProvinceId) &&
                ((String.IsNullOrEmpty(a.ZipPostalCode) && String.IsNullOrEmpty(zipPostalCode)) || a.ZipPostalCode == zipPostalCode) &&
                ((String.IsNullOrEmpty(a.CountryId) && String.IsNullOrEmpty(countryId)) || a.CountryId == countryId) &&
                ((String.IsNullOrEmpty(a.CustomAttributes) && String.IsNullOrEmpty(customAttributes)) || a.CustomAttributes == customAttributes));
        }

    }
}
