using Microsoft.Extensions.DependencyInjection;

namespace Grand.Api.Infrastructure.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
        }
        //public static void AddAuthenticationJwtBearer(this IServiceCollection services, ApiConfig apiConfig)
        //{
        //    services.AddAuthorization(options =>
        //    {
        //        options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
        //        {
        //            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        //            policy.RequireAuthenticatedUser();
        //        });
        //    });
        //    //Token Jwt
        //    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        //        {
        //            options.TokenValidationParameters = new TokenValidationParameters
        //            {
        //                ValidateIssuer = apiConfig.ValidateIssuer,
        //                ValidateAudience = apiConfig.ValidateAudience,
        //                ValidateLifetime = true,
        //                ValidateIssuerSigningKey = true,
        //                ValidIssuer = apiConfig.ValidIssuer,
        //                ValidAudience = apiConfig.ValidAudience,
        //                IssuerSigningKey = JwtSecurityKey.Create(apiConfig.SecretKey)
        //            };

        //            options.Events = new JwtBearerEvents
        //            {
        //                OnAuthenticationFailed = context =>
        //                {
        //                    context.NoResult();
        //                    context.Response.StatusCode = 500;
        //                    context.Response.ContentType = "text/plain";
        //                    context.Response.WriteAsync(context.Exception.Message).Wait();
        //                    return Task.CompletedTask;
        //                },
        //                OnChallenge = context =>
        //                {
        //                    context.HandleResponse();
        //                    return Task.CompletedTask;
        //                },
        //                OnTokenValidated = context =>
        //                {
        //                    var apiauthenticationService = EngineContext.Current.Resolve<IApiAuthenticationService>();
        //                    var email = context.Principal.Claims.ToList().FirstOrDefault(x => x.Type == "Email")?.Value;
        //                    if (!string.IsNullOrEmpty(email))
        //                    {
        //                        //authenticate
        //                        apiauthenticationService.SignIn(email);
        //                    }
        //                    return Task.CompletedTask;
        //                },
        //            };
        //        });
        //}
    }
}
