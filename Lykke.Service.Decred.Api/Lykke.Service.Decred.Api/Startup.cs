﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Decred.BlockExplorer;
using Lykke.Service.Decred.Api.Middleware;
using Lykke.Service.Decred.Api.Repository;
using Lykke.Service.Decred.Api.Services;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.Decred.Api
{
    public class Startup
    {
        private readonly ILog _log;
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            Environment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {            
            services.Configure<AppSettings>(Configuration);
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver =
                        new Newtonsoft.Json.Serialization.DefaultContractResolver();
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                });

            RegisterRepositories(services);

            var appSettings = Configuration.Get<AppSettings>();
            services.AddTransient(o => appSettings.ApiConfig.NetworkSettings);
            services.AddTransient<IAddressValidationService, AddressValidationService>();
        }

        private void RegisterRepositories(IServiceCollection services)
        {
            // Wire up azure connections
            var settings = Configuration.LoadSettings<AppSettings>();
            var connectionString = settings.ConnectionString(a => Configuration.GetConnectionString("azure"));
            services.AddTransient
               <IObservableOperationRepository<ObservableWalletEntity>, AzureObservableOperationRepository<ObservableWalletEntity>>(e => 
                    new AzureObservableOperationRepository<ObservableWalletEntity>(
                        AzureTableStorage<ObservableWalletEntity>.Create(connectionString, "ObservableWallet", _log)
                    ));
            
            // Write up dcrdata postgres client to monitor transactions and balances.
            var dcrdataDbFactory = new Func<Task<IDbConnection>>(async () =>
            {
                var sqlClient = new SqlConnection(Configuration.GetConnectionString("dcrdata"));
                await sqlClient.OpenAsync();
                return sqlClient;
            });
            services.AddTransient<IBlockRepository, DcrdataPgClient>(e => new DcrdataPgClient(dcrdataDbFactory));
            services.AddTransient<IAddressBalanceRepository, DcrdataPgClient>(e => new DcrdataPgClient(dcrdataDbFactory));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware(typeof(ApiErrorHandler));
            app.UseMvc();
        }
    }
}
