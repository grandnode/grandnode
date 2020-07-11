using Grand.Domain;
using Grand.Domain.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Product attribute service interface
    /// </summary>
    public partial interface IProductAttributeService
    {
        #region Product attributes

        /// <summary>
        /// Deletes a product attribute
        /// </summary>
        /// <param name="productAttribute">Product attribute</param>
        Task DeleteProductAttribute(ProductAttribute productAttribute);

        /// <summary>
        /// Gets all product attributes
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Product attributes</returns>
        Task<IPagedList<ProductAttribute>> GetAllProductAttributes(int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets a product attribute 
        /// </summary>
        /// <param name="productAttributeId">Product attribute identifier</param>
        /// <returns>Product attribute </returns>
        Task<ProductAttribute> GetProductAttributeById(string productAttributeId);

        /// <summary>
        /// Inserts a product attribute
        /// </summary>
        /// <param name="productAttribute">Product attribute</param>
        Task InsertProductAttribute(ProductAttribute productAttribute);

        /// <summary>
        /// Updates the product attribute
        /// </summary>
        /// <param name="productAttribute">Product attribute</param>
        Task UpdateProductAttribute(ProductAttribute productAttribute);

        #endregion

        #region Product attributes mappings

        /// <summary>
        /// Deletes a product attribute mapping
        /// </summary>
        /// <param name="productAttributeMapping">Product attribute mapping</param>
        Task DeleteProductAttributeMapping(ProductAttributeMapping productAttributeMapping);

        /// <summary>
        /// Inserts a product attribute mapping
        /// </summary>
        /// <param name="productAttributeMapping">The product attribute mapping</param>
        Task InsertProductAttributeMapping(ProductAttributeMapping productAttributeMapping);

        /// <summary>
        /// Updates the product attribute mapping
        /// </summary>
        /// <param name="productAttributeMapping">The product attribute mapping</param>
        /// <param name="values">Update values</param>
        Task UpdateProductAttributeMapping(ProductAttributeMapping productAttributeMapping, bool values = false);

        #endregion

        #region Product attribute values

        /// <summary>
        /// Deletes a product attribute value
        /// </summary>
        /// <param name="productAttributeValue">Product attribute value</param>
        Task DeleteProductAttributeValue(ProductAttributeValue productAttributeValue);

        /// <summary>
        /// Inserts a product attribute value
        /// </summary>
        /// <param name="productAttributeValue">The product attribute value</param>
        Task InsertProductAttributeValue(ProductAttributeValue productAttributeValue);

        /// <summary>
        /// Updates the product attribute value
        /// </summary>
        /// <param name="productAttributeValue">The product attribute value</param>
        Task UpdateProductAttributeValue(ProductAttributeValue productAttributeValue);

        #endregion

        #region Predefined product attribute values

        /// <summary>
        /// Gets predefined product attribute values by product attribute identifier
        /// </summary>
        /// <param name="productAttributeId">The product attribute identifier</param>
        /// <returns>Product attribute mapping collection</returns>
        Task<IList<PredefinedProductAttributeValue>> GetPredefinedProductAttributeValues(string productAttributeId);


        #endregion

        #region Product attribute combinations

        /// <summary>
        /// Deletes a product attribute combination
        /// </summary>
        /// <param name="combination">Product attribute combination</param>
        Task DeleteProductAttributeCombination(ProductAttributeCombination combination);

        /// <summary>
        /// Inserts a product attribute combination
        /// </summary>
        /// <param name="combination">Product attribute combination</param>
        Task InsertProductAttributeCombination(ProductAttributeCombination combination);

        /// <summary>
        /// Updates a product attribute combination
        /// </summary>
        /// <param name="combination">Product attribute combination</param>
        Task UpdateProductAttributeCombination(ProductAttributeCombination combination);

        #endregion
    }
}
