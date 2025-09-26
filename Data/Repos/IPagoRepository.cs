using Inmobiliaria1.Models;

namespace Inmobiliaria1.Data.Repos;

public interface IPagoRepository
{
    Task<List<Pago>> ObtenerPorContratoAsync(int contratoId);
    Task<Pago?> ObtenerPorIdAsync(int id);
    Task<decimal> TotalPagadoAsync(int contratoId);

    Task<int> AltaAsync(Pago p);                          
    Task<int> EditarDetalleAsync(int id, string? detalle); 
    Task<int> AnularAsync(int idPago, int anuladoPorUsuarioId); 

    Task<int> BorrarAsync(int id);
}
