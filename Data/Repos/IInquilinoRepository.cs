namespace Inmobiliaria1.Data.Repos;

using Inmobiliaria1.Models;

public interface IInquilinoRepository
{
    Task<int> AltaAsync(Inquilino i);
    Task<int> ModificarAsync(Inquilino i);
    Task<int> BajaAsync(int id);
    Task<Inquilino?> ObtenerPorIdAsync(int id);
    Task<List<Inquilino>> ObtenerTodosAsync(string? q=null);
}