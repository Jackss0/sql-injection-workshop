using Microsoft.EntityFrameworkCore;
using SqlInjectionWorkshop.Data;

var builder = WebApplication.CreateBuilder(args);

// Configurar logging detallado para demostración
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "SQL Injection Workshop API", 
        Version = "v1",
        Description = "API de demostración para workshop de SQL Injection - Incluye endpoints vulnerables y seguros"
    });
});

// Configurar Entity Framework con base de datos en memoria
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("SqlInjectionWorkshop"));

var app = builder.Build();

// Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SQL Injection Workshop API v1");
        c.RoutePrefix = string.Empty; // Swagger UI en la raíz
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Inicializar base de datos con datos de prueba
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    
    // Log de inicialización
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("🚀 SQL Injection Workshop API iniciada");
    logger.LogWarning("⚠️  ATENCIÓN: Esta API contiene endpoints vulnerables intencionalmente para fines educativos");
    logger.LogInformation("📊 Base de datos en memoria inicializada con datos de prueba");
}

app.Run();