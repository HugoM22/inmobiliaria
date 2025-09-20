namespace Inmobiliaria1.Models;

public enum RolUsuario
{
    SuperAdmin = 1,
    Admin = 2,
    Empleado = 3
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
    Publicado = 1,
    Suspendido = 2,
    EnUso = 3
}