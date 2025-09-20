using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria1.Models;

public class Usuario
{
    public int Id { get; set; }
    [Required, EmailAddress, StringLength(120)]
    public string Email { get; set; } = "";
    [Required, StringLength(100)]
    public string Password { get; set; } = "";

    [StringLength(250)]
    public string? Avatar { get; set; }

    [Required]
    public RolUsuario Rol { get; set; } = RolUsuario.Empleado;

    public bool Activo { get; set; } = true; 
}