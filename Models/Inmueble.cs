using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria1.Models;

public class Inmueble
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Direccion { get; set; } = "";

    [Required]
    public Uso Uso { get; set; }

    [Range(1, 1000)]
    public int Ambientes { get; set; }

    [Range(0.01, 9999999)]
    [DataType(DataType.Currency)]
    public decimal Precio { get; set; }

    [Required]
    public EstadoInmueble Estado { get; set; } = EstadoInmueble.Publicado;

    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }

    [Required]
    public int PropietarioId { get; set; }

    public TipoInmueble? TipoInmueble { get; set; }
    public Propietario? Propietario { get; set; }
}
