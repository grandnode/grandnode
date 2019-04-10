using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Core.Html;
using Grand.Services.Common;
using Grand.Services.Orders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
namespace Grand.Services.Customers
{
    public static class CustomerExtensions
    {
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
        public static async Task<string[]> ParseAppliedDiscountCouponCodes(this Customer customer, IGenericAttributeService genericAttributeService)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var existingCouponCodes = await customer.GetAttribute<string>(genericAttributeService, SystemCustomerAttributeNames.DiscountCouponCode);

            var couponCodes = new List<string>();
            if (String.IsNullOrEmpty(existingCouponCodes))
                return couponCodes.ToArray();

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(existingCouponCodes);

                var nodeList1 = xmlDoc.SelectNodes(@"//DiscountCouponCodes/CouponCode");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["Code"] != null)
                    {
                        string code = node1.Attributes["Code"].InnerText.Trim();
                        couponCodes.Add(code.ToUpper());
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }
            return couponCodes.ToArray();
        }
        /// <summary>
        /// Adds a coupon code
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="couponCode">Coupon code</param>
        /// <returns>New coupon codes document</returns>
        public static async Task ApplyDiscountCouponCode(this Customer customer, IGenericAttributeService genericAttributeService, string couponCode)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            string result = string.Empty;
            try
            {
                var existingCouponCodes = await customer.GetAttribute<string>(genericAttributeService, SystemCustomerAttributeNames.DiscountCouponCode);

                couponCode = couponCode.Trim().ToLower();

                var xmlDoc = new XmlDocument();
                if (String.IsNullOrEmpty(existingCouponCodes))
                {
                    var element1 = xmlDoc.CreateElement("DiscountCouponCodes");
                    xmlDoc.AppendChild(element1);
                }
                else
                {
                    xmlDoc.LoadXml(existingCouponCodes);
                }
                var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//DiscountCouponCodes");

                XmlElement gcElement = null;
                //find existing
                var nodeList1 = xmlDoc.SelectNodes(@"//DiscountCouponCodes/CouponCode");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["Code"] != null)
                    {
                        string couponCodeAttribute = node1.Attributes["Code"].InnerText.Trim();
                        if (couponCodeAttribute.ToLower() == couponCode.ToLower())
                        {
                            gcElement = (XmlElement)node1;
                            break;
                        }
                    }
                }

                //create new one if not found
                if (gcElement == null)
                {
                    gcElement = xmlDoc.CreateElement("CouponCode");
                    gcElement.SetAttribute("Code", couponCode);
                    rootElement.AppendChild(gcElement);
                }

                result = xmlDoc.OuterXml;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            //apply new value
            await genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DiscountCouponCode, result);
        }
        /// <summary>
        /// Removes a coupon code
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="couponCode">Coupon code to remove</param>
        /// <returns>New coupon codes document</returns>
        public static async Task RemoveDiscountCouponCode(this Customer customer, IGenericAttributeService genericAttributeService, string couponCode)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            //get applied coupon codes
            var existingCouponCodes = await customer.ParseAppliedDiscountCouponCodes(genericAttributeService);

            //clear them
            await genericAttributeService.SaveAttribute<string>(customer, SystemCustomerAttributeNames.DiscountCouponCode, null);

            //save again except removed one
            foreach (string existingCouponCode in existingCouponCodes)
                if (!existingCouponCode.Equals(couponCode, StringComparison.OrdinalIgnoreCase))
                    await customer.ApplyDiscountCouponCode(genericAttributeService, existingCouponCode);
        }


        /// <summary>
        /// Gets coupon codes
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Coupon codes</returns>
        public static async Task<string[]> ParseAppliedGiftCardCouponCodes(this Customer customer, IGenericAttributeService genericAttributeService)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var existingCouponCodes = await customer.GetAttribute<string>(genericAttributeService, SystemCustomerAttributeNames.GiftCardCouponCodes);

            var couponCodes = new List<string>();
            if (String.IsNullOrEmpty(existingCouponCodes))
                return couponCodes.ToArray();

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(existingCouponCodes);

                var nodeList1 = xmlDoc.SelectNodes(@"//GiftCardCouponCodes/CouponCode");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["Code"] != null)
                    {
                        string code = node1.Attributes["Code"].InnerText.Trim();
                        couponCodes.Add(code);
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }
            return couponCodes.ToArray();
        }
        /// <summary>
        /// Get active gift cards that are applied by a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Active gift cards</returns>
        public static async Task<IList<GiftCard>> GetActiveGiftCardsAppliedByCustomer(this Customer customer, IGiftCardService giftCardService, IGenericAttributeService genericAttributeService)
        {
            var result = new List<GiftCard>();
            if (customer == null)
                return result;

            string[] couponCodes = await customer.ParseAppliedGiftCardCouponCodes(genericAttributeService);
            foreach (var couponCode in couponCodes)
            {
                var giftCards = await giftCardService.GetAllGiftCards(isGiftCardActivated: true, giftCardCouponCode: couponCode);
                foreach (var gc in giftCards)
                {
                    if (gc.IsGiftCardValid())
                        result.Add(gc);
                }
            }

            return result;
        }


        /// <summary>
        /// Adds a coupon code
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="couponCode">Coupon code</param>
        /// <returns>New coupon codes document</returns>
        public static async Task ApplyGiftCardCouponCode(this Customer customer, IGenericAttributeService genericAttributeService, string couponCode)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            string result = string.Empty;
            try
            {
                var existingCouponCodes = await customer.GetAttribute<string>(genericAttributeService, SystemCustomerAttributeNames.GiftCardCouponCodes);

                couponCode = couponCode.Trim().ToLower();

                var xmlDoc = new XmlDocument();
                if (String.IsNullOrEmpty(existingCouponCodes))
                {
                    var element1 = xmlDoc.CreateElement("GiftCardCouponCodes");
                    xmlDoc.AppendChild(element1);
                }
                else
                {
                    xmlDoc.LoadXml(existingCouponCodes);
                }
                var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//GiftCardCouponCodes");

                XmlElement gcElement = null;
                //find existing
                var nodeList1 = xmlDoc.SelectNodes(@"//GiftCardCouponCodes/CouponCode");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["Code"] != null)
                    {
                        string couponCodeAttribute = node1.Attributes["Code"].InnerText.Trim();
                        if (couponCodeAttribute.ToLower() == couponCode.ToLower())
                        {
                            gcElement = (XmlElement)node1;
                            break;
                        }
                    }
                }

                //create new one if not found
                if (gcElement == null)
                {
                    gcElement = xmlDoc.CreateElement("CouponCode");
                    gcElement.SetAttribute("Code", couponCode);
                    rootElement.AppendChild(gcElement);
                }

                result = xmlDoc.OuterXml;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            //apply new value
            await genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.GiftCardCouponCodes, result);
        }
        /// <summary>
        /// Removes a coupon code
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="couponCode">Coupon code to remove</param>
        /// <returns>New coupon codes document</returns>
        public static async Task RemoveGiftCardCouponCode(this Customer customer, IGenericAttributeService genericAttributeService, string couponCode)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            //get applied coupon codes
            var existingCouponCodes = await customer.ParseAppliedGiftCardCouponCodes(genericAttributeService);

            //clear them
            await genericAttributeService.SaveAttribute<string>(customer, SystemCustomerAttributeNames.GiftCardCouponCodes, null);

            //save again except removed one
            foreach (string existingCouponCode in existingCouponCodes)
                if (!existingCouponCode.Equals(couponCode, StringComparison.OrdinalIgnoreCase))
                    await customer.ApplyGiftCardCouponCode(genericAttributeService, existingCouponCode);
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
