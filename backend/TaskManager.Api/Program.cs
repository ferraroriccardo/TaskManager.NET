using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TaskManager.Api.Data;
using TaskManager.Api.Middleware;
using TaskManager.Api.Services;

// =====================================================================
// Program.cs = punto di ingresso. Ha due fasi:
//   1) REGISTRAZIONE dei servizi nel container di Dependency Injection
//   2) CONFIGURAZIONE della pipeline HTTP (la catena di middleware)
// =====================================================================

var builder = WebApplication.CreateBuilder(args);

// ---------- 1) DEPENDENCY INJECTION ----------

// Controllers + JSON: gli enum viaggiano come stringhe ("InProgress") invece che numeri (1).
builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// EF Core: registra AppDbContext come SCOPED (un'istanza per ogni richiesta HTTP).
// La connection string si legge dalla configurazione: appsettings.json in locale,
// variabile d'ambiente ConnectionStrings__Default dentro Docker (vedi docker-compose.yml).
// Cambiare database = cambiare SOLO questa riga + il pacchetto NuGet (es. UseSqlServer).
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// I nostri servizi: interfaccia -> implementazione, con lifetime Scoped
// (stesso lifetime del DbContext che usano; un Singleton NON potrebbe dipendere da un DbContext Scoped).
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IProjectService, ProjectService>();

// Gestione globale degli errori -> ProblemDetails
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// CORS: il browser blocca le chiamate da http://localhost:5173 (React) a http://localhost:5080 (API)
// perché sono "origin" diverse. Qui autorizziamo esplicitamente il frontend.
const string FrontendCorsPolicy = "Frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
        .AllowAnyHeader()       //non in produzione
        .AllowAnyMethod());     //non in produzione
});

// Documento OpenAPI (descrizione machine-readable delle API)
builder.Services.AddOpenApi();

var app = builder.Build();

// ---------- Creazione DB + dati di esempio ----------
// Creiamo uno "scope" manuale perché AppDbContext è Scoped e qui non siamo dentro una richiesta HTTP.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // EnsureCreated crea il DB e le tabelle se non esistono. Semplice per iniziare.
    // STEP SUCCESSIVO (vedi guida): sostituirlo con le Migrations -> db.Database.Migrate();
    db.Database.EnsureCreated();
    DbSeeder.Seed(db);
}

// ---------- 2) PIPELINE HTTP (l'ordine conta!) ----------
// Ogni richiesta attraversa i middleware dall'alto verso il basso, la risposta torna indietro al contrario.

app.UseExceptionHandler();          // cattura le eccezioni dei middleware successivi

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();               // JSON su /openapi/v1.json
    app.MapScalarApiReference();    // UI interattiva su /scalar (equivalente di Swagger UI)
}

app.UseCors(FrontendCorsPolicy);

app.MapControllers();               // routing verso i controller

app.Run();
