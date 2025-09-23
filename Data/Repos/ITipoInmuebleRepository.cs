using Inmobiliaria1.Models;

namespace Inmobiliaria1.Data.Repos;

public interface ITipoInmuebleRepository
{
    Task<List<TipoInmueble>> ObtenerTodosAsync();
    Task<TipoInmueble?> ObtenerPorIdAsync(int id);
    Task<int> AltaAsync(TipoInmueble x);
    Task<int> ModificarAsync(TipoInmueble x);
    Task<int> BajaAsync(int id);
}
