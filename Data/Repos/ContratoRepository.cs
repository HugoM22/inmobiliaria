using System.Data;
using Inmobiliaria1.Models;
using Microsoft.Data.SqlClient;

namespace Inmobiliaria1.Data.Repos;

public class ContratoRepository : IContratoRepository
{
    private readonly string _cs;
    public ContratoRepository(IConfiguration configuration)
    {
        _cs = configuration.GetConnectionString("DefaultConnection")!;
    }
    public async Task<int> AltaAsync(Contrato cont)
    {
        const string sql = @"INSERT INTO Contratos(
        InmuebleId, InquilinoId, MontoMensual, Estado, FechaInicio, FechaFin, FechaFinEfectiva, CreadoPorUsuarioId, FinalizadoPorUsuarioId)
        VALUES
        (@InmuebleId, @InquilinoId, @MontoMensual, @Estado, @FechaInicio, @FechaFin, @FechaFinEfectiva, @CreadoPorUsuarioId, @FinalizadoPorUsuarioId);
        SELECT CAST(SCOPE_IDENTITY() AS int);";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new("@InmuebleId", SqlDbType.Int) { Value = cont.InmuebleId });
        cmd.Parameters.Add(new("@InquilinoId", SqlDbType.Int) { Value = cont.InquilinoId });
        cmd.Parameters.Add(new("@MontoMensual", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = cont.MontoMesual });
        cmd.Parameters.Add(new("@Estado", SqlDbType.NVarChar, 50) { Value = cont.Estado });
        cmd.Parameters.Add(new("@FechaInicio", SqlDbType.Date) { Value = cont.FechaInicio });
        cmd.Parameters.Add(new("@FechaFin", SqlDbType.Date) { Value = cont.FechaFin });
        cmd.Parameters.Add(new("@FechaFinEfectiva", SqlDbType.Date) { Value = (object?)cont.FechaFinEfectiva ?? DBNull.Value });
        cmd.Parameters.Add(new("@CreadoPorUsuarioId", SqlDbType.Int) { Value = cont.CreadoPorUsuarioId });
        cmd.Parameters.Add(new("@FinalizadoPorUsuarioId", SqlDbType.Int) { Value = (object?)cont.FinalizadoPorUsuarioId ?? DBNull.Value });

        await cn.OpenAsync();
        var scalar = await cmd.ExecuteScalarAsync();
        if (scalar is null || scalar is DBNull) throw new InvalidOperationException("No se pudo obtener el Id.");
        cont.Id = Convert.ToInt32(scalar);
        return cont.Id;
    }

    public async Task<int> ModificarAsync(Contrato cont)
    {
        const string sql = @"UPDATE Contratos SET
        InmuebleId = @InmuebleId,
        InquilinoId = @InquilinoId,
        MontoMensual = @MontoMensual,
        Estado = @Estado,
        FechaInicio = @FechaInicio,
        FechaFin = @FechaFin,
        FechaFinEfectiva = @FechaFinEfectiva,
        CreadoPorUsuarioId = @CreadoPorUsuarioId,
        FinalizadoPorUsuarioId = @FinalizadoPorUsuarioId
        WHERE Id = @Id;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new("@InmuebleId", SqlDbType.Int) { Value = cont.InmuebleId });
        cmd.Parameters.Add(new("@InquilinoId", SqlDbType.Int) { Value = cont.InquilinoId });
        cmd.Parameters.Add(new("@MontoMensual", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = cont.MontoMesual });
        cmd.Parameters.Add(new("@Estado", SqlDbType.NVarChar, 50) { Value = cont.Estado });
        cmd.Parameters.Add(new("@FechaInicio", SqlDbType.Date) { Value = cont.FechaInicio });
        cmd.Parameters.Add(new("@FechaFin", SqlDbType.Date) { Value = cont.FechaFin });
        cmd.Parameters.Add(new("@FechaFinEfectiva", SqlDbType.Date) { Value = (object?)cont.FechaFinEfectiva ?? DBNull.Value });
        cmd.Parameters.Add(new("@CreadoPorUsuarioId", SqlDbType.Int) { Value = cont.CreadoPorUsuarioId });
        cmd.Parameters.Add(new("@FinalizadoPorUsuarioId", SqlDbType.Int) { Value = (object?)cont.FinalizadoPorUsuarioId ?? DBNull.Value });
        cmd.Parameters.Add(new("@Id", SqlDbType.Int) { Value = cont.Id });

        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> BorrarAsync(int id)
    {
        const string sql = "DELETE FROM Contratos WHERE Id = @Id;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new("@Id", SqlDbType.Int) { Value = id });

        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<Contrato?> ObtenerPorIdAsync(int id)
    {
        const string sql = @"SELECT c.Id, c.InmuebleId, c.InquilinoId, c.MontoMensual, c.Estado, c.FechaInicio, c.FechaFin, c.FechaFinEfectiva,
            c.CreadoPorUsuarioId, c.FinalizadoPorUsuarioId,
            i.Direccion,
            iq.Apellido, iq.Nombre,
            u1.Email AS CreadoPorEmail,
            u2.Email AS FinalizadoPorEmail
        FROM Contratos c
        JOIN Inmuebles  i  ON i.Id  = c.InmuebleId
        JOIN Inquilinos iq ON iq.Id = c.InquilinoId
        LEFT JOIN Usuario u1 ON u1.Id = c.CreadoPorUsuarioId
        LEFT JOIN Usuario u2 ON u2.Id = c.FinalizadoPorUsuarioId
        WHERE c.Id=@Id;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new("@Id", SqlDbType.Int) { Value = id });
        await cn.OpenAsync();

        using var dr = await cmd.ExecuteReaderAsync();
        if (!await dr.ReadAsync()) return null;

        return new Contrato
        {
            Id = dr.GetInt32(0),
            InmuebleId = dr.GetInt32(1),
            InquilinoId = dr.GetInt32(2),
            MontoMesual = dr.GetDecimal(3),
            Estado = Enum.Parse<EstadoContrato>(dr.GetString(4)),
            FechaInicio = dr.GetDateTime(5),
            FechaFin = dr.GetDateTime(6),
            FechaFinEfectiva = dr.GetDateTime(7),
            CreadoPorUsuarioId = dr.GetInt32(8),
            FinalizadoPorUsuarioId = dr.IsDBNull(9) ? null : dr.GetInt32(9),
            Inmueble = new Inmueble { Id = dr.GetInt32(1), Direccion = dr.GetString(10) },
            Inquilino = new Inquilino { Id = dr.GetInt32(2), Apellido = dr.GetString(11), Nombre = dr.GetString(12) },
            CreadoPor = dr.IsDBNull(13) ? null : new Usuario { Id = dr.GetInt32(8), Email = dr.GetString(13) },
            FinalizadoPor = dr.IsDBNull(14) ? null : new Usuario { Id = dr.GetInt32(9), Email = dr.GetString(14) }
        };
    }
    public async Task<List<Contrato>> ObtenerTodosAsync(string? q = null, int? inmuebleId = null, int? inquilinoId = null)
    {
        var sql = @"
    SELECT c.Id, c.InmuebleId, c.InquilinoId, c.MontoMensual, c.Estado, c.FechaInicio, c.FechaFin,
        i.Direccion
    FROM Contratos c
    JOIN Inmuebles i ON i.Id = c.InmuebleId
    JOIN Inquilinos iq ON iq.Id = c.InquilinoId
    WHERE 1=1";
        if (inmuebleId.HasValue) sql += "AND c.InmuebleId = @InmuebleId";
        if (inquilinoId.HasValue) sql += "AND c.InquilinoId = @InquilinoId";
        if (!string.IsNullOrWhiteSpace(q)) sql += "AND (i.Direccion LIKE @q OR iq.Apellido LIKE @q OR iq.Nombre LIKE @q)";
        sql += " ORDER BY c.FechaInicio DESC;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        if (inmuebleId.HasValue) cmd.Parameters.Add(new("@InmuebleId", SqlDbType.Int) { Value = inmuebleId.Value });
        if (inquilinoId.HasValue) cmd.Parameters.Add(new("@InquilinoId", SqlDbType.Int) { Value = inquilinoId.Value });
        if (!string.IsNullOrWhiteSpace(q)) cmd.Parameters.Add(new("@q", SqlDbType.NVarChar, 50) { Value = "%" + q + "%" });
        await cn.OpenAsync();
        using var dr = await cmd.ExecuteReaderAsync();
        var lista = new List<Contrato>();
        while (await dr.ReadAsync())
        {
            lista.Add(new Contrato
            {
                Id = dr.GetInt32(0),
                InmuebleId = dr.GetInt32(1),
                InquilinoId = dr.GetInt32(2),
                MontoMesual = dr.GetDecimal(3),
                Estado = Enum.Parse<EstadoContrato>(dr.GetString(4)),
                FechaInicio = dr.GetDateTime(5),
                FechaFin = dr.GetDateTime(6),
                Inmueble = new Inmueble { Id = dr.GetInt32(1), Direccion = dr.GetString(7) }
            });
        }
        return lista;
    }
    public async Task<int> FinalizarAsync(int contratoId, int finalizadoPorUsuarioId, DateTime fechaFinEfectiva,EstadoContrato estado)
    {
        const string sql = @"UPDATE Contratos SET
        Estado = @Estado,
        FechaFinEfectiva = @FechaFinEfectiva,
        FinalizadoPorUsuarioId = @FinalizadoPorUsuarioId
        WHERE Id = @Id;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new("@Estado", SqlDbType.NVarChar, 50) { Value = EstadoContrato.Finalizado });
        cmd.Parameters.Add(new("@FechaFinEfectiva", SqlDbType.Date) { Value = fechaFinEfectiva });
        cmd.Parameters.Add(new("@FinalizadoPorUsuarioId", SqlDbType.Int) { Value = finalizadoPorUsuarioId });
        cmd.Parameters.Add(new("@Id", SqlDbType.Int) { Value = contratoId });

        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }
}