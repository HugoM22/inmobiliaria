using System.Data;
using Inmobiliaria1.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Inmobiliaria1.Data.Repos;

public class PropietarioRepository : IPropietarioRepository
{
    private readonly string _cs;
    public PropietarioRepository(IConfiguration configuration)
    {
        _cs = configuration.GetConnectionString("DefaultConnection")!;
    }
    public async Task<int> AltaAsync(Propietario p)
    {
        const string sql = @"
        INSERT INTO 
        Propietarios (DNI, Nombre, Apellido, Telefono, Email, CreatedAt, UpdatedAt)
        VALUES (@DNI, @Nombre, @Apellido, @Telefono, @Email, SYSDATETIME(), SYSDATETIME());
        SELECT CAST(SCOPE_IDENTITY() AS int);";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@DNI", SqlDbType.NVarChar, 20) { Value = p.DNI });
        cmd.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.NVarChar, 50) { Value = p.Nombre });
        cmd.Parameters.Add(new SqlParameter("@Apellido", SqlDbType.NVarChar, 50) { Value = p.Apellido });
        cmd.Parameters.Add(new SqlParameter("@Telefono", SqlDbType.NVarChar, 20) { Value = (object?)p.Telefono ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = (object?)p.Email ?? DBNull.Value });

        await cn.OpenAsync();
        var scalar = await cmd.ExecuteScalarAsync();
        if (scalar is null || scalar is DBNull)
            throw new InvalidOperationException("No se pudo obtener el Id (SCOPE_IDENTITY).");
        var id = Convert.ToInt32(scalar);
        p.Id = id;
        return id;
    }
    public async Task<int> ModificarAsync(Propietario p)
    {
        const string sql = @"
        UPDATE Propietarios
        SET DNI = @DNI,
            Nombre = @Nombre,
            Apellido = @Apellido,
            Telefono = @Telefono,
            Email = @Email,
            UpdatedAt = SYSDATETIME()
        WHERE Id = @Id;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = p.Id });
        cmd.Parameters.Add(new SqlParameter("@DNI", SqlDbType.NVarChar, 20) { Value = p.DNI });
        cmd.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.NVarChar, 50) { Value = p.Nombre });
        cmd.Parameters.Add(new SqlParameter("@Apellido", SqlDbType.NVarChar, 50) { Value = p.Apellido });
        cmd.Parameters.Add(new SqlParameter("@Telefono", SqlDbType.NVarChar, 20) { Value = (object?)p.Telefono ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = (object?)p.Email ?? DBNull.Value });

        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> BajaAsync(int id)
    {
        const string sql = "DELETE FROM Propietarios WHERE Id = @Id;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });

        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<Propietario?> ObtenerPorIdAsync(int id)
    {
        const string sql = "SELECT * FROM Propietarios WHERE Id = @Id;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });

        await cn.OpenAsync();
        using var dr = await cmd.ExecuteReaderAsync();
        if (await dr.ReadAsync())
        {
            return new Propietario
            {
                Id = dr.GetInt32(0),
                DNI = dr.GetString(1),
                Nombre = dr.GetString(2),
                Apellido = dr.GetString(3),
                Telefono = dr.IsDBNull(4) ? null : dr.GetString(4),
                Email = dr.IsDBNull(5) ? null : dr.GetString(5),
                CreatedAt = dr.IsDBNull(6) ? null : dr.GetDateTime(6),
                UpdatedAt = dr.IsDBNull(7) ? null : dr.GetDateTime(7)
            };
        }
        return null;
    }
    public async Task<List<Propietario>> ObtenerTodosAsync(string? q = null)
    {
        var sql = @"SELECT * FROM Propietarios";
        if (!string.IsNullOrWhiteSpace(q))
        {
            sql += " WHERE Nombre LIKE @q OR Apellido LIKE @q OR DNI LIKE @q";
        }
        sql += " ORDER BY Apellido, Nombre;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        if (!string.IsNullOrWhiteSpace(q))
        {
            cmd.Parameters.Add(new SqlParameter("@q", SqlDbType.NVarChar, 100) { Value = $"%{q}%" });
        }

        await cn.OpenAsync();
        using var dr = await cmd.ExecuteReaderAsync();
        var lista = new List<Propietario>();
        while (await dr.ReadAsync())
        {
            lista.Add(new Propietario
            {
                Id = dr.GetInt32(0),
                DNI = dr.GetString(1),
                Nombre = dr.GetString(2),
                Apellido = dr.GetString(3),
                Telefono = dr.IsDBNull(4) ? null : dr.GetString(4),
                Email = dr.IsDBNull(5) ? null : dr.GetString(5),
                CreatedAt = dr.IsDBNull(6) ? null : dr.GetDateTime(6),
                UpdatedAt = dr.IsDBNull(7) ? null : dr.GetDateTime(7)
            });
        }
        return lista;
    }

}