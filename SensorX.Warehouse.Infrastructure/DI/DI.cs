using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Infrastructure.Persistences;
using SensorX.Warehouse.Infrastructure.Services;

namespace SensorX.Warehouse.Infrastructure.DI
{
    public static class DI
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer("Server=.;Database=neo-postman;Trusted_Connection=True;TrustServerCertificate=True;"));

            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICurrentUser, CurrentUser>();

            return services;
        }
    }
}

