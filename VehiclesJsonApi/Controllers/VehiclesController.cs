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
        private readonly IConfiguration _config;
        private readonly HttpClient _http;

        public VehiclesController(
            IWebHostEnvironment env,
            IConfiguration config,
            IHttpClientFactory httpFactory)
        {
            _env = env;
            _config = config;

            _http = httpFactory.CreateClient();
            _http.DefaultRequestHeaders.Add("User-Agent", "VehiclesApi/1.0");
            _http.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        // PUENTE A CARAPI

        [HttpGet("carapi/cars")]
        public async Task<IActionResult> GetCarsFromCarApi()
        {
            var url = "https://carapi.app/api/submodels/v2?sort=Makes.name&direction=asc";
            var response = await _http.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, body);

            return Content(body, "application/json");
        }

        [HttpGet("carapi/motorcycles")]
        public async Task<IActionResult> GetMotorcyclesFromCarApi()
        {
            var url = "https://carapi.app/api/models/powersports?sort=Makes.name&direction=asc&type=street_motorcycle";
            var response = await _http.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, body);

            return Content(body, "application/json");
        }

        // Trims por submodelo (para obtener lista y así conseguir trimId)
        [HttpGet("carapi/trims/by-submodel/{submodelId}")]
        public async Task<IActionResult> GetTrimsBySubmodel(int submodelId)
        {
            var url = $"https://carapi.app/api/trims/v2?sort=Makes.name&direction=asc&submodel_id={submodelId}";
            var response = await _http.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, body);

            return Content(body, "application/json");
        }

        // Detalle completo de un trim específico
        [HttpGet("carapi/trims/{trimId}")]
        public async Task<IActionResult> GetTrimDetail(int trimId)
        {
            var url = $"https://carapi.app/api/trims/v2/{trimId}";
            var response = await _http.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, body);

            return Content(body, "application/json");
        }

        // MOTOS (JSON + AZURE)

        [HttpGet("motos")]
        public IActionResult GetMotos()
        {
            var path = Path.Combine(_env.ContentRootPath, "Data", "motos.json");
            if (!System.IO.File.Exists(path))
                return NotFound("Archivo motos.json no encontrado");

            var json = System.IO.File.ReadAllText(path);

            var motos = JsonSerializer.Deserialize<List<Moto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new List<Moto>();

            var accountName = _config["AzureStorage:AccountName"];
            var container = _config["AzureStorage:Container"];
            var sasToken = _config["AzureStorage:BlobSasToken"];

            foreach (var moto in motos)
            {
                moto.ImagenUrl =
                    $"https://{accountName}.blob.core.windows.net/{container}/{moto.Id}.jpg?{sasToken}";
            }

            return Ok(motos);
        }

        // CARROS (JSON + AZURE)

        [HttpGet("carros")]
        public IActionResult GetCarros()
        {
            var path = Path.Combine(_env.ContentRootPath, "Data", "carros.json");
            if (!System.IO.File.Exists(path))
                return NotFound("Archivo carros.json no encontrado");

            var json = System.IO.File.ReadAllText(path);

            var carros = JsonSerializer.Deserialize<List<Carro>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new List<Carro>();

            var accountName = _config["AzureStorage:AccountName"];
            var container = _config["AzureStorage:Container"];
            var sasToken = _config["AzureStorage:BlobSasToken"];

            foreach (var carro in carros)
            {
                carro.ImagenUrl =
                    $"https://{accountName}.blob.core.windows.net/{container}/{carro.Id}.jpg?{sasToken}";
            }

            return Ok(carros);
        }
    }
}
