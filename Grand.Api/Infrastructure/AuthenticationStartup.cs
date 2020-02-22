﻿using Grand.Api.Constants;
using Grand.Api.Infrastructure.DependencyManagement;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using System;
using System.Linq;

namespace Grand.Api.Infrastructure
{
    public partial class AuthenticationStartup : IGrandStartup
    {
        public void Configure(IApplicationBuilder application)
        {
            var apiConfig = application.ApplicationServices.GetService<ApiConfig>();
            if (apiConfig.Enabled)
            {
                application.UseCors(Configurations.CorsPolicyName);
            }
        }

        public void ConfigureServices(IServiceCollection services,
            IConfiguration configuration)
        {
            var apiConfig = services.BuildServiceProvider().GetService<ApiConfig>();

            if (apiConfig.Enabled)
            {
                //cors
                services.AddCors(options =>
                {
                    options.AddPolicy(Configurations.CorsPolicyName,
                        builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
                });

                //Add OData
                services.AddOData();
                services.AddODataQueryFilter();
            }
        }
        public int Order => 505;

        
    }
}
