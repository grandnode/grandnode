using System.Collections.Generic;
using Grand.Core.Domain.Catalog;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Product template interface
    /// </summary>
    public partial interface IProductTemplateService
    {
        /// <summary>
        /// Delete product template
        /// </summary>
        /// <param name="productTemplate">Product template</param>
        void DeleteProductTemplate(ProductTemplate productTemplate);

        /// <summary>
        /// Gets all product templates
        /// </summary>
        /// <returns>Product templates</returns>
        IList<ProductTemplate> GetAllProductTemplates();

        /// <summary>
        /// Gets a product template
        /// </summary>
        /// <param name="productTemplateId">Product template identifier</param>
        /// <returns>Product template</returns>
        ProductTemplate GetProductTemplateById(string productTemplateId);

        /// <summary>
        /// Inserts product template
        /// </summary>
        /// <param name="productTemplate">Product template</param>
        void InsertProductTemplate(ProductTemplate productTemplate);

        /// <summary>
        /// Updates the product template
        /// </summary>
        /// <param name="productTemplate">Product template</param>
        void UpdateProductTemplate(ProductTemplate productTemplate);
    }
}
