using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inmobiliaria1.Models;

public class TipoInmueble
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    [Column("Nombre")]
    public string Descripcion { get; set; } = "";
}
