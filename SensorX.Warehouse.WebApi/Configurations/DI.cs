using System.Reflection;
using SensorX.Warehouse.Domain.Services;

namespace SensorX.Warehouse.WebApi.Configurations
{
    public static class DI
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            // MediatR - scan từ Assembly Application
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.Load("SensorX.Warehouse.Application"));
            });

            // Dịch vụ nghiệp vụ
            services.AddScoped<InventoryService>();
            return services;
        }
    }
}

