using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TaskManager.Api.Data;
using TaskManager.Api.Middleware;
using TaskManager.Api.Services;

// =====================================================================
// Program.cs = entry point. It has two phases:
//   1) REGISTRATION of the services in the Dependency Injection container
//   2) CONFIGURATION of the HTTP pipeline (the middleware chain)
// =====================================================================

var builder = WebApplication.CreateBuilder(args);

// ---------- 1) DEPENDENCY INJECTION ----------

// Controllers + JSON: enums travel as strings ("InProgress") instead of numbers (1).
builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// EF Core: registers AppDbContext as SCOPED (one instance per HTTP request).
// The connection string is read from configuration: appsettings.json locally,
// the ConnectionStrings__Default environment variable inside Docker (see docker-compose.yml).
// Changing database = changing ONLY this line + the NuGet package (e.g. UseSqlServer).
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Our services: interface -> implementation, with Scoped lifetime
// (same lifetime as the DbContext they use; a Singleton could NOT depend on a Scoped DbContext).
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IProjectService, ProjectService>();

// Global error handling -> ProblemDetails
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// CORS: the browser blocks calls from http://localhost:5173 (React) to http://localhost:5080 (API)
// because they are different "origins". Here we explicitly allow the frontend.
const string FrontendCorsPolicy = "Frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
        .AllowAnyHeader()       //not in production
        .AllowAnyMethod());     //not in production
});

// OpenAPI document (machine-readable description of the API)
builder.Services.AddOpenApi();

var app = builder.Build();

// ---------- DB creation + sample data ----------
// We create a manual "scope" because AppDbContext is Scoped and here we are not inside an HTTP request.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // EnsureCreated creates the DB and the tables if they don't exist. Simple to get started.
    // NEXT STEP (see guide): replace it with Migrations -> db.Database.Migrate();
    db.Database.EnsureCreated();
    DbSeeder.Seed(db);
}

// ---------- 2) HTTP PIPELINE (order matters!) ----------
// Each request goes through the middleware from top to bottom, the response travels back in reverse order.

app.UseExceptionHandler();          // catches exceptions from the following middleware

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();               // JSON at /openapi/v1.json
    app.MapScalarApiReference();    // interactive UI at /scalar (equivalent of Swagger UI)
}

app.UseCors(FrontendCorsPolicy);

app.MapControllers();               // routing to the controllers

app.Run();
