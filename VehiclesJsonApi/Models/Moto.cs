using System.Text.Json.Serialization;

namespace VehiclesApi.Models
{
    public class Moto
    {
        public int Id { get; set; }
        public string? Make { get; set; }
        public string? Name { get; set; }
        public int Año { get; set; }

        [JsonPropertyName("imagenurl")]
        public string? ImagenUrl { get; set; }

        public Atributos? Atributos { get; set; }
    }

    public class Atributos
    {
        public string? Motor { get; set; }
        public string? Transmision { get; set; }
        public string? Potencia { get; set; }
        public string? Autonomia { get; set; }
        public string? Frenos { get; set; }
    }
}
