using mini_mes_be.Data;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using mini_mes_be.Services;

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

        // Unify automatic model validation errors to use ApiResponse format
        services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(ms => ms.Value != null && ms.Value.Errors.Count > 0)
                    .ToDictionary(
                        ms => ms.Key.ToSnakeCase(),
                        ms => ms.Value!.Errors.First().ErrorMessage ?? "Validation error"
                    );

                var message = errors.Values.FirstOrDefault() ?? "Validation failed.";
                var response = mini_mes_be.DTOs.ApiResponse.Fail(message, 400, errors);
                return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
            };
        });

        return services;
    }

    /// <summary>
    /// Register JWT Bearer authentication.
    /// The SecretKey is read from config (environment-specific appsettings).
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var secretKey = config["Jwt:SecretKey"]
            ?? throw new InvalidOperationException(
                "Jwt:SecretKey is not configured. " +
                "Add it to appsettings.Development.json or appsettings.Production.json.");

        var issuer = config["Jwt:Issuer"] ?? "mini-mes-api";
        var audience = config["Jwt:Audience"] ?? "mini-mes-client";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero          // No grace period on expiry
                };

                // Return clean JSON 401/403 instead of redirect pages
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async ctx =>
                    {
                        ctx.HandleResponse();
                        ctx.Response.StatusCode = 401;
                        ctx.Response.ContentType = "application/json";
                        var response = mini_mes_be.DTOs.ApiResponse.Fail("Unauthorized: valid Bearer token required.", 401);
                        await ctx.Response.WriteAsJsonAsync(response);
                    },
                    OnForbidden = async ctx =>
                    {
                        ctx.Response.StatusCode = 403;
                        ctx.Response.ContentType = "application/json";
                        var response = mini_mes_be.DTOs.ApiResponse.Fail("Forbidden: you do not have permission to access this resource.", 403);
                        await ctx.Response.WriteAsJsonAsync(response);
                    }
                };
            });

        // Global policy: require authenticated user on all endpoints by default
        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        return services;
    }

    /// <summary>Register application services and repositories.</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Auth
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();

        // Repositories
        // services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();

        // Services
        // services.AddScoped<IWorkOrderService, WorkOrderService>();

        return services;
    }
}
