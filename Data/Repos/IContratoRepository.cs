using Inmobiliaria1.Models;

namespace Inmobiliaria1.Data.Repos;

public interface IContratoRepository
{
    Task<int> AltaAsync(Contrato c);
    Task<int> ModificarAsync(Contrato c);
    Task<int> BorrarAsync(int id);
    Task<Contrato?> ObtenerPorIdAsync(int id);
    Task<List<Contrato>> ObtenerTodosAsync(string? q = null, int? inmuebleId = null, int? inquilinoId = null);
    Task<bool> ExisteSolapamientoAsync(int inmuebleId, DateTime inicio, DateTime fin, int? excluirId = null);

    Task<int> FinalizarAsync(int idContrato, int finalizadoPorUsuarioId,DateTime fechaFinEfectiva, EstadoContrato estado = EstadoContrato.Finalizado);
}