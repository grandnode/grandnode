using Grand.Domain.Shipping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Shipping
{
    public static class ShippingExtensions
    {
        public static bool IsShippingRateComputationMethodActive(this IShippingRateComputationMethod srcm,
            ShippingSettings shippingSettings)
        {
            if (srcm == null)
                throw new ArgumentNullException("srcm");

            if (shippingSettings == null)
                throw new ArgumentNullException("shippingSettings");

            if (shippingSettings.ActiveShippingRateComputationMethodSystemNames == null)
                return false;
            foreach (string activeMethodSystemName in shippingSettings.ActiveShippingRateComputationMethodSystemNames)
                if (srcm.PluginDescriptor.SystemName.Equals(activeMethodSystemName, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        public static bool CountryRestrictionExists(this ShippingMethod shippingMethod,
            string countryId)
        {
            if (shippingMethod == null)
                throw new ArgumentNullException("shippingMethod");

            bool result = shippingMethod.RestrictedCountries.ToList().Find(c => c.Id == countryId) != null;
            return result;
        }
        public static bool CustomerRoleRestrictionExists(this ShippingMethod shippingMethod,
           string roleId)
        {
            if (shippingMethod == null)
                throw new ArgumentNullException("shippingMethod");

            bool result = shippingMethod.RestrictedRoles.ToList().Find(c => c == roleId) != null;
            return result;
        }

        public static bool CustomerRoleRestrictionExists(this ShippingMethod shippingMethod,
           List<string> roleIds)
        {
            if (shippingMethod == null)
                throw new ArgumentNullException("shippingMethod");

            bool result = shippingMethod.RestrictedRoles.ToList().Find(c => roleIds.Contains(c)) != null;
            return result;
        }
    }
}
