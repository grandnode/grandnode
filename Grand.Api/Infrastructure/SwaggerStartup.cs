using Grand.Api.Extensions;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Api.Infrastructure
{
    public class SwaggerStartup : IGrandStartup
    {
        public void Configure(IApplicationBuilder application, IWebHostEnvironment webHostEnvironment)
        {
            var apiConfig = application.ApplicationServices.GetService<ApiConfig>();
            if (apiConfig.Enabled && apiConfig.UseSwagger)
            {
                application.UseSwagger();
                application.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Grandnode API V1");
                });
            }
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var apiConfig = services.BuildServiceProvider().GetService<ApiConfig>();
            if (apiConfig.Enabled && apiConfig.UseSwagger)
            {

                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Grandnode API", Version = "v1" });
                    c.AddSecurityDefinition("Bearer", //Name the security scheme
                        new OpenApiSecurityScheme {
                            Description = "JWT Authorization header using the Bearer scheme.",
                            Type = SecuritySchemeType.Http, //We set the scheme type to http since we're using bearer authentication
                            Scheme = "bearer"               //The name of the HTTP Authorization scheme to be used in the Authorization header. In this case "bearer".
                        });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                        {
                            new OpenApiSecurityScheme {
                                Reference = new OpenApiReference{
                                    Id = "Bearer",      //The name of the previously defined security scheme.
                                    Type = ReferenceType.SecurityScheme
                                }
                            },
                            new List<string>()
                        }
                    });
                    c.OperationFilter<AddParamOperationFilter>();
                    c.EnableAnnotations();
                });

                SetOutputFormatters(services);
            }
        }

        public int Order => 1001;

        private void SetOutputFormatters(IServiceCollection services)
        {
            services.AddMvcCore(options =>
            {
                foreach (var outputFormatter in options.OutputFormatters.OfType<ODataOutputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0))
                {
                    outputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
                foreach (var inputFormatter in options.InputFormatters.OfType<ODataInputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0))
                {
                    inputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
            });
        }


    }
}
