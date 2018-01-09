using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using RazorRen.Generator;

namespace RazorRen.Sample.Pages
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorRenSettings(this.Configuration);
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }
    }
}
