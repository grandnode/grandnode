using Grand.Services.Customers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Authentication
{
    public class JwtBearerAuthenticationService : IJwtBearerAuthenticationService
    {
        private readonly ICustomerService _customerService;
        private readonly IUserApiService _userApiService;

        private string _errorMessage;
        private string _email;

        public JwtBearerAuthenticationService(
            ICustomerService customerService, IUserApiService userApiService)
        {
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
            var token = context.Principal.Claims.ToList().FirstOrDefault(x => x.Type == "Token")?.Value;
            if(string.IsNullOrEmpty(token))
            {
                _errorMessage = "Wrong token, change password on the customer and create token again";
                return await Task.FromResult(false);
            }
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
            if(userapi.Token != token)
            {
                _errorMessage = "Wrong token, generate again";
                return await Task.FromResult(false);
            }

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Get error message
        /// </summary>
        /// <returns></returns>
        public virtual Task<string> ErrorMessage()
        {
            return Task.FromResult(_errorMessage);
        }

    }
}
