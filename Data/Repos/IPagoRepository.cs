using Inmobiliaria1.Models;

namespace Inmobiliaria1.Data.Repos;

public interface IPagoRepository
{
    Task<int> AltaAsync(Pago p);
    Task<int> ModificarAsync(Pago p);
    Task<int> BorrarAsync(int id);

    Task<Pago?> ObtenerPorIdAsync(int id);
    Task<List<Pago>> ObtenerPorContratoAsync(int contratoId);
    Task<decimal> TotalPagadoAsync(int contratoId);

    Task<int> AnularAsync(int idPago, int anuladoPorUsuarioId);

}