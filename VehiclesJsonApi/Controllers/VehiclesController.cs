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

        public VehiclesController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet("motos")]
        public IActionResult GetMotos()
        {
            var path = Path.Combine(_env.ContentRootPath, "Data", "motos.json");
            if (!System.IO.File.Exists(path)) return NotFound("Archivo motos.json no encontrado");

            var json = System.IO.File.ReadAllText(path);

            // ignora mayúsculas/minúsculas
            var motos = JsonSerializer.Deserialize<List<Moto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return Ok(motos);
        }

        [HttpGet("carros")]
        public IActionResult GetCarros()
        {
            var path = Path.Combine(_env.ContentRootPath, "Data", "carros.json");
            if (!System.IO.File.Exists(path)) return NotFound("Archivo carros.json no encontrado");

            var json = System.IO.File.ReadAllText(path);

            // ignora mayúsculas/minúsculas
            var carros = JsonSerializer.Deserialize<List<Carro>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return Ok(carros);
        }
    }
}
