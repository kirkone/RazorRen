namespace RazorRen.Sample.MVC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Configuration;
    using RazorRen.Generator;
    using Microsoft.Extensions.FileProviders;

    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorRenSettings(this.Configuration);
            services.AddMvc();
        }

        public void Configure(
                IApplicationBuilder app,
                IHostingEnvironment env,
                RazorRen.Generator.Options razorRenOptions
            )
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (
                    !string.IsNullOrWhiteSpace(razorRenOptions.OutputPath)
                    &&
                    !System.IO.Directory.Exists(System.IO.Path.Combine(env.ContentRootPath, razorRenOptions.OutputPath))
                )
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(env.ContentRootPath, razorRenOptions.OutputPath));
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseStaticFiles(
                new StaticFileOptions
                {
                    FileProvider = new CompositeFileProvider(
                            new PhysicalFileProvider(env.WebRootPath),
                            new PhysicalFileProvider(System.IO.Path.Combine(env.ContentRootPath, razorRenOptions.OutputPath))
                        )
                }
            );

            app.UseRazorRen();

            app.UseMvc(
                routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");
                }
            );
        }
    }
}
