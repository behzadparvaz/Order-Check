using BuildingBlock.Messaging.Command;
using BuildingBlock.Monitoring;
using Confluent.Kafka;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OKEService.EndPoints.Web.StartupExtentions;
using OKEService.Utilities.Configurations;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using System.Configuration;
using System.Diagnostics.Metrics;
using System.Reflection;
using TapsiDOC.Order.Core.ApplicationService;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.AuctionOrders;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.JobCancelCustomer;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.OrderDraft;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.OrderScheduled;
using TapsiDOC.Order.Core.ApplicationService.Orders.Commands.OrderScheduledFoAllVendors;
using TapsiDOC.Order.Core.ApplicationService.Orders.Consumers;
using TapsiDOC.Order.Core.Domain.Contracts;
using TapsiDOC.Order.Core.Domain.Coupons.Contracts.Repositories.SQL;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;
using TapsiDOC.Order.EndPoints.Api.V1.Extensions;
using TapsiDOC.Order.EndPoints.Api.V1.Middlewares;
using TapsiDOC.Order.EndPoints.Api.V1.Tasks;
using TapsiDOC.Order.Infra.Connection.ThirdParty.Services;
using TapsiDOC.Order.Infra.Data.Sql.Commands.Coupons.Persistence.SQL;
using TapsiDOC.Order.Infra.Data.Sql.Commands.Coupons.Repositories.SQL;
using TapsiDOC.Order.Infra.Data.Sql.Commands.Drnext.Persistence.SQL;
using TapsiDOC.Order.Infra.Data.Sql.Commands.Orders;
using TapsiDOC.Order.Infra.Data.Sql.Queries.Coupons.Persistence.SQL;
using TapsiDOC.Order.Infra.Data.Sql.Queries.Coupons.Repositories.SQL;
using TapsiDOC.Order.Infra.Data.Sql.Queries.Drnext.Persistence.SQL;
using TapsiDOC.Order.Infra.Data.Sql.Queries.Orders;
using TapsiDOC.Order.Infra.Data.Sql.Queries.Outbox;
using TapsiDOC.Order.Infra.Event.Kafka.Orders;
using CommandContextSetting = TapsiDOC.Order.Infra.Data.Sql.Commands.Orders.ContextSetting;
using QueryContextSetting = TapsiDOC.Order.Infra.Data.Sql.Queries.Orders.ContextSetting;

namespace TapsiDOC.Order.EndPoints.Api.V1
{
    public class Services
    {
        public Services(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOKEServiceApiServices(Configuration);

            services.AddScoped<ICouponQueryRepository, CouponQueryRepository>();
            services.AddScoped<ICouponCommandRepository, CouponCommandRepository>();
            //services.AddInfrastructureServices(Configuration);
            services.AddOIDCIdentity(Configuration);
            services.Configure<ProducerConfig>(Configuration.GetSection(nameof(ProducerConfig)));
            services.AddSingleton<IOrderQueryRepository, OrderQueryRepository>();
            services.AddScoped<IOrderCommandRepository, OrderCommandRepository>();
            services.AddSingleton<IOutboxQueryRepository, OutboxQueryRepository>();
            services.AddSingleton<IEventStoreRepository, EventStoreRepository>();
            services.AddSingleton<IOrderSmsSender, OrderSmsSender>();
            services.AddSingleton<IRequestOrder, RequestOrder>();
            services.AddSingleton<IOMSSender, OMSSender>();
            services.AddSingleton<INFCConnection, NFCConnection>();
            services.AddCors();

            services
                 .AddMediatR(typeof(IAssemblyMarker).Assembly)
                 .AddTransient<IRequestHandler<OrderDraftCommand, string>, OrderDraftCommandHandler>();

            string commandConnectionString = Configuration.GetSection("MongoConnection:CommandConectionString").Value;
            string queryConnectionString = Configuration.GetSection("MongoConnection:QueryConnectionString").Value;
            string mongoDatabase = Configuration.GetSection("MongoConnection:Database").Value;
            services.Configure<CommandContextSetting.Settings>(options =>
            {
                options.ConnectionString = commandConnectionString;
                options.Database = mongoDatabase;
            });
            services.Configure<QueryContextSetting.Settings>(options =>
            {
                options.ConnectionString = queryConnectionString;
                options.Database = mongoDatabase;
            });
            services.Configure<Infra.Event.Kafka.ContextSettingEvent.Settings>(options =>
            {
                options.ConnectionString = queryConnectionString;
                options.Database = mongoDatabase;
            });

            var sqlCommandConnectionString = Configuration.GetConnectionString("CouponCommand") ??
             throw new Exception("Coupon connection string not found");

            var sqlQueryConnectionString = Configuration.GetConnectionString("CouponQuery") ??
                throw new Exception("Coupon connection string not found");

            services.AddDbContextPool<CommandDataContext>(opt =>
            {
                opt.UseSqlServer(sqlCommandConnectionString);
            });
            services.AddDbContextPool<QueryDataContext>(opt =>
            {
                opt.UseSqlServer(sqlQueryConnectionString);
            });           

            var drnextSqlCommandConnectionString = Configuration.GetConnectionString("DrnextCommand") ??
            throw new Exception("DRNEXT COMMAND connection string not found");

            var drnextSqlQueryConnectionString = Configuration.GetConnectionString("DrnextQuery") ??
                throw new Exception("DRNEXT QUERY connection string not found");

            services.AddDbContextPool<DrnextCommandDataContext>(opt =>
            {
                opt.UseSqlServer(drnextSqlCommandConnectionString);
            });
            services.AddDbContextPool<DrnextQueryDataContext>(opt =>
            {
                opt.UseSqlServer(drnextSqlQueryConnectionString);
            });
            services.AddControllers();

            services.AddHttpClient();
            services.AddOpenTelemetry(Configuration, "order-service");          

            var host = Configuration["RabbitMqConnection:Host"]
                ?? throw new Exception("RABBIT MQ HOST NOT FOUND");

            var username = Configuration["RabbitMqConnection:Username"]
                ?? throw new Exception("RABBIT MQ USERNAME NOT FOUND");

            var password = Configuration["RabbitMqConnection:Password"]
                ?? throw new Exception("RABBIT MQ PASSWORD NOT FOUND");

            var virtualHost = Configuration["RabbitMqConnection:VirtualHost"]
             ?? throw new Exception("RABBIT MQ VIRTUAL HOST NOT FOUND");

            var port = ushort.Parse(Configuration["RabbitMqConnection:Port"]
              ?? throw new Exception("RABBIT MQ PORT NOT FOUND"));
            services.AddMassTransit(busConfigurator =>
            {
                // Step 1: Add Consumers Here
                busConfigurator.AddConsumers(typeof(IAssemblyMarker).Assembly);
                // Step 2: Select a Transport
                busConfigurator.UsingRabbitMq((context, configurator) =>
                {
                    configurator.Host(host, port, virtualHost, h =>
                    {
                        h.Username(username);
                        h.Password(password);
                    });

                    configurator.ReceiveEndpoint("oms_order_accept_order", endpoint =>
                    {
                        endpoint.ConfigureConsumeTopology = false;
                        endpoint.PrefetchCount = 1;
                        endpoint.ConcurrentMessageLimit = 1;
                        endpoint.UseMessageRetry(r => r.Interval(5, (int)TimeSpan.FromMinutes(1).TotalMilliseconds));
                        endpoint.ConfigureConsumer<AcceptOrderCosumer>(context);
                    });

                    configurator.Message<OrderDeliveryCommand>(x =>
                    {
                        x.SetEntityName("dms_order_deliverd_exchange");
                    });
                    configurator.ReceiveEndpoint("dms_order_deliverd_queue", endpoint =>
                    {
                        endpoint.Bind("dms_order_deliverd_exchange", cfg =>
                        {
                            cfg.ExchangeType = "fanout";
                        });
                        endpoint.PrefetchCount = 1;
                        endpoint.ConcurrentMessageLimit = 1;
                        endpoint.ConfigureConsumeTopology = false;
                        endpoint.UseMessageRetry(r => r.Interval(5, (int)TimeSpan.FromMinutes(1).TotalMilliseconds));
                        endpoint.ConfigureConsumer<OrderDeliverdConsumer>(context);
                    });
                });


            });

            //services.AddOptions<MassTransitHostOptions>().Configure(options =>
            //{
            //    //The MassTransit Hosted Service blocks execution until a successful connection to the message broker is established.
            //    //options.WaitUntilStarted = true;
            //    options.StartTimeout = TimeSpan.FromMilliseconds(2000);
            //    options.StopTimeout = TimeSpan.FromMilliseconds(2000);
            //});
            Serilog.Debugging.SelfLog.Enable(Console.Out);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
           OKEServiceConfigurationOptions okeserviceConfigurations)
        {
            app.UseCors(x => x
               .AllowAnyMethod()
               .AllowAnyHeader()
               .SetIsOriginAllowed(origin => true)
               .AllowCredentials());
            
            app.UseMiddleware<LogContextEnrichmentMiddleware>();
            
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpMetrics();
            // Must come AFTER routing + cors, BEFORE endpoints
            app.UseMiddleware<TraceIdMiddleware>();
            app.UseOKEServiceApiConfigure(okeserviceConfigurations, env);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMetrics();
                endpoints.MapControllers();
            });
        }
    }
}
