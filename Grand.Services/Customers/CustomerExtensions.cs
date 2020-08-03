using Grand.Core;
using Grand.Core.Html;
using Grand.Domain.Customers;
using Grand.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Customers
{
    public static class CustomerExtensions
    {
        public static string CouponSeparator => ";";

        /// <summary>
        /// Get full name
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Customer full name</returns>
        public static string GetFullName(this Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");
            var firstName = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.FirstName);
            var lastName = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LastName);

            string fullName = "";
            if (!String.IsNullOrWhiteSpace(firstName) && !String.IsNullOrWhiteSpace(lastName))
                fullName = string.Format("{0} {1}", firstName, lastName);
            else
            {
                if (!String.IsNullOrWhiteSpace(firstName))
                    fullName = firstName;

                if (!String.IsNullOrWhiteSpace(lastName))
                    fullName = lastName;
            }
            return fullName;
        }
        /// <summary>
        /// Formats the customer name
        /// </summary>
        /// <param name="customer">Source</param>
        /// <param name="stripTooLong">Strip too long customer name</param>
        /// <param name="maxLength">Maximum customer name length</param>
        /// <returns>Formatted text</returns>
        public static string FormatUserName(this Customer customer, CustomerNameFormat customerNameFormat, bool stripTooLong = false, int maxLength = 0)
        {
            if (customer == null)
                return string.Empty;

            if (customer.IsGuest())
            {
                return "Customer.Guest";
            }

            string result = string.Empty;
            switch (customerNameFormat)
            {
                case CustomerNameFormat.ShowEmails:
                    result = customer.Email;
                    break;
                case CustomerNameFormat.ShowUsernames:
                    result = customer.Username;
                    break;
                case CustomerNameFormat.ShowFullNames:
                    result = customer.GetFullName();
                    break;
                case CustomerNameFormat.ShowFirstName:
                    result = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.FirstName);
                    break;
                default:
                    break;
            }

            if (stripTooLong && maxLength > 0)
            {
                result = CommonHelper.EnsureMaximumLength(result, maxLength);
            }

            return result;
        }

        /// <summary>
        /// Gets coupon codes
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Coupon codes</returns>
        public static string[] ParseAppliedCouponCodes(this Customer customer, string key)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var existingCouponCodes = customer.GetAttributeFromEntity<string>(key);

            var couponCodes = new List<string>();
            if (string.IsNullOrEmpty(existingCouponCodes))
                return couponCodes.ToArray();

            return existingCouponCodes.Split(CouponSeparator);

        }

        /// <summary>
        /// Adds a coupon code
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="couponCode">Coupon code</param>
        /// <returns>New coupon codes document</returns>
        public static string ApplyCouponCode(this Customer customer, string key, string couponCode)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var existingCouponCodes = customer.GetAttributeFromEntity<string>(key);
            if (string.IsNullOrEmpty(existingCouponCodes))
            {
                return couponCode;
            }
            else
            {
                return string.Join(CouponSeparator, existingCouponCodes.Split(CouponSeparator).Append(couponCode).Distinct());
            }
        }
        /// <summary>
        /// Adds a coupon codes
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="couponCode">Coupon code</param>
        /// <returns>New coupon codes document</returns>
        public static string ApplyCouponCode(this Customer customer, string key, string[] couponCodes)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var existingCouponCodes = customer.GetAttributeFromEntity<string>(key);
            if (string.IsNullOrEmpty(existingCouponCodes))
            {
                return string.Join(CouponSeparator, couponCodes);
            }
            else
            {
                var coupons = existingCouponCodes.Split(CouponSeparator).ToList();
                coupons.AddRange(couponCodes.ToList());
                return string.Join(CouponSeparator, coupons.Distinct());
            }
        }
        /// <summary>
        /// Adds a coupon code
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="couponCode">Coupon code</param>
        /// <returns>New coupon codes document</returns>
        public static string RemoveCouponCode(this Customer customer, string key, string couponCode)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var existingCouponCodes = customer.GetAttributeFromEntity<string>(key);
            if (string.IsNullOrEmpty(existingCouponCodes))
            {
                return "";
            }
            else
            {
                return string.Join(CouponSeparator, existingCouponCodes.Split(CouponSeparator).Except(new List<string> { couponCode }).Distinct());
            }
        }

        /// <summary>
        /// Check whether password recovery token is valid
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="token">Token to validate</param>
        /// <returns>Result</returns>
        public static bool IsPasswordRecoveryTokenValid(this Customer customer, string token)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var cPrt = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.PasswordRecoveryToken);
            if (String.IsNullOrEmpty(cPrt))
                return false;

            if (!cPrt.Equals(token, StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        /// <summary>
        /// Check whether password recovery link is expired
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="customerSettings">Customer settings</param>
        /// <returns>Result</returns>
        public static bool IsPasswordRecoveryLinkExpired(this Customer customer, CustomerSettings customerSettings)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (customerSettings == null)
                throw new ArgumentNullException("customerSettings");

            if (customerSettings.PasswordRecoveryLinkDaysValid == 0)
                return false;

            var geneatedDate = customer.GetAttributeFromEntity<DateTime?>(SystemCustomerAttributeNames.PasswordRecoveryTokenDateGenerated);
            if (!geneatedDate.HasValue)
                return false;

            var daysPassed = (DateTime.UtcNow - geneatedDate.Value).TotalDays;
            if (daysPassed > customerSettings.PasswordRecoveryLinkDaysValid)
                return true;

            return false;
        }

        /// <summary>
        /// Get customer role identifiers
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>Customer role identifiers</returns>
        public static string[] GetCustomerRoleIds(this Customer customer, bool showHidden = false)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var customerRolesIds = customer.CustomerRoles
               .Where(cr => showHidden || cr.Active)
               .Select(cr => cr.Id)
               .ToArray();

            return customerRolesIds;
        }

        /// <summary>
        /// Check whether customer password is expired 
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>True if password is expired; otherwise false</returns>
        public static bool PasswordIsExpired(this Customer customer, CustomerSettings customerSettings)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            //the guests don't have a password
            if (customer.IsGuest())
                return false;

            //password lifetime is disabled for user
            if (!customer.CustomerRoles.Any(role => role.Active && role.EnablePasswordLifetime))
                return false;

            //setting disabled for all
            if (customerSettings.PasswordLifetime == 0)
                return false;

            var currentLifetime = 0;
            if (!customer.PasswordChangeDateUtc.HasValue)
                currentLifetime = int.MaxValue;
            else
                currentLifetime = (DateTime.UtcNow - customer.PasswordChangeDateUtc.Value).Days;

            return currentLifetime >= customerSettings.PasswordLifetime;
        }

        /// <summary>
        /// Formats the customer note text
        /// </summary>
        /// <param name="customerNote">Customer note</param>
        /// <returns>Formatted text</returns>
        public static string FormatCustomerNoteText(this CustomerNote customerNote)
        {
            if (customerNote == null)
                throw new ArgumentNullException("customerNote");

            string text = customerNote.Note;

            if (String.IsNullOrEmpty(text))
                return string.Empty;

            text = HtmlHelper.FormatText(text, false, true, false, false, false, false);

            return text;
        }
    }
}
