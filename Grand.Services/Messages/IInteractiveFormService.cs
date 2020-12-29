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
        /// <returns>Interactive form</returns>
        Task<InteractiveForm> GetFormById(string formId);

        /// <summary>
        /// Gets a form by system name
        /// </summary>
        /// <param name="systemName">Form system name</param>
        /// <returns>Interactive form</returns>
        Task<InteractiveForm> GetFormBySystemName(string systemName);

        /// <summary>
        /// Gets all interactive forms
        /// </summary>
        /// <returns>Interactive forms</returns>
        Task<IList<InteractiveForm>> GetAllForms();

    }
}
