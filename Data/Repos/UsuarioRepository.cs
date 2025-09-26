using System.Data;
using Microsoft.Data.SqlClient;
using Inmobiliaria1.Models;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly string _cs;

    public UsuarioRepository(IConfiguration configuration)
    {
        _cs = configuration.GetConnectionString("DefaultConnection")!;
    }
    private static Usuario Map(SqlDataReader rd) => new()
    {
        Id = rd.GetInt32("Id"),
        Email = rd.GetString("Email"),
        PasswordHash = rd.GetString("PasswordHash"),
        Avatar = rd.IsDBNull(rd.GetOrdinal("Avatar")) ? null : rd.GetString(rd.GetOrdinal("Avatar")),
        Rol = Enum.Parse<RolUsuario>(rd.GetString(rd.GetOrdinal("Rol"))),
        Activo = rd.GetBoolean("Activo")
    };
    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(@"SELECT Id,Email,PasswordHash,Avatar,Rol,Activo
                                         FROM Usuario WHERE Id=@id", cn);
        cmd.Parameters.AddWithValue("@id", id);
        await cn.OpenAsync();
        using var rd = await cmd.ExecuteReaderAsync();
        return await rd.ReadAsync() ? Map(rd) : null;
    }
    public async Task<Usuario?> ObtenerPorEmailAsync(string email)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(@"SELECT Id,Email,PasswordHash,Avatar,Rol,Activo
                                         FROM Usuario WHERE Email=@email", cn);
        cmd.Parameters.AddWithValue("@Email", email);
        await cn.OpenAsync();
        using var rd = await cmd.ExecuteReaderAsync();
        return await rd.ReadAsync() ? Map(rd) : null;
    }
    public async Task<int> AltaAsync(Usuario u)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(@"
            INSERT INTO Usuario(Email,PasswordHash,Avatar,Rol,Activo)
            OUTPUT INSERTED.Id
            VALUES(@Email,@PasswordHash,@Avatar,@Rol,@Activo)", cn);
        cmd.Parameters.AddWithValue("@Email", u.Email);
        cmd.Parameters.AddWithValue("@PasswordHash", u.PasswordHash);
        cmd.Parameters.AddWithValue("@Avatar", (object?)u.Avatar ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Rol", u.Rol.ToString());
        cmd.Parameters.AddWithValue("@Activo", u.Activo);
        await cn.OpenAsync();
        return (int)await cmd.ExecuteScalarAsync();
    }
    public async Task ModificarAsync(Usuario u)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(@"
            UPDATE Usuario
               SET Email=@Email, PasswordHash=@PasswordHash, Avatar=@Avatar, Rol=@Rol, Activo=@Activo
             WHERE Id=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", u.Id);
        cmd.Parameters.AddWithValue("@Email", u.Email);
        cmd.Parameters.AddWithValue("@PasswordHash", u.PasswordHash);
        cmd.Parameters.AddWithValue("@Avatar", (object?)u.Avatar ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Rol", u.Rol.ToString());
        cmd.Parameters.AddWithValue("@Activo", u.Activo);
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    public async Task ModificarPerfilAsync(Usuario u)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(@"
            UPDATE Usuario
               SET Email=@Email, Avatar=@Avatar
             WHERE Id=@Id", cn);

        cmd.Parameters.AddWithValue("@Id", u.Id);
        cmd.Parameters.AddWithValue("@Email", u.Email);
        cmd.Parameters.AddWithValue("@Avatar", (object?)u.Avatar ?? DBNull.Value);
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    public async Task CmabiarPasswordAsync(int id, string Hash)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(@"
            UPDATE Usuario
               SET PasswordHash=@PasswordHash
             WHERE Id=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@PasswordHash", Hash);
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    public async Task CambiarAvatarAsync(int id, string? avatar)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(@"
            UPDATE Usuario
               SET Avatar=@Avatar
             WHERE Id=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@Avatar", (object?)avatar ?? DBNull.Value);
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    public async Task CambiarRolAsync(int id, RolUsuario rol)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(@"
            UPDATE Usuario
               SET Rol=@Rol
             WHERE Id=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@Rol", rol.ToString());
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    public async Task CambiarActivoAsync(int id, bool activo)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(@"
            UPDATE Usuario
               SET Activo=@Activo
             WHERE Id=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@Activo", activo);
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    public async Task<List<Usuario>> ListarAsync(bool incluirInactivos = false)
    {
        var res = new List<Usuario>();
        using var cn = new SqlConnection(_cs);
        var sql = @"SELECT Id,Email,PasswordHash,Avatar,Rol,Activo
                    FROM Usuario " + (incluirInactivos ? "" : "WHERE Activo=1 ") + "ORDER BY Email";
        using var cmd = new SqlCommand(sql, cn);
        await cn.OpenAsync();
        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync()) res.Add(Map(rd));
        return res;
    }
    public async Task EliminarAsync(int id)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand("DELETE FROM Usuario WHERE Id=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", id);
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
}