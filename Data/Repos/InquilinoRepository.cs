using System.Data;
using Inmobiliaria1.Models;
using Microsoft.Data.SqlClient;

namespace Inmobiliaria1.Data.Repos;

public class InquilinoRepository : IInquilinoRepository
{
    private readonly string _cs;
    public InquilinoRepository(IConfiguration configuration)
    {
        _cs = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<int> AltaAsync(Inquilino i)
    {
        const string sql = @"
        INSERT INTO 
        Inquilinos (DNI, Nombre, Apellido, Telefono, Email, CreatedAt, UpdatedAt)
        VALUES (@DNI, @Nombre, @Apellido, @Telefono, @Email, SYSDATETIME(), SYSDATETIME());
        SELECT CAST(SCOPE_IDENTITY() AS int);";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@DNI", SqlDbType.NVarChar, 20) { Value = i.DNI });
        cmd.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.NVarChar, 50) { Value = i.Nombre });
        cmd.Parameters.Add(new SqlParameter("@Apellido", SqlDbType.NVarChar, 50) { Value = i.Apellido });
        cmd.Parameters.Add(new SqlParameter("@Telefono", SqlDbType.NVarChar, 20) { Value = (object?)i.Telefono ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = (object?)i.Email ?? DBNull.Value });

        await cn.OpenAsync();
        var scalar = await cmd.ExecuteScalarAsync();
        if (scalar is null || scalar is DBNull)
            throw new InvalidOperationException("No se pudo obtener el Id (SCOPE_IDENTITY).");
        var id = Convert.ToInt32(scalar);
        i.Id = id;
        return id;
    }
    public async Task<int> ModificarAsync(Inquilino i)
    {
        const string sql = @"
        UPDATE Inquilinos
        SET DNI = @DNI,
            Nombre = @Nombre,
            Apellido = @Apellido,
            Telefono = @Telefono,
            Email = @Email,
            UpdatedAt = SYSDATETIME()
        WHERE Id = @Id;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = i.Id });
        cmd.Parameters.Add(new SqlParameter("@DNI", SqlDbType.NVarChar, 20) { Value = i.DNI });
        cmd.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.NVarChar, 50) { Value = i.Nombre });
        cmd.Parameters.Add(new SqlParameter("@Apellido", SqlDbType.NVarChar, 50) { Value = i.Apellido });
        cmd.Parameters.Add(new SqlParameter("@Telefono", SqlDbType.NVarChar, 20) { Value = (object?)i.Telefono ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = (object?)i.Email ?? DBNull.Value });

        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> BajaAsync(int id)
    {
        const string sql = "DELETE FROM Inquilinos WHERE Id = @Id;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });

        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<Inquilino?> ObtenerPorIdAsync(int id)
    {
        const string sql = "SELECT Id, DNI, Nombre, Apellido, Telefono, Email, CreatedAt, UpdatedAt FROM Inquilinos WHERE Id = @Id;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });

        await cn.OpenAsync();
        using var dr = await cmd.ExecuteReaderAsync();
        if (await dr.ReadAsync())
        {
            return new Inquilino
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
    public async Task<List<Inquilino>> ObtenerTodosAsync(string? q = null)
    {
        var sql = "SELECT Id, DNI, Nombre, Apellido, Telefono, Email, CreatedAt, UpdatedAt FROM Inquilinos";
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
        var lista = new List<Inquilino>();
        while (await dr.ReadAsync())
        {
            lista.Add(new Inquilino
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