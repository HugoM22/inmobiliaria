using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria1.Models;

public class Pago
{
    public int Id { get; set; }

    public int Numero { get; set; }

    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; } = DateTime.Today;

    [Display(Name = "Importe")]
    [Range(0.01, 9999999, ErrorMessage = "El {0} debe ser un número válido mayor que 0.")]
    [DataType(DataType.Currency)]
    public decimal Importe { get; set; }

    [StringLength(200)]
    public string? Detalle { get; set; }

    [Required, EnumDataType(typeof(EstadoPago))]
    public EstadoPago Estado { get; set; } = EstadoPago.Activo;

    public bool Multa { get; set; } = false;

    [Required]
    public int ContratoId { get; set; }

    [Required]
    public int CreadoPorUsuarioId { get; set; }

    public int? AnuladoPorUsuarioId { get; set; }

    public Contrato? Contrato { get; set; }
    public Usuario? CreadoPor { get; set; }
    public Usuario? AnuladoPor { get; set; }
}
