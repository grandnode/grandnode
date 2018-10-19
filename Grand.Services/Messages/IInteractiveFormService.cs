using Grand.Core.Domain.Messages;
using System.Collections.Generic;

namespace Grand.Services.Messages
{
    public partial interface IInteractiveFormService
    {
        /// <summary>
        /// Inserts a InteractiveForm
        /// </summary>
        /// <param name="InteractiveForm">Interactive Form</param>        
        void InsertForm(InteractiveForm form);

        /// <summary>
        /// Updates a InteractiveForm
        /// </summary>
        /// <param name="InteractiveForm">Interactive Form</param>
        void UpdateForm(InteractiveForm form);

        /// <summary>
        /// Deleted a InteractiveForm
        /// </summary>
        /// <param name="InteractiveForm">Interactive Form</param>
        void DeleteForm(InteractiveForm form);

        /// <summary>
        /// Gets a form by identifier
        /// </summary>
        /// <param name="formId">Form identifier</param>
        /// <returns>Banner</returns>
        InteractiveForm GetFormById(string formId);

        /// <summary>
        /// Gets all banner
        /// </summary>
        /// <returns>Banners</returns>
        IList<InteractiveForm> GetAllForms();

    }
}
