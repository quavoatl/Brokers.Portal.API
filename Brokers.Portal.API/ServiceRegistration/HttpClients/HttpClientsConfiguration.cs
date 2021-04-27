using System;
using BrokersPortal.Client.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Brokers.Portal.API.ServiceRegistration.HttpClients
{
    public static class HttpClientsConfiguration
    {
        public static void ConfigureHttpClients(this IServiceCollection services)
        {
            services.AddTransient<AuthorizationHttpMessageHandler>();

            services.AddHttpClient(ClientNames.AUTHORIZATION_SERVER_IS4, config =>
            {
                config.BaseAddress = new Uri("https://localhost:5001/");
                config.DefaultRequestHeaders.Clear();
                config.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });

            services.AddHttpClient(ClientNames.BROKERS_COVER_API, config =>
                {
                    config.BaseAddress = new Uri("https://localhost:5045/");
                    config.DefaultRequestHeaders.Clear();
                    config.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
                })
                .AddHttpMessageHandler<AuthorizationHttpMessageHandler>();

        }
    }
}
