using System.Data;
using Microsoft.Data.SqlClient;
using Inmobiliaria1.Models;

namespace Inmobiliaria1.Data.Repos;

public class TipoInmuebleRepository : ITipoInmuebleRepository
{
    private readonly string _cs;
    public TipoInmuebleRepository(IConfiguration cfg) =>
        _cs = cfg.GetConnectionString("DefaultConnection")!;

    public async Task<List<TipoInmueble>> ObtenerTodosAsync()
    {
        const string sql = "SELECT Id, Descripcion FROM TipoInmuebles ORDER BY Descripcion;";
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        await cn.OpenAsync();
        using var rd = await cmd.ExecuteReaderAsync();
        var list = new List<TipoInmueble>();
        while (await rd.ReadAsync())
            list.Add(new TipoInmueble { Id = rd.GetInt32(0), Descripcion = rd.GetString(1) });
        return list;
    }

    public async Task<TipoInmueble?> ObtenerPorIdAsync(int id)
    {
        const string sql = "SELECT Id, Descripcion FROM TipoInmuebles WHERE Id=@Id;";
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new("@Id", SqlDbType.Int){ Value = id });
        await cn.OpenAsync();
        using var rd = await cmd.ExecuteReaderAsync();
        if (!await rd.ReadAsync()) return null;
        return new TipoInmueble { Id = rd.GetInt32(0), Descripcion = rd.GetString(1) };
    }

    public async Task<int> AltaAsync(TipoInmueble x)
    {
        const string sql = "INSERT INTO TipoInmuebles (Descripcion) VALUES (@Descripcion); SELECT CAST(SCOPE_IDENTITY() AS int);";
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new("@Descripcion", SqlDbType.NVarChar, 100){ Value = x.Descripcion });
        await cn.OpenAsync();
        x.Id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        return x.Id;
    }

    public async Task<int> ModificarAsync(TipoInmueble x)
    {
        const string sql = "UPDATE TipoInmuebles SET Descripcion=@Descripcion WHERE Id=@Id;";
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new("@Id", SqlDbType.Int){ Value = x.Id });
        cmd.Parameters.Add(new("@Descripcion", SqlDbType.NVarChar, 100){ Value = x.Descripcion });
        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> BajaAsync(int id)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand("DELETE FROM TipoInmuebles WHERE Id=@Id;", cn);
        cmd.Parameters.Add(new("@Id", SqlDbType.Int){ Value = id });
        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }
}
