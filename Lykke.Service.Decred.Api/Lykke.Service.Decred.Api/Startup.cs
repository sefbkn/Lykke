using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using AzureStorage;
using Decred.BlockExplorer;
using Lykke.Service.Decred.Api.Middleware;
using Lykke.Service.Decred.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.Decred.Api
{
    public class Startup
    {
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

            var appSettings = Configuration.Get<AppSettings>();
            services.AddTransient(o => appSettings.ApiConfig.NetworkSettings);
            services.AddTransient<IAddressValidationService, AddressValidationService>();
            
            // Write up dcrdata postgres client to monitor transactions and balances.
            var dcrdataDbFactory = new Func<Task<IDbConnection>>(async () =>
            {
                var sqlClient = new SqlConnection(Configuration.GetConnectionString("dcrdata"));
                await sqlClient.OpenAsync();
                return sqlClient;
            });
            services.AddTransient<IBlockRepository, DcrdataPgClient>(e => new DcrdataPgClient(dcrdataDbFactory));
            services.AddTransient<IAddressBalanceRepository, DcrdataPgClient>(e => new DcrdataPgClient(dcrdataDbFactory));
            
            // Set up observation repositories.
            //services.AddTransient<INoSQLTableStorage<ObservableWalletEntity>, NoSqlT>();
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

    public class InitializeRepositories
    {
        
    }
}
