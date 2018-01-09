namespace RazorRen.Generator
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseRazorRen(
                this IApplicationBuilder builder
            )
        {
            return builder.UseMiddleware<Middleware>();
        }

        /// <summary>
        /// Add the settings from "RazorRen" of the appsettings as a Singleton of RazorRen.Generator.Options
        /// </summary>
        /// <param name="services">Specifies the contract for a collection of service descriptors.</param>
        /// <param name="configuration">Represents the root of an IConfiguration hierarchy.</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddRazorRenSettings(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var section = configuration.GetSection("RazorRen");
            var settings = new Options();
            new ConfigureFromConfigurationOptions<Options>(section)
                .Configure(settings);
            services.AddSingleton(settings);

            return services;
        }
    }

}