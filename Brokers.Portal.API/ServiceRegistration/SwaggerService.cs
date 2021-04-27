using System;
using System.Collections.Generic;
using Brokers.Portal.API.Swagger;
using IdentityServer4;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Brokers.Portal.API.ServiceRegistration
{
    public static class SwaggerService
    {
        public static IServiceCollection RegisterSwagger(this IServiceCollection services, IConfiguration config)
        {
            var swaggerOptions = new SwaggerOptions();
            config.Bind(nameof(swaggerOptions), swaggerOptions);
            services.AddSingleton(swaggerOptions);

            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    OpenIdConnectUrl = new Uri($"https://localhost:5001/.well-known/openid-configuration"),
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri("https://localhost:5001/connect/authorize"),
                            TokenUrl = new Uri("https://localhost:5001/connect/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                {"roles", "Roles"},
                                {"brokers.first.api", "Microservices Access"},
                                { IdentityServerConstants.StandardScopes.OpenId, "OIDC"},
                                { IdentityServerConstants.StandardScopes.Profile, "Profile"},
                                { IdentityServerConstants.StandardScopes.Address, "Address"}
                            }
                        }
                    }
                });

                options.OperationFilter<SwaggerAuthenticationRequirementsOperationFilter>();
            });
            return services;
        }

       
    }
}
