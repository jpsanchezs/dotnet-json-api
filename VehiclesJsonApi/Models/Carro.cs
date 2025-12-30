using System.Text.Json.Serialization;

namespace VehiclesApi.Models
{
    public class Carro
    {
        public int Id { get; set; }

        [JsonPropertyName("imagenurl")]
        public string? ImagenUrl { get; set; }
    }
}
