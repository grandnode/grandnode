using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Grand.Core.Domain.Customers;
using Grand.Services.Customers;
using Grand.Services.Authentication;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;

namespace Grand.Service.Authentication
{
    public partial class CookieAuthenticationService : IGrandAuthenticationService
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerService _customerService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private Customer _cachedCustomer;

        #endregion

        #region Ctor

        public CookieAuthenticationService(CustomerSettings customerSettings,
            ICustomerService customerService,
            IHttpContextAccessor httpContextAccessor)
        {
            this._customerSettings = customerSettings;
            this._customerService = customerService;
            this._httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sign in
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="isPersistent">Whether the authentication session is persisted across multiple requests</param>
        public virtual void SignIn(Customer customer, bool isPersistent)
        {
            var authenticationManager = _httpContextAccessor.HttpContext;
            if (authenticationManager == null)
                return;

            //create claims for username and email of the customer
            var claims = new List<Claim>();
            if (!string.IsNullOrEmpty(customer.Username))
                claims.Add(new Claim(ClaimTypes.Name, customer.Username, ClaimValueTypes.String, GrandCookieAuthenticationDefaults.ClaimsIssuer));

            if (!string.IsNullOrEmpty(customer.Email))
                claims.Add(new Claim(ClaimTypes.Email, customer.Email, ClaimValueTypes.Email, GrandCookieAuthenticationDefaults.ClaimsIssuer));

            //create principal for the current authentication scheme
            var userIdentity = new ClaimsIdentity(claims, GrandCookieAuthenticationDefaults.AuthenticationScheme);
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            //set value indicating whether session is persisted and the time at which the authentication was issued
            var authenticationProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                IssuedUtc = DateTime.UtcNow
            };

            //sign in
            var signInTask = authenticationManager.SignInAsync(GrandCookieAuthenticationDefaults.AuthenticationScheme, userPrincipal, authenticationProperties);
            signInTask.Wait();

            //cache authenticated customer
            _cachedCustomer = customer;
        }

        /// <summary>
        /// Sign out
        /// </summary>
        public virtual void SignOut()
        {
            var authenticationManager = _httpContextAccessor.HttpContext;
            if (authenticationManager == null)
                return;

            //reset cached customer
            _cachedCustomer = null;

            //and sign out from the current authentication scheme
            var signOutTask = authenticationManager.SignOutAsync(GrandCookieAuthenticationDefaults.AuthenticationScheme);
            signOutTask.Wait();
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

            var authenticationManager = _httpContextAccessor.HttpContext;
            if (authenticationManager == null)
                return null;

            //try to get authenticated user identity
            var authenticateTask = authenticationManager.AuthenticateAsync(GrandCookieAuthenticationDefaults.AuthenticationScheme);
            var userPrincipal = authenticateTask.Result;
            var userIdentity = userPrincipal?.Principal?.Identities?.FirstOrDefault(identity => identity.IsAuthenticated);
            if (userIdentity == null)
                return null;

            Customer customer = null;
            if (_customerSettings.UsernamesEnabled)
            {
                //try to get customer by username
                var usernameClaim = userIdentity.FindFirst(claim => claim.Type == ClaimTypes.Name
                    && claim.Issuer.Equals(GrandCookieAuthenticationDefaults.ClaimsIssuer, StringComparison.OrdinalIgnoreCase));
                if (usernameClaim != null)
                    customer = _customerService.GetCustomerByUsername(usernameClaim.Value);
            }
            else
            {
                //try to get customer by email
                var emailClaim = userIdentity.FindFirst(claim => claim.Type == ClaimTypes.Email
                    && claim.Issuer.Equals(GrandCookieAuthenticationDefaults.ClaimsIssuer, StringComparison.OrdinalIgnoreCase));
                if (emailClaim != null)
                    customer = _customerService.GetCustomerByEmail(emailClaim.Value);
            }

            //whether the found customer is available
            if (customer == null || !customer.Active || customer.Deleted || !customer.IsRegistered())
                return null;

            //cache authenticated customer
            _cachedCustomer = customer;

            return _cachedCustomer;
        }

        #endregion
    }
}