using Grand.Core.Domain.Customers;

namespace Grand.Services.Authentication
{
    public partial interface IApiAuthenticationService
    {
        /// <summary>
        /// Sign in
        /// </summary>
        /// <param name="email">Email</param>
        void SignIn(string email);

        /// <summary>
        /// Get authenticated customer
        /// </summary>
        /// <returns>Customer</returns>
        Customer GetAuthenticatedCustomer();

    }
}
