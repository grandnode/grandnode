using Grand.Core.Domain.Customers;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Grand.Services.Authentication
{
    public partial interface IApiAuthenticationService
    {
        /// <summary>
        /// Sign in
        /// </summary>
        void SignIn();

        /// <summary>
        /// Sign in
        /// </summary>
        /// <param name="email">email</param>
        void SignIn(string email);

        /// <summary>
        /// Valid email 
        /// </summary>
        ///<param name="context">Token</param>
        bool Valid(TokenValidatedContext context);

        /// <summary>
        /// Get error message
        /// </summary>
        /// <returns></returns>
        string ErrorMessage();

        /// <summary>
        /// Get authenticated customer
        /// </summary>
        /// <returns>Customer</returns>
        Customer GetAuthenticatedCustomer();
    }
}
