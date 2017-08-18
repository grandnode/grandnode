using Grand.Core.Domain.Customers;

namespace Grand.Services.Authentication
{
    /// <summary>
    /// Authentication service interface
    /// </summary>
    public partial interface IGrandAuthenticationService
    {
        /// <summary>
        /// Sign in
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="createPersistentCookie">A value indicating whether to create a persistent cookie</param>
        void SignIn(Customer customer, bool createPersistentCookie);

        /// <summary>
        /// Sign out
        /// </summary>
        void SignOut();

        /// <summary>
        /// Get authenticated customer
        /// </summary>
        /// <returns>Customer</returns>
        Customer GetAuthenticatedCustomer();
    }
}