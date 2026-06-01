using mini_mes_be.Data;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace mini_mes_be.Extensions;

public static class ServiceExtensions
{
    /// <summary>Register SQL Server DbContext.</summary>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        return services;
    }

    /// <summary>Register FluentValidation for all validators in the assembly.</summary>
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<Program>();
        return services;
    }

    /// <summary>Register application services and repositories.</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Repositories
        // services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();

        // Services
        // services.AddScoped<IWorkOrderService, WorkOrderService>();

        return services;
    }
}
