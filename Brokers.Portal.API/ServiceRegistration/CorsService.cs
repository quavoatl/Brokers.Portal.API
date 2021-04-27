using Microsoft.Extensions.DependencyInjection;

namespace Brokers.Portal.API.ServiceRegistration
{
    public static class CorsService
    {
        public static IServiceCollection RegisterCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin();
                });
            });
            return services;
        }

       
    }
}
