using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Events;
using SensorX.Warehouse.Application.Events.Consumers;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Infrastructure.Persistences;
using SensorX.Warehouse.Infrastructure.Services;

namespace SensorX.Warehouse.Infrastructure.DI
{
    public static class DI
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                       .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

            services.AddMassTransit(x =>
            {
                // Đăng ký Consumer
                x.AddConsumer<OrderCreatedConsumer>();
                x.AddConsumer<TransferOrderCreatedConsumer>();
                x.AddConsumer<CreateProductConsumer>();
                x.AddConsumer<UpdateProductConsumer>();
                x.AddConsumer<ChangeProductStatusConsumer>();
                x.AddConsumer<DeleteProductConsumer>();
                
                x.AddConsumer<TransferOrderFinishedConsumer>();
                x.AddConsumer<SupplyRequestCreatedConsumer>();
                x.AddConsumer<SupplyRequestFulfilledConsumer>();

                // Đăng ký Entity Framework Outbox
                x.AddEntityFrameworkOutbox<AppDbContext>(o =>
                {
                    o.UsePostgres();
                    o.UseBusOutbox();
                });

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqSettings = configuration.GetSection("RabbitMq");
                    var host = rabbitMqSettings["Host"] ?? "localhost";
                    var port = ushort.Parse(rabbitMqSettings["Port"] ?? "5672");
                    var virtualHost = rabbitMqSettings["VirtualHost"] ?? "/";
                    
                    // Get warehouse ID to make endpoints unique per warehouse
                    var warehouseId = configuration["WAREHOUSE_ID"] ?? configuration["Warehouse:Id"] ?? "default";
                    var warehouseIdShort = warehouseId.Length > 8 ? warehouseId.Substring(0, 8) : warehouseId;

                    cfg.Host(host, port, virtualHost, h =>
                    {
                        h.Username(rabbitMqSettings["Username"] ?? "guest");
                        h.Password(rabbitMqSettings["Password"] ?? "guest");

                        // Intentionally not setting client provided name here to maintain compatibility
                    });

                    cfg.Message<CreateProductEvent>(e =>
                        e.SetEntityName("Product-Created-Event"));

                    // Create unique endpoint name per warehouse instance to ensure all warehouses receive all events
                    cfg.ReceiveEndpoint($"product-created-consumer-{warehouseIdShort}", e =>
                    {
                        // Optimize for real-time product sync - low latency
                        e.PrefetchCount = 1;  // Process messages immediately, not in batches
                        e.ConcurrentMessageLimit = 1;  // Sequential processing ensures consistency
                        e.ConfigureConsumer<CreateProductConsumer>(context);
                    });

                    cfg.Message<UpdateProductEvent>(e =>
                        e.SetEntityName("Product-Updated-Event"));

                    cfg.ReceiveEndpoint($"product-updated-consumer-{warehouseIdShort}", e =>
                    {
                        e.PrefetchCount = 1;
                        e.ConcurrentMessageLimit = 1;
                        e.ConfigureConsumer<UpdateProductConsumer>(context);
                    });

                    cfg.Message<ChangeProductStatusEvent>(e =>
                        e.SetEntityName("Product-Status-Changed-Event"));

                    cfg.ReceiveEndpoint($"product-status-changed-consumer-{warehouseIdShort}", e =>
                    {
                        e.PrefetchCount = 1;
                        e.ConcurrentMessageLimit = 1;
                        e.ConfigureConsumer<ChangeProductStatusConsumer>(context);
                    });

                    cfg.Message<DeleteProductEvent>(e =>
                        e.SetEntityName("Product-Deleted-Event"));

                    cfg.ReceiveEndpoint($"product-deleted-consumer-{warehouseIdShort}", e =>
                    {
                        e.PrefetchCount = 1;
                        e.ConcurrentMessageLimit = 1;
                        e.ConfigureConsumer<DeleteProductConsumer>(context);
                    });

                    cfg.ReceiveEndpoint($"transfer-order-finished-consumer-{warehouseIdShort}", e =>
                    {
                        e.ConfigureConsumer<TransferOrderFinishedConsumer>(context);
                    });
                    
                    cfg.ReceiveEndpoint($"supply-request-created-consumer-{warehouseIdShort}", e =>
                    {
                        e.ConfigureConsumer<SupplyRequestCreatedConsumer>(context);
                    });
                    
                    cfg.ReceiveEndpoint($"supply-request-fulfilled-consumer-{warehouseIdShort}", e =>
                    {
                        e.ConfigureConsumer<SupplyRequestFulfilledConsumer>(context);
                    });

                    cfg.ConfigureEndpoints(context);
                          // Message entity names for new events
                          cfg.Message<InventorySnapshotEvent>(e => e.SetEntityName("Inventory-Snapshot-Event"));
                          cfg.Message<WarehouseConnectedEvent>(e => e.SetEntityName("Warehouse-Connected-Event"));
                });
            });

            // Register hosted service to publish warehouse connected + inventory snapshot on startup
            services.AddHostedService<WarehouseStartupPublisher>();

            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped(typeof(IQueryBuilder<>), typeof(QueryBuilder<>));
            services.AddScoped<IQueryExecutor, QueryExecutor>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICurrentUser, CurrentUser>();

            return services;
        }
    }
}