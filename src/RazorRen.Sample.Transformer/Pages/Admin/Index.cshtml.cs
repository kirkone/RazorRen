using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorRen.Sample.Transformer.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly IHostingEnvironment env;
        private readonly RazorRen.Generator.Options razorRenOptions;

        public IndexModel(
            IHostingEnvironment env,
            RazorRen.Generator.Options razorRenOptions
        )
        {
            this.env = env;
            this.razorRenOptions = razorRenOptions;
        }

        public void OnGet()
        {

        }

        public IActionResult OnGetCleanup()
        {
            this.ClearFolder(Path.Combine(env.ContentRootPath, razorRenOptions.OutputPath));
            return Page();
        }

        public async Task<IActionResult> OnGetRenderAsync()
        {
            var baseuri =$"{Request.Scheme}://{Request.Host}/";
            var uris = new string[] {"", "About", "Contact" };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseuri);
                HttpResponseMessage response;

                foreach (var uri in uris)
                {
                    response = await client.GetAsync(uri);
                }
            }
            return Page();
        }

        public IActionResult OnGetPublish()
        {
            if (!Directory.Exists(Path.Combine(env.ContentRootPath, "publish")))
            {
                Directory.CreateDirectory(Path.Combine(env.ContentRootPath, "publish"));
            }

            this.ClearFolder(Path.Combine(env.ContentRootPath, "publish"));

            CopyDirectory(new DirectoryInfo(Path.Combine(env.ContentRootPath, "wwwroot")),new DirectoryInfo(Path.Combine(env.ContentRootPath, "publish")));
            CopyDirectory(new DirectoryInfo(Path.Combine(env.ContentRootPath, razorRenOptions.OutputPath)),new DirectoryInfo(Path.Combine(env.ContentRootPath, "publish")));
            return Page();
        }

        private void ClearFolder(string folderPath)
        {
            DirectoryInfo dir = new DirectoryInfo(folderPath);

            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.IsReadOnly = false;
                fi.Delete();
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                ClearFolder(di.FullName);
                di.Delete();
            }
        }

        public static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyDirectory(dir, target.CreateSubdirectory(dir.Name));

            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));

        }  
    }
}
