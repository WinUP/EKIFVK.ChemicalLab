using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySQL.Data.EntityFrameworkCore.Extensions;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Configurations;
using EKIFVK.ChemicalLab.Services.Tracking;
using EKIFVK.ChemicalLab.Services.Verification;
using Microsoft.AspNetCore.HttpOverrides;

namespace EKIFVK.ChemicalLab {
    public class Startup {
        public Startup(IHostingEnvironment env) {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc();
            services.AddOptions();
            services.Configure<AuthenticationModule>(Configuration.GetSection("AuthenticationConfiguration"));
            services.Configure<UserModule>(Configuration.GetSection("UserModuleConfiguration"));
            services.AddDbContext<ChemicalLabContext>(
                options => options.UseMySQL(Configuration.GetConnectionString("Database")));
            services.AddSingleton<ITrackerService, TrackerService>();
            services.AddScoped<IVerificationService, VerificationService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.UseMvc();
            //Support for non-iis environment
            app.UseForwardedHeaders(new ForwardedHeadersOptions {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
    }
}