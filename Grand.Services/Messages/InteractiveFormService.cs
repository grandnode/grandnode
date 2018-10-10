using Grand.Core.Data;
using Grand.Core.Domain.Messages;
using Grand.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Messages
{
    public partial class InteractiveFormService : IInteractiveFormService
    {
        private readonly IRepository<InteractiveForm> _formRepository;
        private readonly IEventPublisher _eventPublisher;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="FormRepository">Form repository</param>
        /// <param name="eventPublisher">Event published</param>
        public InteractiveFormService(IRepository<InteractiveForm> formRepository,
            IEventPublisher eventPublisher)
        {
            this._formRepository = formRepository;
            this._eventPublisher = eventPublisher;
        }

        /// <summary>
        /// Inserts a form
        /// </summary>
        /// <param name="InteractiveForm">InteractiveForm</param>        
        public virtual void InsertForm(InteractiveForm form)
        {
            if (form == null)
                throw new ArgumentNullException("form");

            _formRepository.Insert(form);
            //event notification
            _eventPublisher.EntityInserted(form);
        }

        /// <summary>
        /// Updates a form
        /// </summary>
        /// <param name="Form">Form</param>
        public virtual void UpdateForm(InteractiveForm form)
        {
            if (form == null)
                throw new ArgumentNullException("form");

            _formRepository.Update(form);

            //event notification
            _eventPublisher.EntityUpdated(form);
        }

        /// <summary>
        /// Deleted a banner
        /// </summary>
        /// <param name="banner">Banner</param>
        public virtual void DeleteForm(InteractiveForm form)
        {
            if (form == null)
                throw new ArgumentNullException("form");

            _formRepository.Delete(form);
            //event notification
            _eventPublisher.EntityDeleted(form);
        }

        /// <summary>
        /// Gets a form by identifier
        /// </summary>
        /// <param name="formId">Form identifier</param>
        /// <returns>Banner</returns>
        public virtual InteractiveForm GetFormById(string formId)
        {
            return _formRepository.GetById(formId);
        }

        /// <summary>
        /// Gets all banners
        /// </summary>
        /// <returns>Banners</returns>
        public virtual IList<InteractiveForm> GetAllForms()
        {

            var query = from c in _formRepository.Table
                        orderby c.CreatedOnUtc
                        select c;
            var forms = query.ToList();

            return forms;
        }

    }
}
