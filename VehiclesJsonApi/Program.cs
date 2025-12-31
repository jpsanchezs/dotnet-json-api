using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Vehicles API",
        Version = "v1",
        Description = "API para manejar datos de vehículos (carros y motos)"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Swagger siempre, no solo en Development
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vehicles API V1");
    c.RoutePrefix = "swagger"; // URL será /swagger
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
