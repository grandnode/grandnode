using Grand.Core.Configuration;
using Grand.Core.Domain.Customers;
using Grand.Services.Customers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace Grand.Services.Authentication
{
    public partial class ApiAuthenticationService : IApiAuthenticationService
    {
        private readonly ApiConfig _apiConfig;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICustomerService _customerService;
        private Customer _cachedCustomer;

        public ApiAuthenticationService(ApiConfig apiConfig, IHttpContextAccessor httpContextAccessor,
            ICustomerService customerService)
        {
            this._apiConfig = apiConfig;
            this._httpContextAccessor = httpContextAccessor;
            this._customerService = customerService;
        }

        /// <summary>
        /// Sign in
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual void SignIn(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email));

            var customer = _customerService.GetCustomerByEmail(email);
            if (customer != null)
                _cachedCustomer = customer;

        }

        /// <summary>
        /// Get authenticated customer
        /// </summary>
        /// <returns>Customer</returns>
        public virtual Customer GetAuthenticatedCustomer()
        {
            //whether there is a cached customer
            if (_cachedCustomer != null)
                return _cachedCustomer;

            //try to get authenticated user identity
            var authenticateResult = _httpContextAccessor.HttpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme).Result;
            if (!authenticateResult.Succeeded)
                return null;

            Customer customer = null;

            //try to get customer by email
            var emailClaim = authenticateResult.Principal.Claims.FirstOrDefault(claim => claim.Type == "Email");
            if (emailClaim != null)
                customer = _customerService.GetCustomerByEmail(emailClaim.Value);


            //whether the found customer is available
            if (customer == null || !customer.Active || customer.Deleted || !customer.IsRegistered())
                return null;

            //cache authenticated customer
            _cachedCustomer = customer;

            return _cachedCustomer;

        }

    }
}
