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