using Grand.Core.Domain.Catalog;
using System.Collections.Generic;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Manufacturer template service interface
    /// </summary>
    public partial interface IManufacturerTemplateService
    {
        /// <summary>
        /// Delete manufacturer template
        /// </summary>
        /// <param name="manufacturerTemplate">Manufacturer template</param>
        void DeleteManufacturerTemplate(ManufacturerTemplate manufacturerTemplate);

        /// <summary>
        /// Gets all manufacturer templates
        /// </summary>
        /// <returns>Manufacturer templates</returns>
        IList<ManufacturerTemplate> GetAllManufacturerTemplates();

        /// <summary>
        /// Gets a manufacturer template
        /// </summary>
        /// <param name="manufacturerTemplateId">Manufacturer template identifier</param>
        /// <returns>Manufacturer template</returns>
        ManufacturerTemplate GetManufacturerTemplateById(string manufacturerTemplateId);

        /// <summary>
        /// Inserts manufacturer template
        /// </summary>
        /// <param name="manufacturerTemplate">Manufacturer template</param>
        void InsertManufacturerTemplate(ManufacturerTemplate manufacturerTemplate);

        /// <summary>
        /// Updates the manufacturer template
        /// </summary>
        /// <param name="manufacturerTemplate">Manufacturer template</param>
        void UpdateManufacturerTemplate(ManufacturerTemplate manufacturerTemplate);
    }
}
