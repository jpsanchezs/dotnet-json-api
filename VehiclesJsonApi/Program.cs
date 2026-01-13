using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// =============================
// Services
// =============================

// HttpClient para consumir CarAPI
builder.Services.AddHttpClient();

builder.Services.AddMemoryCache();

// Controllers
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:4173",
                "https://vehicle-information-web.onrender.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
            // ❌ NO AllowCredentials (no usas cookies)
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Vehicles API",
        Version = "v1"
    });
});

var app = builder.Build();

// =============================
// Middleware pipeline
// =============================

// Swagger (puede estar en prod sin problema)
app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection(); // opcional

app.UseRouting();

// CORS DEBE ir aquí
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
