using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MvcApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {


        }

        private static async Task ReadDataFromBlobAsync(string str, Stream dest)
        {
            var blob = await ServiceLocator.BlobStorage.GetAsync(ServiceLocator.BlobContainerName, str);
            blob.CopyTo(dest);
        }

        private static async Task ReadWithAttempts(string filename, HttpResponse response)
        {

            var ext = Path.GetExtension(filename);

            if (!string.IsNullOrEmpty(ext)){
               await ReadDataFromBlobAsync(filename, response.Body);
                return;
            }


            try
            {
                await ReadDataFromBlobAsync(filename, response.Body);
            }
            catch (Exception)
            {

            }

            try
            {
                await ReadDataFromBlobAsync(filename+".txt", response.Body);
            }
            catch (Exception)
            {

            }

            await ReadDataFromBlobAsync(filename+".json", response.Body);

        }




        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ServiceLocator.BlobContainerName = Configuration.GetSection("BlobContainerName").Value;
            var connectionString = Configuration.GetSection("AzureConnectionString");
            ServiceLocator.BlobStorage = new AzureStorage.Blob.AzureBlobStorage(connectionString.Value);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.Run(async ctx =>
            {

                try
                {
                    var filename =ctx.Request.Path.Value;
                    filename = filename.Substring(1, filename.Length - 1).ToLower();

                    await ReadWithAttempts(filename, ctx.Response);

                }
                catch (Exception)
                {
                    ctx.Response.StatusCode = 404;
                    await ctx.Response.WriteAsync("");
                }

            });

        }

    }
}
