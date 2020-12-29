using Grand.Domain.Data;
using Grand.Domain.Messages;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    public partial class InteractiveFormService : IInteractiveFormService
    {
        private readonly IRepository<InteractiveForm> _formRepository;
        private readonly IMediator _mediator;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="formRepository">Form repository</param>
        /// <param name="mediator">Mediator</param>
        public InteractiveFormService(IRepository<InteractiveForm> formRepository,
            IMediator mediator)
        {
            _formRepository = formRepository;
            _mediator = mediator;
        }

        /// <summary>
        /// Inserts a form
        /// </summary>
        /// <param name="InteractiveForm">InteractiveForm</param>        
        public virtual async Task InsertForm(InteractiveForm form)
        {
            if (form == null)
                throw new ArgumentNullException("form");

            await _formRepository.InsertAsync(form);
            //event notification
            await _mediator.EntityInserted(form);
        }

        /// <summary>
        /// Updates a form
        /// </summary>
        /// <param name="Form">Form</param>
        public virtual async Task UpdateForm(InteractiveForm form)
        {
            if (form == null)
                throw new ArgumentNullException("form");

            await _formRepository.UpdateAsync(form);

            //event notification
            await _mediator.EntityUpdated(form);
        }

        /// <summary>
        /// Deleted a interactive form
        /// </summary>
        /// <param name="form">Interactive form</param>
        public virtual async Task DeleteForm(InteractiveForm form)
        {
            if (form == null)
                throw new ArgumentNullException("form");

            await _formRepository.DeleteAsync(form);
            //event notification
            await _mediator.EntityDeleted(form);
        }

        /// <summary>
        /// Gets a form by identifier
        /// </summary>
        /// <param name="formId">Form identifier</param>
        /// <returns>Banner</returns>
        public virtual Task<InteractiveForm> GetFormById(string formId)
        {
            if (string.IsNullOrEmpty(formId))
                throw new ArgumentNullException("formId");

            return _formRepository.GetByIdAsync(formId);
        }

        /// <summary>
        /// Gets a form by system name
        /// </summary>
        /// <param name="systemName">Form system name</param>
        /// <returns>Interactive form</returns>
        public virtual async Task<InteractiveForm> GetFormBySystemName(string systemName)
        {
            if (string.IsNullOrEmpty(systemName))
                throw new ArgumentNullException("systemName");

            return await _formRepository.Table.FirstOrDefaultAsync(x => x.SystemName == systemName);
        }

        /// <summary>
        /// Gets all Interactive forms
        /// </summary>
        /// <returns>Interactive forms</returns>
        public virtual async Task<IList<InteractiveForm>> GetAllForms()
        {

            var query = from c in _formRepository.Table
                        orderby c.CreatedOnUtc
                        select c;
            return await query.ToListAsync();
        }

    }
}
