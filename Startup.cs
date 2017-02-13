using System;
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
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {


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
                    var str =ctx.Request.Path.Value;
                    str = str.Substring(1, str.Length - 1);
                    var blob = await ServiceLocator.BlobStorage.GetAsync(ServiceLocator.BlobContainerName, str);
                    blob.CopyTo(ctx.Response.Body);
                }
                catch (Exception)
                {
                    await ctx.Response.WriteAsync(ServiceLocator.BlobContainerName);
                }

            });

        }

    }
}
