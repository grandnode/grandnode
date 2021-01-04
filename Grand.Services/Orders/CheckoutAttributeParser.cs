using Grand.Domain.Common;
using Grand.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Orders
{
    /// <summary>
    /// Checkout attribute parser
    /// </summary>
    public partial class CheckoutAttributeParser : ICheckoutAttributeParser
    {
        private readonly ICheckoutAttributeService _checkoutAttributeService;

        public CheckoutAttributeParser(ICheckoutAttributeService checkoutAttributeService)
        {
            _checkoutAttributeService = checkoutAttributeService;
        }

        /// <summary>
        /// Gets selected checkout attributes
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <returns>Selected checkout attributes</returns>
        public virtual async Task<IList<CheckoutAttribute>> ParseCheckoutAttributes(IList<CustomAttribute> customAttributes)
        {
            var result = new List<CheckoutAttribute>();
            if (customAttributes == null || !customAttributes.Any())
                return result;

            foreach (var customAttribute in customAttributes.GroupBy(x => x.Key))
            {
                var attribute = await _checkoutAttributeService.GetCheckoutAttributeById(customAttribute.Key);
                if (attribute != null)
                {
                    result.Add(attribute);
                }
            }
            return result;
        }

        /// <summary>
        /// Get checkout attribute values
        /// </summary>
        /// <param name="customAttributes">Attributes</param>
        /// <returns>Checkout attribute values</returns>
        public virtual async Task<IList<CheckoutAttributeValue>> ParseCheckoutAttributeValues(IList<CustomAttribute> customAttributes)
        {
            var values = new List<CheckoutAttributeValue>();
            if (customAttributes == null || !customAttributes.Any())
                return values;

            var attributes = await ParseCheckoutAttributes(customAttributes);
            foreach (var attribute in attributes)
            {
                if (!attribute.ShouldHaveValues())
                    continue;

                var valuesStr = customAttributes.Where(x => x.Key == attribute.Id).Select(x => x.Value);
                foreach (var valueStr in valuesStr)
                {
                    if (!string.IsNullOrEmpty(valueStr))
                    {
                        var value = attribute.CheckoutAttributeValues.Where(x => x.Id == valueStr).FirstOrDefault();
                        if (value != null)
                            values.Add(value);
                    }
                }
            }
            return values;
        }

        /// <summary>
        /// Get checkout attribute values with checkout attribute 
        /// </summary>
        /// <param name="customAttributes">Attributes</param>
        /// <returns>Checkout attribute values with checkout attribute </returns>
        public virtual async Task<IList<(CheckoutAttribute ca, CheckoutAttributeValue cav)>> ParseCheckoutAttributeValue(IList<CustomAttribute> customAttributes)
        {
            var values = new List<(CheckoutAttribute ca, CheckoutAttributeValue cav)>();
            if (customAttributes == null || !customAttributes.Any())
                return values;

            var attributes = await ParseCheckoutAttributes(customAttributes);
            foreach (var attribute in attributes)
            {
                if (!attribute.ShouldHaveValues())
                    continue;

                var valuesStr = customAttributes.Where(x => x.Key == attribute.Id).Select(x => x.Value);
                foreach (var valueStr in valuesStr)
                {
                    if (!string.IsNullOrEmpty(valueStr))
                    {
                        var value = attribute.CheckoutAttributeValues.Where(x => x.Id == valueStr).FirstOrDefault();
                        if (value != null)
                            values.Add((attribute, value));
                    }
                }
            }
            return values;
        }

        /// <summary>
        /// Adds an attribute
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <param name="ca">Checkout attribute</param>
        /// <param name="value">Value</param>
        /// <returns>Attributes</returns>
        public virtual IList<CustomAttribute> AddCheckoutAttribute(IList<CustomAttribute> customAttributes, CheckoutAttribute ca, string value)
        {
            if (customAttributes == null)
                customAttributes = new List<CustomAttribute>();

            customAttributes.Add(new CustomAttribute() { Key = ca.Id, Value = value });
            return customAttributes;
        }

        /// <summary>
        /// Removes checkout attributes which cannot be applied to the current cart and returns an update attributes 
        /// </summary>
        /// <param name="customAttributes">Attributes</param>
        /// <param name="cart">Shopping cart items</param>
        /// <returns>Updated attributes</returns>
        public virtual async Task<IList<CustomAttribute>> EnsureOnlyActiveAttributes(IList<CustomAttribute> customAttributes, IList<ShoppingCartItem> cart)
        {
            if (customAttributes == null)
                customAttributes = new List<CustomAttribute>();

            if (!cart.RequiresShipping())
            {
                //find attribute IDs to remove
                var checkoutAttributeIdsToRemove = new List<string>();
                var attributes = await ParseCheckoutAttributes(customAttributes);
                foreach (var ca in attributes)
                    if (ca.ShippableProductRequired)
                        checkoutAttributeIdsToRemove.Add(ca.Id);

                foreach (var id in checkoutAttributeIdsToRemove)
                {
                    var attr = customAttributes.FirstOrDefault(x => x.Key == id);
                    if (attr != null)
                        customAttributes.Remove(attr);
                }
            }

            return customAttributes;
        }
        /// <summary>
        /// Check whether condition of some attribute is met (if specified). Return "null" if not condition is specified
        /// </summary>
        /// <param name="attribute">Checkout attribute</param>
        /// <param name="selectedAttributes">Selected attributes</param>
        /// <returns>Result</returns>
        public virtual async Task<bool?> IsConditionMet(CheckoutAttribute attribute, IList<CustomAttribute> customAttributes)
        {
            if (attribute == null)
                throw new ArgumentNullException("attribute");

            if (customAttributes == null)
                customAttributes = new List<CustomAttribute>();

            var conditionAttribute = attribute.ConditionAttribute;
            if (!conditionAttribute.Any())
                //no condition
                return null;

            //load an attribute this one depends on
            var dependOnAttribute = (await ParseCheckoutAttributes(conditionAttribute)).FirstOrDefault();
            if (dependOnAttribute == null)
                return true;

            var valuesThatShouldBeSelected = conditionAttribute.Where(x => x.Key == dependOnAttribute.Id).Select(x => x.Value)
                //a workaround here:
                //ConditionAttributeXml can contain "empty" values (nothing is selected)
                //but in other cases (like below) we do not store empty values
                //that's why we remove empty values here
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            var selectedValues = customAttributes.Where(x => x.Key == dependOnAttribute.Id).Select(x => x.Value).ToList();
            if (valuesThatShouldBeSelected.Count != selectedValues.Count)
                return false;

            //compare values
            var allFound = true;
            foreach (var t1 in valuesThatShouldBeSelected)
            {
                bool found = false;
                foreach (var t2 in selectedValues)
                    if (t1 == t2)
                        found = true;
                if (!found)
                    allFound = false;
            }

            return allFound;
        }

        /// <summary>
        /// Remove an attribute
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <param name="attribute">Checkout attribute</param>
        /// <returns>Updated</returns>
        public virtual IList<CustomAttribute> RemoveCheckoutAttribute(IList<CustomAttribute> customAttributes, CheckoutAttribute attribute)
        {
            return customAttributes.Where(x => x.Key != attribute.Id).ToList();
        }

    }
}
