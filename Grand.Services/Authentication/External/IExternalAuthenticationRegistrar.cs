﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;

namespace Grand.Services.Authentication.External
{
    /// <summary>
    /// Interface to register (configure) an external authentication service (plugin)
    /// </summary>
    public interface IExternalAuthenticationRegistrar
    {
        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="builder">Authentication builder</param>
        void Configure(AuthenticationBuilder builder, IConfiguration configuration);

        /// <summary>
        /// Gets order of this registrar implementation
        /// </summary>
        int Order { get; }
    }
}
