using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria1.Models;

public enum RolUsuario
{
    Administrador = 1,
    Empleado = 2
}

public enum EstadoContrato
{
    Borrador = 1,
    Vigente = 2,
    Finalizado = 3,
    Rescindido = 4
}

public enum EstadoPago
{
    Activo = 1,
    Vencido = 2,
    Anulado = 3
}

public enum Uso
{
    Residencial = 1,
    Comercial = 2
}

public enum EstadoInmueble
{
    [Display(Name = "Disponible")]
    Publicado = 1,
    [Display(Name = "Suspendido")]
    Suspendido = 2,
    [Display(Name = "En uso")]
    EnUso = 3
}