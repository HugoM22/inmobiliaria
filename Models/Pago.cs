using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria1.Models;

public class Pago
{
    public int Id { get; set; }

    [Required, Range(1, 240)]
    public int Numero { get; set; }

    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; } = DateTime.Today;

    [Range(0.01, 9999999)]
    [DataType(DataType.Currency)]
    public decimal Importe { get; set; }

    [StringLength(200)]
    public string? Detalle { get; set; }

    [Required, StringLength(20)]
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