using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
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
        private readonly IMemoryCache _cache;

        // TTLs
        private static readonly TimeSpan CarsCacheTtl = TimeSpan.FromHours(6);
        private static readonly TimeSpan MotorcyclesCacheTtl = TimeSpan.FromHours(6);
        private static readonly TimeSpan TrimsCacheTtl = TimeSpan.FromHours(12);

        // Bloqueo temporal global cuando CarAPI responde 429
        private static DateTime _carApiBlockedUntil = DateTime.MinValue;
        private static readonly object _blockLock = new();

        public VehiclesController(
            IWebHostEnvironment env,
            IConfiguration config,
            IHttpClientFactory httpFactory,
            IMemoryCache cache)
        {
            _env = env;
            _config = config;
            _cache = cache;

            _http = httpFactory.CreateClient();
            _http.DefaultRequestHeaders.Add("User-Agent", "VehiclesApi/1.0");
            _http.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        // ======================
        // PUENTE A CARAPI
        // ======================

        [HttpGet("carapi/cars")]
        public async Task<IActionResult> GetCarsFromCarApi()
        {
            const string cacheKey = "carapi:cars";

            if (_cache.TryGetValue(cacheKey, out string cached))
                return Content(cached, "application/json");

            if (DateTime.UtcNow < _carApiBlockedUntil)
                return StatusCode(503, "CarAPI temporalmente bloqueada");

            try
            {
                var url = "https://carapi.app/api/submodels/v2?sort=Makes.name&direction=asc";
                var response = await _http.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    lock (_blockLock)
                        _carApiBlockedUntil = DateTime.UtcNow.AddHours(1);

                    return StatusCode(503, "CarAPI alcanzó el límite de peticiones");
                }

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, body);

                _cache.Set(cacheKey, body, CarsCacheTtl);
                return Content(body, "application/json");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new
                {
                    error = "No se pudo contactar CarAPI",
                    detail = ex.Message
                });
            }
            catch (TaskCanceledException)
            {
                return StatusCode(504, "Timeout llamando a CarAPI");
            }
        }

        [HttpGet("carapi/motorcycles")]
        public async Task<IActionResult> GetMotorcyclesFromCarApi()
        {
            const string cacheKey = "carapi:motorcycles";

            if (_cache.TryGetValue(cacheKey, out string cached))
                return Content(cached, "application/json");

            if (DateTime.UtcNow < _carApiBlockedUntil)
                return StatusCode(503, "CarAPI temporalmente bloqueada");

            try
            {
                var url = "https://carapi.app/api/models/powersports?sort=Makes.name&direction=asc&type=street_motorcycle";
                var response = await _http.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    lock (_blockLock)
                        _carApiBlockedUntil = DateTime.UtcNow.AddHours(1);

                    return StatusCode(503, "CarAPI alcanzó el límite de peticiones");
                }

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, body);

                _cache.Set(cacheKey, body, MotorcyclesCacheTtl);
                return Content(body, "application/json");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new
                {
                    error = "No se pudo contactar CarAPI",
                    detail = ex.Message
                });
            }
            catch (TaskCanceledException)
            {
                return StatusCode(504, "Timeout llamando a CarAPI");
            }
        }

        [HttpGet("carapi/trims/by-submodel/{submodelId}")]
        public async Task<IActionResult> GetTrimsBySubmodel(int submodelId)
        {
            var cacheKey = $"carapi:trims:submodel:{submodelId}";

            if (_cache.TryGetValue(cacheKey, out string cached))
                return Content(cached, "application/json");

            if (DateTime.UtcNow < _carApiBlockedUntil)
                return StatusCode(503, "CarAPI temporalmente bloqueada");

            try
            {
                var url = $"https://carapi.app/api/trims/v2?sort=Makes.name&direction=asc&submodel_id={submodelId}";
                var response = await _http.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    lock (_blockLock)
                        _carApiBlockedUntil = DateTime.UtcNow.AddHours(1);

                    return StatusCode(503, "CarAPI alcanzó el límite de peticiones");
                }

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, body);

                _cache.Set(cacheKey, body, TrimsCacheTtl);
                return Content(body, "application/json");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new
                {
                    error = "No se pudo contactar CarAPI",
                    detail = ex.Message
                });
            }
            catch (TaskCanceledException)
            {
                return StatusCode(504, "Timeout llamando a CarAPI");
            }
        }

        [HttpGet("carapi/trims/{trimId}")]
        public async Task<IActionResult> GetTrimDetail(int trimId)
        {
            var cacheKey = $"carapi:trims:detail:{trimId}";

            if (_cache.TryGetValue(cacheKey, out string cached))
                return Content(cached, "application/json");

            if (DateTime.UtcNow < _carApiBlockedUntil)
                return StatusCode(503, "CarAPI temporalmente bloqueada");

            try
            {
                var url = $"https://carapi.app/api/trims/v2/{trimId}";
                var response = await _http.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    lock (_blockLock)
                        _carApiBlockedUntil = DateTime.UtcNow.AddHours(1);

                    return StatusCode(503, "CarAPI alcanzó el límite de peticiones");
                }

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, body);

                _cache.Set(cacheKey, body, TrimsCacheTtl);
                return Content(body, "application/json");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new
                {
                    error = "No se pudo contactar CarAPI",
                    detail = ex.Message
                });
            }
            catch (TaskCanceledException)
            {
                return StatusCode(504, "Timeout llamando a CarAPI");
            }
        }

        // ======================
        // MOTOS (JSON + AZURE)
        // ======================

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

        // ======================
        // CARROS (JSON + AZURE)
        // ======================

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
