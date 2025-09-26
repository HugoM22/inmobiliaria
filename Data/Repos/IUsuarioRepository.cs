
using Inmobiliaria1.Models;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorIdAsync(int id);
    Task<Usuario?> ObtenerPorEmailAsync(string email);

    Task<int> AltaAsync(Usuario u); //admin
    Task ModificarAsync(Usuario u); //admin
    Task ModificarPerfilAsync(Usuario u);
    Task CmabiarPasswordAsync(int id, string Hash);
    Task CambiarAvatarAsync(int id, string? avatar);
    Task CambiarRolAsync(int id, RolUsuario rol); //admin
    Task CambiarActivoAsync(int id, bool activo); //admin

    Task<List<Usuario>> ListarAsync(bool incluirInactivos = false);
    Task EliminarAsync(int id); //admin
    
}