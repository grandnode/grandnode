using Grand.Domain;
using Grand.Domain.Customers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Customers
{
    public partial interface ICustomerActionService
    {

        /// <summary>
        /// Gets customer action
        /// </summary>
        /// <param name="id">Customer action identifier</param>
        /// <returns>Customer Action</returns>
        Task<CustomerAction> GetCustomerActionById(string id);

        /// <summary>
        /// Gets all customer actions
        /// </summary>
        /// <returns>Customer actions</returns>
        Task<IList<CustomerAction>> GetCustomerActions();

        /// <summary>
        /// Inserts a customer action
        /// </summary>
        /// <param name="CustomerAction">Customer action</param>
        Task InsertCustomerAction(CustomerAction customerAction);

        /// <summary>
        /// Delete a customer action
        /// </summary>
        /// <param name="customerAction">Customer action</param>
        Task DeleteCustomerAction(CustomerAction customerAction);

        /// <summary>
        /// Updates the customer action
        /// </summary>
        /// <param name="customerTag">Customer tag</param>
        Task UpdateCustomerAction(CustomerAction customerAction);

        Task<IList<CustomerActionType>> GetCustomerActionType();
        Task<CustomerActionType> GetCustomerActionTypeById(string id);
        Task<IPagedList<CustomerActionHistory>> GetAllCustomerActionHistory(string customerActionId, int pageIndex = 0, int pageSize = 2147483647);
        Task UpdateCustomerActionType(CustomerActionType customerActionType);
    }
}
