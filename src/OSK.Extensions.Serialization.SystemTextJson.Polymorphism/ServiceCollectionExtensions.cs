using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

namespace OSK.Extensions.Serialization.SystemTextJson.Polymorphism
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSystemTextJsonPolymorphism(this IServiceCollection services)
        {
            services.AddTransient<JsonConverter, PolymorphismJsonConverter>();

            return services;
        }
    }
}
