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
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddMassTransit(x =>
            {
                // Đăng ký Consumer
                x.AddConsumer<OrderCreatedConsumer>();
                x.AddConsumer<TransferOrderCreatedConsumer>();
                x.AddConsumer<CreateProductConsumer>();
                x.AddConsumer<UpdateProductConsumer>();
                x.AddConsumer<ChangeProductStatusConsumer>();
                x.AddConsumer<DeleteProductConsumer>();

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

                    cfg.Host(host, port, virtualHost, h =>
                    {
                        h.Username(rabbitMqSettings["Username"] ?? "guest");
                        h.Password(rabbitMqSettings["Password"] ?? "guest");
                    });

                    cfg.Message<CreateProductEvent>(e =>
                        e.SetEntityName("Product-Created-Event"));

                    cfg.ReceiveEndpoint("product-created-consumer", e =>
                    {
                        e.ConfigureConsumer<CreateProductConsumer>(context);
                    });

                    cfg.Message<UpdateProductEvent>(e =>
                        e.SetEntityName("Product-Updated-Event"));

                    cfg.ReceiveEndpoint("product-updated-consumer", e =>
                    {
                        e.ConfigureConsumer<UpdateProductConsumer>(context);
                    });

                    cfg.Message<ChangeProductStatusEvent>(e =>
                        e.SetEntityName("Product-Status-Changed-Event"));

                    cfg.ReceiveEndpoint("product-status-changed-consumer", e =>
                    {
                        e.ConfigureConsumer<ChangeProductStatusConsumer>(context);
                    });

                    cfg.Message<DeleteProductEvent>(e =>
                        e.SetEntityName("Product-Deleted-Event"));

                    cfg.ReceiveEndpoint("product-deleted-consumer", e =>
                    {
                        e.ConfigureConsumer<DeleteProductConsumer>(context);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped(typeof(IQueryBuilder<>), typeof(QueryBuilder<>));
            services.AddScoped<IQueryExecutor, QueryExecutor>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICurrentUser, CurrentUser>();

            return services;
        }
    }
}