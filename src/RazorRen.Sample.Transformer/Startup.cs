namespace RazorRen.Sample.Transformer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using RazorRen.Generator;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.AspNetCore.Http;

    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
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
            else
            {
                app.UseExceptionHandler("/Error");
            }

                        if (
                    !string.IsNullOrWhiteSpace(razorRenOptions.OutputPath)
                    &&
                    !System.IO.Directory.Exists(System.IO.Path.Combine(env.ContentRootPath, razorRenOptions.OutputPath))
                )
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(env.ContentRootPath, razorRenOptions.OutputPath));
            }

            app.UseWhen(
                context =>
                {
                    return !context.Request.Path.StartsWithSegments(new PathString("/admin"));
                },
                (branch) =>
                {
                    branch.UseDefaultFiles();
                    branch.UseStaticFiles();
                    branch.UseStaticFiles(
                        new StaticFileOptions
                        {
                            FileProvider = new CompositeFileProvider(
                                    new PhysicalFileProvider(env.WebRootPath),
                                    new PhysicalFileProvider(System.IO.Path.Combine(env.ContentRootPath, razorRenOptions.OutputPath))
                                )
                        }
                    );

                    branch.UseRazorRen();
                }
            );

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }
    }
}
