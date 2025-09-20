using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria1.Models;

public class TipoInmueble
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Descripcion { get; set; } = "";
}
