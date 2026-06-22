using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Authorization;
using mini_mes_be.Extensions;
using mini_mes_be.Middlewares;
using Scalar.AspNetCore;
using Serilog;

// ── Serilog bootstrap logger ──────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, lc) =>
        lc.ReadFrom.Configuration(ctx.Configuration)
          .WriteTo.Console());

    // ── Core services ─────────────────────────────────────────────────────────
    // Global AuthorizeFilter: all endpoints require a valid JWT by default.
    // Use [AllowAnonymous] on specific actions to opt-out (e.g. login, register).
    builder.Services.AddControllers(options =>
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        options.Filters.Add(new AuthorizeFilter(policy));
    });
    builder.Services.AddSignalR();
    builder.Services.AddRouting(options =>
    {
        options.LowercaseUrls = true;
        options.LowercaseQueryStrings = true;
    });
    builder.Services.AddOpenApi();

    // ── Custom extensions ─────────────────────────────────────────────────────
    builder.Services.AddDatabase(builder.Configuration);
    builder.Services.AddValidation();
    builder.Services.AddJwtAuthentication(builder.Configuration);   // ← JWT
    builder.Services.AddApplicationServices();

    // ── CORS (adjust origins for production) ──────────────────────────────────
    // SignalR requires AllowCredentials(), which cannot be combined with AllowAnyOrigin().
    // SetIsOriginAllowed(_ => true) acts as a wildcard that is compatible with credentials.
    builder.Services.AddCors(opt => opt.AddPolicy("AllowAll", p =>
        p.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials()));

    // ── Mapster ───────────────────────────────────────────────────────────────
    var mapsterConfig = new Mapster.TypeAdapterConfig();
    builder.Services.AddSingleton(mapsterConfig);
    builder.Services.AddScoped<IMapper, ServiceMapper>();

    var app = builder.Build();

    // ── Middleware pipeline ───────────────────────────────────────────────────
    app.UseMiddleware<ExceptionMiddleware>();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(opt =>
        {
            opt.Title = "Mini MES API";
            opt.Theme = ScalarTheme.DeepSpace;
        });
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");

    app.UseAuthentication();    // ← must come before UseAuthorization
    app.UseAuthorization();

    // RequireAuthorization() at the routing level is a second layer of enforcement.
    // AllowAnonymous metadata (from [AllowAnonymous] attribute) still overrides this.
    app.MapControllers().RequireAuthorization();
    app.MapHub<mini_mes_be.Hubs.MesHub>("/mes-hub");

    // ── Log Scalar URL after the server binds ────────────────────────────────
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var server = app.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses ?? [];
        var baseUrl = addresses.FirstOrDefault(a => a.StartsWith("http://")) ?? addresses.FirstOrDefault() ?? "http://localhost:5130";
        var scalarUrl = $"{baseUrl}/scalar/v1";

        Log.Information("Mini MES API is running");
        Log.Information("Scalar API docs → {Url}", scalarUrl);

        if (app.Environment.IsDevelopment())
        {
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(scalarUrl) { UseShellExecute = true }); }
            catch { /* ignore if browser cannot be opened */ }
        }
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
