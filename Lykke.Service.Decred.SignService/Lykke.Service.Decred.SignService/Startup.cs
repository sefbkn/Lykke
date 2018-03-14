using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Decred.Common;
using Lykke.Service.Decred.SignService.Services;
using Lykke.Service.Decred_SignService.Core.Services;
using Lykke.Service.Decred_SignService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lykke.Service.Decred.SignService
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var network = Configuration.GetValue<string>("Network");
            services.AddTransient(s => Network.ByName(network));
            services.AddTransient<ECSecurityService>();
            services.AddTransient<SigningService>();
            services.AddTransient<IWalletProvider, WalletProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
