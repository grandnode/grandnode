using Grand.Domain;
using Grand.Domain.Catalog;
using System.Threading.Tasks;

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
        Task<SpecificationAttribute> GetSpecificationAttributeById(string specificationAttributeId);

        /// <summary>
        /// Gets a specification attribute by sename
        /// </summary>
        /// <param name="sename">Sename</param>
        /// <returns>Specification attribute</returns>
        Task<SpecificationAttribute> GetSpecificationAttributeBySeName(string sename);

        /// <summary>
        /// Gets specification attributes
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Specification attributes</returns>
        Task<IPagedList<SpecificationAttribute>> GetSpecificationAttributes(int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Deletes a specification attribute
        /// </summary>
        /// <param name="specificationAttribute">The specification attribute</param>
        Task DeleteSpecificationAttribute(SpecificationAttribute specificationAttribute);

        /// <summary>
        /// Inserts a specification attribute
        /// </summary>
        /// <param name="specificationAttribute">The specification attribute</param>
        Task InsertSpecificationAttribute(SpecificationAttribute specificationAttribute);

        /// <summary>
        /// Updates the specification attribute
        /// </summary>
        /// <param name="specificationAttribute">The specification attribute</param>
        Task UpdateSpecificationAttribute(SpecificationAttribute specificationAttribute);

        #endregion

        #region Specification attribute option

        /// <summary>
        /// Gets a specification attribute option
        /// </summary>
        /// <param name="specificationAttributeOption">The specification attribute option</param>
        /// <returns>Specification attribute option</returns>
        Task<SpecificationAttribute> GetSpecificationAttributeByOptionId(string specificationAttributeOption);

        /// <summary>
        /// Deletes a specification attribute option
        /// </summary>
        /// <param name="specificationAttributeOption">The specification attribute option</param>
        Task DeleteSpecificationAttributeOption(SpecificationAttributeOption specificationAttributeOption);

        #endregion

        #region Product specification attribute

        /// <summary>
        /// Deletes a product specification attribute mapping
        /// </summary>
        /// <param name="productSpecificationAttribute">Product specification attribute</param>
        Task DeleteProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute);

        /// <summary>
        /// Inserts a product specification attribute mapping
        /// </summary>
        /// <param name="productSpecificationAttribute">Product specification attribute mapping</param>
        Task InsertProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute);

        /// <summary>
        /// Updates the product specification attribute mapping
        /// </summary>
        /// <param name="productSpecificationAttribute">Product specification attribute mapping</param>
        Task UpdateProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute);

        /// <summary>
        /// Gets a count of product specification attribute mapping records
        /// </summary>
        /// <param name="productId">Product identifier; "" to load all records</param>
        /// <param name="specificationAttributeOptionId">The specification attribute option identifier; "" to load all records</param>
        /// <returns>Count</returns>
        int GetProductSpecificationAttributeCount(string productId = "", string specificationAttributeOptionId = "");

        #endregion
    }
}
