using System.Collections.Generic;
using Grand.Core.Domain.Tax;

namespace Grand.Services.Tax
{
    /// <summary>
    /// Tax category service interface
    /// </summary>
    public partial interface ITaxCategoryService
    {
        /// <summary>
        /// Deletes a tax category
        /// </summary>
        /// <param name="taxCategory">Tax category</param>
        void DeleteTaxCategory(TaxCategory taxCategory);

        /// <summary>
        /// Gets all tax categories
        /// </summary>
        /// <returns>Tax categories</returns>
        IList<TaxCategory> GetAllTaxCategories();

        /// <summary>
        /// Gets a tax category
        /// </summary>
        /// <param name="taxCategoryId">Tax category identifier</param>
        /// <returns>Tax category</returns>
        TaxCategory GetTaxCategoryById(string taxCategoryId);

        /// <summary>
        /// Inserts a tax category
        /// </summary>
        /// <param name="taxCategory">Tax category</param>
        void InsertTaxCategory(TaxCategory taxCategory);

        /// <summary>
        /// Updates the tax category
        /// </summary>
        /// <param name="taxCategory">Tax category</param>
        void UpdateTaxCategory(TaxCategory taxCategory);
    }
}
