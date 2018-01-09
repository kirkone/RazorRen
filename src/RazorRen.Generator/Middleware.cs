
namespace RazorRen.Generator
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;

    public class Middleware
    {
        private readonly RequestDelegate next;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly string outputFolder;

        public Middleware(RequestDelegate next, Options options, IHostingEnvironment hostingEnvironment)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }

            this.next = next;
            this.hostingEnvironment = hostingEnvironment;
            if(options == null || string.IsNullOrWhiteSpace(options.OutputPath))
            {
                this.outputFolder = hostingEnvironment.WebRootPath;
            }
            else
            {
                this.outputFolder = Path.Combine(hostingEnvironment.ContentRootPath, options.OutputPath);
            }
        }

        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (context.Request == null)
            {
                throw new ArgumentNullException(nameof(context.Request));
            }

            // we skip the first slash and we reverse the slashes
            var baseUrl = context.Request.Path.Value.Substring(1).Replace("/", "\\");
            // default files will look for "index.html"
            var destinationFile = Path.Combine(this.outputFolder, baseUrl, "index.html");
            // replace the output stream to collect the result
            var responseStream = context.Response.Body;
            var buffer = new MemoryStream();
            var reader = new StreamReader(buffer);
            context.Response.Body = buffer;
            try
            {
                // execute the rest of the pipeline
                await this.next(context);

                if (context.Response?.ContentType?.Contains("text/html") == false && context.Response.StatusCode != 200)
                {
                    await this.next(context);
                    return;
                }

                EnsureDestinationFolderExist(destinationFile);

                // reset the buffer and retrieve the content
                buffer.Seek(0, SeekOrigin.Begin);
                var responseBody = await reader.ReadToEndAsync();

                // output the content to disk
                await WriteBodyToDisk(responseBody, destinationFile);

                // copy back our buffer to the response stream
                buffer.Seek(0, SeekOrigin.Begin);
                await buffer.CopyToAsync(responseStream);
            }
            finally
            {
                // Workaround for https://github.com/aspnet/KestrelHttpServer/issues/940
                context.Response.Body = responseStream;
            }
        }

        private void EnsureDestinationFolderExist(string destinationFile)
        {
            var directoryName = Path.GetDirectoryName(destinationFile);
            Directory.CreateDirectory(directoryName);
        }

        private async Task WriteBodyToDisk(string responseBody, string destinationFile)
        {
            using (FileStream fs = new FileStream(destinationFile, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    await sw.WriteAsync(responseBody);
                }
            }
        }
    }
}
