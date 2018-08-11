using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Grand.Core.Http;
using Grand.Core.Infrastructure;

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

        public static void StoreParametersForRoundTrip(ExternalAuthenticationParameters parameters)
        {
            EngineContext.Current.Resolve<IHttpContextAccessor>().HttpContext?.Session?.Set(EXTERNAL_AUTHENTICATION_PARAMETERS, parameters);
        }

        public static void AddErrorsToDisplay(string error)
        {
            var session = EngineContext.Current.Resolve<IHttpContextAccessor>().HttpContext?.Session;
            var errors = session?.Get<IList<string>>(EXTERNAL_AUTHENTICATION_ERRORS) ?? new List<string>();
            errors.Add(error);
            session?.Set(EXTERNAL_AUTHENTICATION_ERRORS, errors);
        }

        public static IList<string> RetrieveErrorsToDisplay(bool removeOnRetrieval)
        {
            var session = EngineContext.Current.Resolve<IHttpContextAccessor>().HttpContext?.Session;
            var errors = session?.Get<IList<string>>(EXTERNAL_AUTHENTICATION_ERRORS);

            if (errors != null && removeOnRetrieval)
                session.Remove(EXTERNAL_AUTHENTICATION_ERRORS);

            return errors;
        }

        #endregion
    }
}