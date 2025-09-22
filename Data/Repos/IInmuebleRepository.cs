using Inmobiliaria1.Models;

namespace Inmobiliaria1.Data.Repos;

public interface IInmuebleRepository
{
    Task<int> AltaAsync(Inmueble i);
    Task<int> ModificarAsync(Inmueble i);
    Task<int> BajaAsync(int id);
    Task<Inmueble?> ObtenerPorIdAsync(int id);
    Task<List<Inmueble>> ObtenerTodosAsync(string? q = null, int? propietarioId = null, int? tipoId = null);
}