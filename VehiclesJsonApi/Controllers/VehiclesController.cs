using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using VehiclesApi.Models;

namespace VehiclesApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config; // leer configuración de appsettings.json

        public VehiclesController(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;
        }

        // endpoint para motos
        [HttpGet("motos")]
        public IActionResult GetMotos()
        {
            // ruta al archivo JSON
            var path = Path.Combine(_env.ContentRootPath, "Data", "motos.json");
            if (!System.IO.File.Exists(path)) 
                return NotFound("Archivo motos.json no encontrado");

            // se lee el contenido del archivo
            var json = System.IO.File.ReadAllText(path);

            // ?? new List<Moto>() para evitar que sea null si falla la deserialización
            var motos = JsonSerializer.Deserialize<List<Moto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Ignora mayúsculas/minúsculas
            }) ?? new List<Moto>();

            // se lee la configuración desde appsettings.json
            var accountName = _config["AzureStorage:AccountName"];
            var container = _config["AzureStorage:Container"];
            var sasToken = _config["AzureStorage:BlobSasToken"];

            // se genera automáticamente la URL de la imagen usando el Id y el SAS Token
            foreach (var moto in motos)
            {
                moto.ImagenUrl = $"https://{accountName}.blob.core.windows.net/{container}/{moto.Id}.jpg?{sasToken}";
            }

            // se devuelve la lista como respuesta
            return Ok(motos);
        }

        // endpoint para carros
        [HttpGet("carros")]
        public IActionResult GetCarros()
        {
            // ruta al archivo JSON
            var path = Path.Combine(_env.ContentRootPath, "Data", "carros.json");
            if (!System.IO.File.Exists(path)) 
                return NotFound("Archivo carros.json no encontrado");

            // se lee el contenido del archivo
            var json = System.IO.File.ReadAllText(path);

            // ?? new List<Carro>() para evitar que sea null si falla la deserialización
            var carros = JsonSerializer.Deserialize<List<Carro>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Carro>();

            // se lee la configuración desde appsettings.json
            var accountName = _config["AzureStorage:AccountName"];
            var container = _config["AzureStorage:Container"];
            var sasToken = _config["AzureStorage:BlobSasToken"];

            // se genera automáticamente la URL de la imagen usando el Id y el SAS Token
            foreach (var carro in carros)
            {
                carro.ImagenUrl = $"https://{accountName}.blob.core.windows.net/{container}/{carro.Id}.jpg?{sasToken}";
            }

            // se devuelve la lista como respuesta
            return Ok(carros);
        }
    }
}
