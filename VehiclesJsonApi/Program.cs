using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173",
                    "https://localhost:5173",
                    "http://localhost:4173"
                    "https://vehicle-information-web.onrender.com" 
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

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

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vehicles API V1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

// cors
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
