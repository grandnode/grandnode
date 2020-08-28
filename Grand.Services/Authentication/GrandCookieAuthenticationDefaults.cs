using Microsoft.AspNetCore.Http;

namespace Grand.Services.Authentication
{
    public static class GrandCookieAuthenticationDefaults
    {
        /// <summary>
        /// The default value used for authentication scheme
        /// </summary>
        public const string AuthenticationScheme = "Authentication";

        /// <summary>
        /// The default value used for external authentication scheme
        /// </summary>
        public const string ExternalAuthenticationScheme = "ExternalAuthentication";
        
        /// <summary>
        /// The default value for the login path
        /// </summary>
        public static readonly PathString LoginPath = new PathString("/login");

        /// <summary>
        /// The default value used for the logout path
        /// </summary>
        public static readonly PathString LogoutPath = new PathString("/logout");

        /// <summary>
        /// The default value for the access denied path
        /// </summary>
        public static readonly PathString AccessDeniedPath = new PathString("/page-not-found");

        /// <summary>
        /// The default value of the return url parameter
        /// </summary>
        public static readonly string ReturnUrlParameter = "";
    }
}
