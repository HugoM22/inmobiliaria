using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria1.Models;

public class Contrato
{
    public int Id { get; set; }

    [Required]
    public int InmuebleId { get; set; }

    [Required]
    public int InquilinoId { get; set; }

    [Range(0.01, 9999999)]
    [DataType(DataType.Currency)]
    public decimal MontoMensual { get; set; }

    [Required]
    public EstadoContrato Estado { get; set; } = EstadoContrato.Vigente;

    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; }

    [DataType(DataType.Date)]
    public DateTime FechaFin { get; set; }

    [DataType(DataType.Date)]
    public DateTime? FechaFinEfectiva { get; set; }

    public int? CreadoPorUsuarioId { get; set; }
    public int? FinalizadoPorUsuarioId { get; set; }

    public Inmueble? Inmueble { get; set; }
    public Inquilino? Inquilino { get; set; }
    public Usuario? CreadoPor { get; set; }
    public Usuario? FinalizadoPor { get; set; }

}