using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class TierPriceExtensions
    {
        /// <summary>
        /// Filter tier prices by a store
        /// </summary>
        /// <param name="source">Tier prices</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Filtered tier prices</returns>
        public static IEnumerable<TierPrice> FilterByStore(this IEnumerable<TierPrice> source, string storeId)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return source.Where(tierPrice => string.IsNullOrEmpty(tierPrice.StoreId) || tierPrice.StoreId == storeId);
        }

        /// <summary>
        /// Filter tier prices for a customer
        /// </summary>
        /// <param name="source">Tier prices</param>
        /// <param name="customer">Customer</param>
        /// <returns>Filtered tier prices</returns>
        public static IEnumerable<TierPrice> FilterForCustomer(this IEnumerable<TierPrice> source, Customer customer)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (customer == null)
                return source.Where(tierPrice => string.IsNullOrEmpty(tierPrice.CustomerRoleId));

            return source.Where(tierPrice => string.IsNullOrEmpty(tierPrice.CustomerRoleId) ||
                customer.CustomerRoles.Where(role => role.Active).Select(role => role.Id).Contains(tierPrice.CustomerRoleId));
        }

        /// <summary>
        /// Filter tier prices by a currency
        /// </summary>
        /// <param name="source">Tier prices</param>
        /// <param name="currencyCode">currencyCode</param>
        /// <returns>Filtered tier prices</returns>
        public static IEnumerable<TierPrice> FilterByCurrency(this IEnumerable<TierPrice> source, string currencyCode)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return source.Where(tierPrice => string.IsNullOrEmpty(tierPrice.CurrencyCode) || tierPrice.CurrencyCode == currencyCode);
        }

        /// <summary>
        /// Remove duplicated quantities (leave only an tier price with minimum price)
        /// </summary>
        /// <param name="source">Tier prices</param>
        /// <returns>Filtered tier prices</returns>
        public static IEnumerable<TierPrice> RemoveDuplicatedQuantities(this IEnumerable<TierPrice> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            //get group of tier prices with the same quantity
            var tierPricesWithDuplicates = source.GroupBy(tierPrice => tierPrice.Quantity).Where(group => group.Count() > 1);

            //get tier prices with higher prices 
            var duplicatedPrices = tierPricesWithDuplicates.SelectMany(group =>
            {
                //find minimal price for quantity
                var minTierPrice = group.Aggregate((currentMinTierPrice, nextTierPrice) =>
                    (currentMinTierPrice.Price < nextTierPrice.Price ? currentMinTierPrice : nextTierPrice));

                //and return all other with higher price
                return group.Where(tierPrice => tierPrice.Id != minTierPrice.Id);
            });

            //return tier prices without duplicates
            return source.Where(tierPrice => !duplicatedPrices.Any(duplicatedPrice => duplicatedPrice.Id == tierPrice.Id));
        }

        /// <summary>
        /// Filter tier prices by date
        /// </summary>
        /// <param name="source">Tier prices</param>
        /// <param name="date">Date in UTC; pass null to filter by current date</param>
        /// <returns>Filtered tier prices</returns>
        public static IEnumerable<TierPrice> FilterByDate(this IEnumerable<TierPrice> source, DateTime? date = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (!date.HasValue)
                date = DateTime.UtcNow;

            return source.Where(tierPrice =>
                (!tierPrice.StartDateTimeUtc.HasValue || tierPrice.StartDateTimeUtc.Value < date.Value) &&
                (!tierPrice.EndDateTimeUtc.HasValue || tierPrice.EndDateTimeUtc.Value > date.Value));
        }
    }
}
