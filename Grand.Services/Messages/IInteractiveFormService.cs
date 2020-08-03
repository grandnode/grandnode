using Grand.Domain.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    public partial interface IInteractiveFormService
    {
        /// <summary>
        /// Inserts a InteractiveForm
        /// </summary>
        /// <param name="InteractiveForm">Interactive Form</param>        
        Task InsertForm(InteractiveForm form);

        /// <summary>
        /// Updates a InteractiveForm
        /// </summary>
        /// <param name="InteractiveForm">Interactive Form</param>
        Task UpdateForm(InteractiveForm form);

        /// <summary>
        /// Deleted a InteractiveForm
        /// </summary>
        /// <param name="InteractiveForm">Interactive Form</param>
        Task DeleteForm(InteractiveForm form);

        /// <summary>
        /// Gets a form by identifier
        /// </summary>
        /// <param name="formId">Form identifier</param>
        /// <returns>Banner</returns>
        Task<InteractiveForm> GetFormById(string formId);

        /// <summary>
        /// Gets all banner
        /// </summary>
        /// <returns>Banners</returns>
        Task<IList<InteractiveForm>> GetAllForms();

    }
}
