using Grand.Domain.Customers;
using Grand.Services.Customers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Authentication
{
    public partial class ApiAuthenticationService : IApiAuthenticationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICustomerService _customerService;
        private readonly IUserApiService _userApiService;

        private Customer _cachedCustomer;

        private string _errorMessage;
        private string _email;

        public ApiAuthenticationService(IHttpContextAccessor httpContextAccessor,
            ICustomerService customerService, IUserApiService userApiService)
        {
            _httpContextAccessor = httpContextAccessor;
            _customerService = customerService;
            _userApiService = userApiService;
        }

        /// <summary>
        /// Valid
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual async Task<bool> Valid(TokenValidatedContext context)
        {
            _email = context.Principal.Claims.ToList().FirstOrDefault(x => x.Type == "Email")?.Value;

            if (string.IsNullOrEmpty(_email))
            {
                _errorMessage = "Email not exists in the context";
                return await Task.FromResult(false);
            }
            var customer = await _customerService.GetCustomerByEmail(_email);
            if (customer == null || !customer.Active || customer.Deleted)
            {
                _errorMessage = "Email not exists/or not active in the customer table";
                return await Task.FromResult(false);
            }
            var userapi = await _userApiService.GetUserByEmail(_email);
            if (userapi == null || !userapi.IsActive)
            {
                _errorMessage = "User api not exists/or not active in the user api table";
                return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }

        public virtual async Task SignIn()
        {
            if (string.IsNullOrEmpty(_email))
                throw new ArgumentNullException(nameof(_email));

            await SignIn(_email);
        }

        /// <summary>
        /// Sign in
        /// </summary>
        ///<param name="email">Email</param>
        public virtual async Task SignIn(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email));

            var customer = await _customerService.GetCustomerByEmail(email);
            if (customer != null)
                _cachedCustomer = customer;
        }


        /// <summary>
        /// Get error message
        /// </summary>
        /// <returns></returns>
        public virtual Task<string> ErrorMessage()
        {
            return Task.FromResult(_errorMessage);
        }

        /// <summary>
        /// Get authenticated customer
        /// </summary>
        /// <returns>Customer</returns>
        public virtual async Task<Customer> GetAuthenticatedCustomer()
        {
            //whether there is a cached customer
            if (_cachedCustomer != null)
                return _cachedCustomer;

            Customer customer = null;

            //try to get authenticated user identity
            string authHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith(JwtBearerDefaults.AuthenticationScheme))
                return null;

            var authenticateResult = await _httpContextAccessor.HttpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded)
                return null;

            //try to get customer by email
            var emailClaim = authenticateResult.Principal.Claims.FirstOrDefault(claim => claim.Type == "Email");
            if (emailClaim != null)
                customer = await _customerService.GetCustomerByEmail(emailClaim.Value);


            //whether the found customer is available
            if (customer == null || !customer.Active || customer.Deleted || !customer.IsRegistered())
                return null;

            //cache authenticated customer
            _cachedCustomer = customer;

            return _cachedCustomer;

        }

    }
}
