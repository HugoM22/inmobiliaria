namespace Inmobiliaria1.Data.Repos;

using Inmobiliaria1.Models;

public interface IPropietarioRepository
{
    Task<int> AltaAsync(Propietario p);
    Task<int> ModificarAsync(Propietario p);
    Task<int> BajaAsync(int id);
    Task<Propietario?> ObtenerPorIdAsync(int id);
    Task<List<Propietario>> ObtenerTodosAsync(string? q=null);
}