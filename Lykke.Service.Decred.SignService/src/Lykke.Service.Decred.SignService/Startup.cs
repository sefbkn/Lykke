using System;
using Lykke.Service.Decred.SignService.Core.Services;
using Lykke.Service.Decred.SignService.Services;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NDecred.Common;
using NDecred.Common.Wallet;

namespace Lykke.Service.Decred.SignService
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var reloadableSettings = Configuration.LoadSettings<AppSettings>();
            
           
            services.AddTransient(p =>
            {
                var networkType = reloadableSettings.CurrentValue.NetworkType.Trim().ToLower();
                var name = 
                    networkType == "test" ? "testnet" :
                    networkType == "main" ? "mainnet" :
                    throw new Exception($"Unrecognized network type '{networkType}'");
                return Network.ByName(name);
            });

            services.AddTransient<ISigningWallet, SigningWallet>();
            services.AddTransient<ISecurityService, SecurityService>();
            services.AddTransient<SigningService>();
            services.AddTransient<IKeyService, KeyService>();
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
