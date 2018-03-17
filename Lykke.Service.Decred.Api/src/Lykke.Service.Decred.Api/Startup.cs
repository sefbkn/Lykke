using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
using NDecred.Common;
using Newtonsoft.Json.Serialization;
using Npgsql;

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
                    options.SerializerSettings.SerializationBinder = new DefaultSerializationBinder();
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                });

            RegisterRepositories(services);

            var appSettings = Configuration.Get<AppSettings>();

            // Register network dependency
            services.AddTransient(p => Network.ByName(appSettings.Network));
            services.AddTransient(p => new DcrdConfig
            {
                DcrdApiUrl = appSettings.DcrdApiUrl,
                HttpClientHandler = new HttpClientHandler
                {
                    Credentials = new NetworkCredential(appSettings.DcrdRpcUser, appSettings.DcrdRpcPass),
                }
            });
            
            services.AddTransient<HttpClient>();
            services.AddTransient<TransactionHistoryService>();
            services.AddTransient<TransactionBuilderService>();
            services.AddTransient<ITransactionFeeService, TransactionFeeService>();
            services.AddTransient<ITransactionBroadcastService, TransactionBroadcastService>();
            services.AddTransient<IAddressValidationService, AddressValidationService>();
            services.AddTransient<BalanceService>();
        }

        private void RegisterRepositories(IServiceCollection services)
        {
            var consoleLogger = new LogToConsole();

            // Wire up azure connections
            var settings = Configuration.LoadSettings<AppSettings>();
            var connectionString = settings.ConnectionString(a => Configuration.GetConnectionString("azure"));
            
            services.AddTransient
               <IObservableOperationRepository<ObservableWalletEntity>, AzureRepo<ObservableWalletEntity>>(e => 
                    new AzureRepo<ObservableWalletEntity>(
                        AzureTableStorage<ObservableWalletEntity>.Create(connectionString, "ObservableWallet", consoleLogger)
                    ));
            
            services.AddTransient
                <IObservableOperationRepository<ObservableAddressActivityEntity>, AzureRepo<ObservableAddressActivityEntity>>(e => 
                    new AzureRepo<ObservableAddressActivityEntity>(
                        AzureTableStorage<ObservableAddressActivityEntity>.Create(connectionString, "ObservableAddress", consoleLogger)
                    ));

            services.AddTransient
                <IObservableOperationRepository<KeyValueEntity>, AzureRepo<KeyValueEntity>>(e => 
                    new AzureRepo<KeyValueEntity>(
                        AzureTableStorage<KeyValueEntity>.Create(connectionString, "BuildTransactionRequestEntity", consoleLogger)
                    ));

            // Write up dcrdata postgres client to monitor transactions and balances.
            var dcrdataDbFactory = new Func<Task<IDbConnection>>(async () =>
            {
                var sqlClient = new NpgsqlConnection(Configuration.GetConnectionString("dcrdata"));
                await sqlClient.OpenAsync();
                return sqlClient;
            });

            services.AddScoped<IDbConnection, NpgsqlConnection>((p) =>
            {
                var sqlClient = new NpgsqlConnection(Configuration.GetConnectionString("dcrdata"));
                sqlClient.Open();
                return sqlClient;
            });
            
            services.AddTransient<IBlockRepository, BlockRepository>();
            services.AddTransient<IAddressRepository, AddressRepository>();
            services.AddTransient<ITransactionRepository, TransactionRepository>();
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
