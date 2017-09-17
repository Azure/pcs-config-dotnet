// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.IoTSolutions.UIConfig.Services;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService
{
    public class Startup
    {
        // Initialized in `Startup`
        public IConfigurationRoot Configuration { get; }

        // Initialized in `ConfigureServices`
        public IContainer ApplicationContainer { get; private set; }

        // Invoked by `Program.cs`
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddIniFile("appsettings.ini", optional: false, reloadOnChange: true);
            this.Configuration = builder.Build();
        }

        // This is where you register dependencies, add services to the
        // container. This method is called by the runtime, before the
        // Configure method below.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add CORS service
            services.AddCors();

            // Add controllers as services so they'll be resolved.
            services.AddMvc().AddControllersAsServices();

            this.ApplicationContainer = DependencyResolution.Setup(services);

            // Create the IServiceProvider based on the container
            return new AutofacServiceProvider(this.ApplicationContainer);
        }

        // This method is called by the runtime, after the ConfigureServices
        // method above. Use this method to add middleware.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IApplicationLifetime appLifetime)
        {
            loggerFactory.AddConsole(this.Configuration.GetSection("Logging"));

            app.UseCors(this.BuildCorsPolicy);

            app.UseMvc();

            // If you want to dispose of resources that have been resolved in the
            // application container, register for the "ApplicationStopped" event.
            appLifetime.ApplicationStopped.Register(() => this.ApplicationContainer.Dispose());

            appLifetime.ApplicationStarted.Register(() =>
            {
                var unused = this.OnStartAsync();
            });
        }

        private async Task OnStartAsync()
        {
            var seed = this.ApplicationContainer.Resolve<ISeed>();
            await seed.TrySeedAsync();

            await Task.Delay(TimeSpan.FromMinutes(5));

            var cache = this.ApplicationContainer.Resolve<ICache>();
            await cache.RebuildCacheAsync();
        }

        private void BuildCorsPolicy(CorsPolicyBuilder builder)
        {
            var config = this.ApplicationContainer.Resolve<IConfig>();
            var logger = this.ApplicationContainer.Resolve<Services.Diagnostics.ILogger>();

            CorsWhitelistModel model;
            try
            {
                model = JsonConvert.DeserializeObject<CorsWhitelistModel>(config.CorsWhitelist);
                if (model == null)
                {
                    logger.Error("Invalid CORS whitelist. Ignored", () => new { config.CorsWhitelist });
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Invalid CORS whitelist. Ignored", () => new { config.CorsWhitelist, ex.Message });
                return;
            }

            if (model.Origins == null)
            {
                logger.Info("No setting for CORS origin policy was found, ignore", () => { });
            }
            else if (model.Origins.Contains("*"))
            {
                logger.Info("CORS policy allowed any origin", () => { });
                builder.AllowAnyOrigin();
            }
            else
            {
                logger.Info("Add specified origins to CORS policy", () => new { model.Origins });
                builder.WithOrigins(model.Origins);
            }

            if (model.Origins == null)
            {
                logger.Info("No setting for CORS method policy was found, ignore", () => { });
            }
            else if (model.Methods.Contains("*"))
            {
                logger.Info("CORS policy allowed any method", () => { });
                builder.AllowAnyMethod();
            }
            else
            {
                logger.Info("Add specified methods to CORS policy", () => new { model.Methods });
                builder.WithMethods(model.Methods);
            }

            if (model.Origins == null)
            {
                logger.Info("No setting for CORS header policy was found, ignore", () => { });
            }
            else if (model.Headers.Contains("*"))
            {
                logger.Info("CORS policy allowed any header", () => { });
                builder.AllowAnyHeader();
            }
            else
            {
                logger.Info("Add specified headers to CORS policy", () => new { model.Headers });
                builder.WithHeaders(model.Headers);
            }
        }
    }
}
