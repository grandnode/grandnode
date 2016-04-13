
using Nop.Core.Domain.Customers;
using System.Collections.Generic;

namespace Nop.Services.Customers
{
    public partial interface ICustomerActionService
    {


        /// <summary>
        /// Gets customer action
        /// </summary>
        /// <param name="id">Customer action identifier</param>
        /// <returns>Customer Action</returns>
        CustomerAction GetCustomerActionById(string id);


        /// <summary>
        /// Gets all customer actions
        /// </summary>
        /// <returns>Customer actions</returns>
        IList<CustomerAction> GetCustomerActions();

        /// <summary>
        /// Inserts a customer action
        /// </summary>
        /// <param name="CustomerAction">Customer action</param>
        void InsertCustomerAction(CustomerAction customerAction);

        /// <summary>
        /// Delete a customer action
        /// </summary>
        /// <param name="customerAction">Customer action</param>
        void DeleteCustomerAction(CustomerAction customerAction);

        /// <summary>
        /// Updates the customer action
        /// </summary>
        /// <param name="customerTag">Customer tag</param>
        void UpdateCustomerAction(CustomerAction customerAction);

        //CustomerActionConditionType GetCustomerActionConditionTypeById(string id);
        IList<CustomerActionType> GetCustomerActionType();
        CustomerActionType GetCustomerActionTypeById(string id);

        void UpdateCustomerActionType(CustomerActionType customerActionType);

    }
}
