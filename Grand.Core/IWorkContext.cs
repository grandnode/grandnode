using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Localization;
using Grand.Domain.Tax;
using Grand.Domain.Vendors;
using System.Threading.Tasks;

namespace Grand.Core
{
    /// <summary>
    /// Work context
    /// </summary>
    public interface IWorkContext
    {
        /// <summary>
        /// Gets the current customer
        /// </summary>
        Customer CurrentCustomer { get; }

        /// <summary>
        /// Set the current customer by Middleware
        /// </summary>
        /// <returns></returns>
        Task<Customer> SetCurrentCustomer();

        /// <summary>
        /// Gets or sets the original customer (in case the current one is impersonated)
        /// </summary>
        Customer OriginalCustomerIfImpersonated { get; }

        /// <summary>
        /// Gets the current vendor (logged-in manager)
        /// </summary>
        Vendor CurrentVendor { get; }

        /// <summary>
        /// Set the current vendor (logged-in manager)
        /// </summary>
        Task<Vendor> SetCurrentVendor(Customer customer);

        /// <summary>
        /// Get or set current user working language
        /// </summary>
        Language WorkingLanguage { get; }

        /// <summary>
        /// Set current user working language by Middleware
        /// </summary>
        Task<Language> SetWorkingLanguage(Customer customer);

        /// <summary>
        /// Set current user working language
        /// </summary>
        Task<Language> SetWorkingLanguage(Language language);

        /// <summary>
        /// Get or set current user working currency
        /// </summary>
        Currency WorkingCurrency { get; }

        /// <summary>
        /// Set current user working currency by Middleware
        /// </summary>
        Task<Currency> SetWorkingCurrency(Customer customer);

        /// <summary>
        /// Set user working currency
        /// </summary>
        Task<Currency> SetWorkingCurrency(Currency currency);

        /// <summary>
        /// Get current tax display type
        /// </summary>
        TaxDisplayType TaxDisplayType { get; }
        
        /// <summary>
        /// Set current tax display type by Middleware
        /// </summary>
        Task<TaxDisplayType> SetTaxDisplayType(Customer customer);

        /// <summary>
        /// Set tax display type 
        /// </summary>
        Task<TaxDisplayType> SetTaxDisplayType(TaxDisplayType taxDisplayType);

    }
}
