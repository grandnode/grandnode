using Grand.Core.Http;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Grand.Services.Authentication.External
{
    /// <summary>
    /// External authorizer helper
    /// </summary>
    public static partial class ExternalAuthorizerHelper
    {
        #region Constants

        /// <summary>
        /// Key for store external authentication parameters to session
        /// </summary>
        private const string EXTERNAL_AUTHENTICATION_PARAMETERS = "grand.externalauth.parameters";

        /// <summary>
        /// Key for store external authentication errors to session
        /// </summary>
        private const string EXTERNAL_AUTHENTICATION_ERRORS = "grand.externalauth.errors";

        #endregion

        #region Methods

        public static void StoreParametersForRoundTrip(ExternalAuthenticationParameters parameters, IHttpContextAccessor httpContextAccessor)
        {
            httpContextAccessor.HttpContext?.Session?.Set(EXTERNAL_AUTHENTICATION_PARAMETERS, parameters);
        }

        public static void AddErrorsToDisplay(string error, IHttpContextAccessor httpContextAccessor)
        {
            var session = httpContextAccessor.HttpContext?.Session;
            var errors = session?.Get<IList<string>>(EXTERNAL_AUTHENTICATION_ERRORS) ?? new List<string>();
            errors.Add(error);
            session?.Set(EXTERNAL_AUTHENTICATION_ERRORS, errors);
        }

        public static IList<string> RetrieveErrorsToDisplay(bool removeOnRetrieval, IHttpContextAccessor httpContextAccessor)
        {
            var session = httpContextAccessor.HttpContext?.Session;
            var errors = session?.Get<IList<string>>(EXTERNAL_AUTHENTICATION_ERRORS);

            if (errors != null && removeOnRetrieval)
                session.Remove(EXTERNAL_AUTHENTICATION_ERRORS);

            return errors;
        }

        #endregion
    }
}