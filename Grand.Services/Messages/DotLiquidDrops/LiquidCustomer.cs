using DotLiquid;
using Grand.Domain.Customers;
using Grand.Domain.Stores;
using Grand.Domain.Tax;
using Grand.Services.Common;
using Grand.Services.Customers;
using System.Collections.Generic;
using System.Net;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidCustomer : Drop
    {
        private Customer _customer;
        private CustomerNote _customerNote;
        private Store _store;

        public LiquidCustomer(Customer customer, Store store, CustomerNote customerNote = null)
        {
            _customer = customer;
            _customerNote = customerNote;
            _store = store;
            AdditionalTokens = new Dictionary<string, string>();
        }

        public string Email
        {
            get { return _customer.Email; }
        }

        public string Username
        {
            get { return _customer.Username; }
        }

        public string FullName
        {
            get { return _customer.GetFullName(); }
        }

        public string FirstName
        {
            get { return _customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.FirstName); }
        }

        public string LastName
        {
            get { return _customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LastName); }
        }

        public string Gender 
        {
            get { return _customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Gender); }
        }

        public string DateOfBirth 
        {
            get { return _customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.DateOfBirth); }
        }        

        public string Company 
        {
            get { return _customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Company); }
        }

        public string StreetAddress 
        {
            get { return _customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StreetAddress); }
        }

        public string StreetAddress2 
        {
            get { return _customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.StreetAddress2); }
        }

        public string ZipPostalCode 
        {
            get { return _customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.ZipPostalCode); }
        }

        public string City 
        {
            get { return _customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.City); }
        }

        public string Phone 
        {
            get { return _customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Phone); }
        }

        public string Fax {
            get { return _customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Fax); }
        }

        public string VatNumber
        {
            get { return _customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.VatNumber); }
        }

        public string VatNumberStatus
        {
            get { return ((VatNumberStatus)_customer.GetAttributeFromEntity<int>(SystemCustomerAttributeNames.VatNumberStatusId)).ToString(); }
        }

        public string PasswordRecoveryURL
        {
            get { return string.Format("{0}passwordrecovery/confirm?token={1}&email={2}", (_store.SslEnabled ? _store.SecureUrl : _store.Url), _customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.PasswordRecoveryToken), WebUtility.UrlEncode(_customer.Email)); }
        }

        public string AccountActivationURL
        {
            get { return string.Format("{0}customer/activation?token={1}&email={2}", (_store.SslEnabled ? _store.SecureUrl : _store.Url), _customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.AccountActivationToken), WebUtility.UrlEncode(_customer.Email)); ; }
        }

        public string WishlistURLForCustomer
        {
            get { return string.Format("{0}wishlist/{1}", (_store.SslEnabled ? _store.SecureUrl : _store.Url), _customer.CustomerGuid); }
        }

        public string NewNoteText
        {
            get { return _customerNote.FormatCustomerNoteText(); }
        }

        public string NewTitleText
        {
            get { return _customerNote.Title; }
        }

        public string CustomerNoteAttachmentUrl
        {
            get { return string.Format("{0}download/customernotefile/{1}", (_store.SslEnabled ? _store.SecureUrl : _store.Url), _customerNote.Id); }
        }

        public string Token {
            get { return _customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.TwoFactorValidCode); }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}