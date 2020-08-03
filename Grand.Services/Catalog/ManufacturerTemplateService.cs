using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Services.Events;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Manufacturer template service
    /// </summary>
    public partial class ManufacturerTemplateService : IManufacturerTemplateService
    {
        #region Fields

        private readonly IRepository<ManufacturerTemplate> _manufacturerTemplateRepository;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="manufacturerTemplateRepository">Manufacturer template repository</param>
        /// <param name="mediator">Mediator</param>
        public ManufacturerTemplateService(IRepository<ManufacturerTemplate> manufacturerTemplateRepository,
            IMediator mediator)
        {
            _manufacturerTemplateRepository = manufacturerTemplateRepository;
            _mediator = mediator;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete manufacturer template
        /// </summary>
        /// <param name="manufacturerTemplate">Manufacturer template</param>
        public virtual async Task DeleteManufacturerTemplate(ManufacturerTemplate manufacturerTemplate)
        {
            if (manufacturerTemplate == null)
                throw new ArgumentNullException("manufacturerTemplate");

            await _manufacturerTemplateRepository.DeleteAsync(manufacturerTemplate);

            //event notification
            await _mediator.EntityDeleted(manufacturerTemplate);
        }

        /// <summary>
        /// Gets all manufacturer templates
        /// </summary>
        /// <returns>Manufacturer templates</returns>
        public virtual async Task<IList<ManufacturerTemplate>> GetAllManufacturerTemplates()
        {
            return await _manufacturerTemplateRepository.Collection.Find(new BsonDocument()).SortBy(x => x.DisplayOrder).ToListAsync();
        }

        /// <summary>
        /// Gets a manufacturer template
        /// </summary>
        /// <param name="manufacturerTemplateId">Manufacturer template identifier</param>
        /// <returns>Manufacturer template</returns>
        public virtual async Task<ManufacturerTemplate> GetManufacturerTemplateById(string manufacturerTemplateId)
        {
            return await _manufacturerTemplateRepository.GetByIdAsync(manufacturerTemplateId);
        }

        /// <summary>
        /// Inserts manufacturer template
        /// </summary>
        /// <param name="manufacturerTemplate">Manufacturer template</param>
        public virtual async Task InsertManufacturerTemplate(ManufacturerTemplate manufacturerTemplate)
        {
            if (manufacturerTemplate == null)
                throw new ArgumentNullException("manufacturerTemplate");

            await _manufacturerTemplateRepository.InsertAsync(manufacturerTemplate);

            //event notification
            await _mediator.EntityInserted(manufacturerTemplate);
        }

        /// <summary>
        /// Updates the manufacturer template
        /// </summary>
        /// <param name="manufacturerTemplate">Manufacturer template</param>
        public virtual async Task UpdateManufacturerTemplate(ManufacturerTemplate manufacturerTemplate)
        {
            if (manufacturerTemplate == null)
                throw new ArgumentNullException("manufacturerTemplate");

            await _manufacturerTemplateRepository.UpdateAsync(manufacturerTemplate);

            //event notification
            await _mediator.EntityUpdated(manufacturerTemplate);
        }

        #endregion
    }
}
