using System.ComponentModel.DataAnnotations;


namespace Inmobiliaria1.Models;


public class Inquilino
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    [RegularExpression(@"^\d{7,10}$", ErrorMessage = "DNI inválido.")]
    public string DNI { get; set; } = "";

    [Required, StringLength(50)]
    public string Nombre { get; set; } = "";

    [Required, StringLength(50)]
    public string Apellido { get; set; } = "";

    [StringLength(15)]
    public string? Telefono { get; set; }

    [EmailAddress, StringLength(100)]
    public string? Email { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}