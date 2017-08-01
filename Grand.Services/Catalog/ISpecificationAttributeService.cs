using Grand.Core;
using Grand.Core.Domain.Catalog;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Specification attribute service interface
    /// </summary>
    public partial interface ISpecificationAttributeService
    {
        #region Specification attribute

        /// <summary>
        /// Gets a specification attribute
        /// </summary>
        /// <param name="specificationAttributeId">The specification attribute identifier</param>
        /// <returns>Specification attribute</returns>
        SpecificationAttribute GetSpecificationAttributeById(string specificationAttributeId);

        /// <summary>
        /// Gets specification attributes
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Specification attributes</returns>
        IPagedList<SpecificationAttribute> GetSpecificationAttributes(int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Deletes a specification attribute
        /// </summary>
        /// <param name="specificationAttribute">The specification attribute</param>
        void DeleteSpecificationAttribute(SpecificationAttribute specificationAttribute);

        /// <summary>
        /// Inserts a specification attribute
        /// </summary>
        /// <param name="specificationAttribute">The specification attribute</param>
        void InsertSpecificationAttribute(SpecificationAttribute specificationAttribute);

        /// <summary>
        /// Updates the specification attribute
        /// </summary>
        /// <param name="specificationAttribute">The specification attribute</param>
        void UpdateSpecificationAttribute(SpecificationAttribute specificationAttribute);

        #endregion

        #region Specification attribute option

        /// <summary>
        /// Gets a specification attribute option
        /// </summary>
        /// <param name="specificationAttributeOption">The specification attribute option</param>
        /// <returns>Specification attribute option</returns>
        SpecificationAttribute GetSpecificationAttributeByOptionId(string specificationAttributeOption);

        /// <summary>
        /// Deletes a specification attribute option
        /// </summary>
        /// <param name="specificationAttributeOption">The specification attribute option</param>
        void DeleteSpecificationAttributeOption(SpecificationAttributeOption specificationAttributeOption);

        #endregion

        #region Product specification attribute

        /// <summary>
        /// Deletes a product specification attribute mapping
        /// </summary>
        /// <param name="productSpecificationAttribute">Product specification attribute</param>
        void DeleteProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute);

        /// <summary>
        /// Inserts a product specification attribute mapping
        /// </summary>
        /// <param name="productSpecificationAttribute">Product specification attribute mapping</param>
        void InsertProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute);

        /// <summary>
        /// Updates the product specification attribute mapping
        /// </summary>
        /// <param name="productSpecificationAttribute">Product specification attribute mapping</param>
        void UpdateProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute);

        /// <summary>
        /// Gets a count of product specification attribute mapping records
        /// </summary>
        /// <param name="productId">Product identifier; 0 to load all records</param>
        /// <param name="specificationAttributeOptionId">The specification attribute option identifier; 0 to load all records</param>
        /// <returns>Count</returns>
        int GetProductSpecificationAttributeCount(string productId = "", string specificationAttributeId = "", string specificationAttributeOptionId = "");

        #endregion
    }
}
