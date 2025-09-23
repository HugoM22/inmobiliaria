using System.Data;
using Inmobiliaria1.Models;
using Microsoft.Data.SqlClient;

namespace Inmobiliaria1.Data.Repos;

public class PagoRepository : IPagoRepository
{
    private readonly string _cs;
    public PagoRepository(IConfiguration configuration)
    {
        _cs = configuration.GetConnectionString("DefaultConnection")!;
    }
    public async Task<int> AltaAsync(Pago p)
    {
        const string sql = @"
        INSERT INTO Pagos (ContratoId, Numero, Fecha, Importe, Detalle, Estado, Multa, CreadoPorUsuarioId, AnuladoPorUsuarioId)
        VALUES (@ContratoId, @Numero, @Fecha, @Importe, @Detalle, @Estado, @Multa, @CreadoPorUsuarioId, @AnuladoPorUsuarioId);
        SELECT CAST(SCOPE_IDENTITY() AS int);";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new("@ContratoId", SqlDbType.Int) { Value = p.ContratoId });
        cmd.Parameters.Add(new("@Numero", SqlDbType.Int) { Value = p.Numero });
        cmd.Parameters.Add(new("@Fecha", SqlDbType.Date) { Value = p.Fecha });
        cmd.Parameters.Add(new("@Importe", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = p.Importe });
        cmd.Parameters.Add(new("@Detalle", SqlDbType.NVarChar, 200) { Value = (object?)p.Detalle ?? DBNull.Value });
        cmd.Parameters.Add(new("@Estado", SqlDbType.NVarChar, 20) { Value = p.Estado });
        cmd.Parameters.Add(new("@Multa", SqlDbType.Bit) { Value = p.Multa });
        cmd.Parameters.Add(new("@CreadoPorUsuarioId", SqlDbType.Int) { Value = p.CreadoPorUsuarioId });
        cmd.Parameters.Add(new("@AnuladoPorUsuarioId", SqlDbType.Int) { Value = (object?)p.AnuladoPorUsuarioId ?? DBNull.Value });

        await cn.OpenAsync();
        try
        {
            var scalar = await cmd.ExecuteScalarAsync();
            if (scalar is null || scalar is DBNull) throw new InvalidOperationException("No se pudo obtener el Id.");
            p.Id = Convert.ToInt32(scalar);
            return p.Id;
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            throw new InvalidOperationException("El número de pago ya existe para este contrato.", ex);
        }
    }
    public async Task<int> ModificarAsync(Pago P)
    {
        const string sql = @"
            UPDATE Pagos SET
            ContratoId=@ContratoId, Numero=@Numero, Fecha=@Fecha, Importe=@Importe, Detalle=@Detalle,
            Estado=@Estado, Multa=@Multa, CreadoPorUsuarioId=@CreadoPorUsuarioId, AnuladoPorUsuarioId=@AnuladoPorUsuarioId
            WHERE Id=@Id;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new("@Id", SqlDbType.Int) { Value = P.Id });
        cmd.Parameters.Add(new("@ContratoId", SqlDbType.Int) { Value = P.ContratoId });
        cmd.Parameters.Add(new("@Numero", SqlDbType.Int) { Value = P.Numero });
        cmd.Parameters.Add(new("@Fecha", SqlDbType.Date) { Value = P.Fecha });
        cmd.Parameters.Add(new("@Importe", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = P.Importe });
        cmd.Parameters.Add(new("@Detalle", SqlDbType.NVarChar, 200) { Value = (object?)P.Detalle ?? DBNull.Value });
        cmd.Parameters.Add(new("@Estado", SqlDbType.NVarChar, 20) { Value = P.Estado });
        cmd.Parameters.Add(new("@Multa", SqlDbType.Bit) { Value = P.Multa });
        cmd.Parameters.Add(new("@CreadoPorUsuarioId", SqlDbType.Int) { Value = P.CreadoPorUsuarioId });
        cmd.Parameters.Add(new("@AnuladoPorUsuarioId", SqlDbType.Int) { Value = (object?)P.AnuladoPorUsuarioId ?? DBNull.Value });

        await cn.OpenAsync();
        try
        {
            return await cmd.ExecuteNonQueryAsync();
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            throw new InvalidOperationException("El número de pago ya existe para este contrato.", ex);
        }
    }
    public async Task<int> BorrarAsync(int id)
    {
        const string sql = "DELETE FROM Pagos WHERE Id = @Id;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new("@Id", SqlDbType.Int) { Value = id });

        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }
    public async Task<Pago?> ObtenerPorIdAsync(int id)
    {
        const string sql = @"
            SELECT p.Id, p.ContratoId, p.Numero, p.Fecha, p.Importe, p.Detalle, p.Estado, p.Multa,
                p.CreadoPorUsuarioId, p.AnuladoPorUsuarioId,
                c.InmuebleId, c.InquilinoId,
                u1.Email AS CreadoPorEmail,
                u2.Email AS AnuladoPorEmail
            FROM Pagos p
            JOIN Contratos c ON c.Id = p.ContratoId
            LEFT JOIN Usuario u1 ON u1.Id = p.CreadoPorUsuarioId
            LEFT JOIN Usuario u2 ON u2.Id = p.AnuladoPorUsuarioId
            WHERE p.Id=@Id;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new("@Id", SqlDbType.Int) { Value = id });
        await cn.OpenAsync();

        using var dr = await cmd.ExecuteReaderAsync();
        if (!await dr.ReadAsync()) return null;

        return new Pago
        {
            Id = dr.GetInt32(0),
            ContratoId = dr.GetInt32(1),
            Numero = dr.GetInt32(2),
            Fecha = dr.GetDateTime(3),
            Importe = dr.GetDecimal(4),
            Detalle = dr.IsDBNull(5) ? null : dr.GetString(5),
            Estado = Enum.Parse<EstadoPago>(dr.GetString(6)),
            Multa = dr.GetBoolean(7),
            CreadoPorUsuarioId = dr.GetInt32(8),
            AnuladoPorUsuarioId = dr.IsDBNull(9) ? null : dr.GetInt32(9),
            Contrato = new Contrato { Id = dr.GetInt32(1), InmuebleId = dr.GetInt32(10), InquilinoId = dr.GetInt32(11) },
            CreadoPor = dr.IsDBNull(12) ? null : new Usuario { Id = dr.GetInt32(8), Email = dr.GetString(12) },
            AnuladoPor = dr.IsDBNull(13) ? null : new Usuario { Id = dr.GetInt32(9), Email = dr.GetString(13) }
        };
    }
    public async Task<List<Pago>> ObtenerPorContratoAsync(int contratoId)
    {
        const string sql = @"
            SELECT Id, ContratoId, Numero, Fecha, Importe, Detalle, Estado, Multa,
                CreadoPorUsuarioId, AnuladoPorUsuarioId
            FROM Pagos
            WHERE ContratoId=@ContratoId
            ORDER BY Numero;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new("@ContratoId", SqlDbType.Int) { Value = contratoId });

        await cn.OpenAsync();
        using var dr = await cmd.ExecuteReaderAsync();
        var list = new List<Pago>();
        while (await dr.ReadAsync())
        {
            list.Add(new Pago
            {
                Id = dr.GetInt32(0),
                ContratoId = dr.GetInt32(1),
                Numero = dr.GetInt32(2),
                Fecha = dr.GetDateTime(3),
                Importe = dr.GetDecimal(4),
                Detalle = dr.IsDBNull(5) ? null : dr.GetString(5),
                Estado = Enum.Parse<EstadoPago>(dr.GetString(6)),
                Multa = dr.GetBoolean(7),
                CreadoPorUsuarioId = dr.GetInt32(8),
                AnuladoPorUsuarioId = dr.IsDBNull(9) ? null : dr.GetInt32(9)
            });
        }
        return list;
    }
    public async Task<decimal> TotalPagadoAsync(int contratoId)
    {
        const string sql = "SELECT ISNULL(SUM(Importe),0) FROM Pagos WHERE ContratoId=@ContratoId AND Estado='Activo';";
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new("@ContratoId", SqlDbType.Int) { Value = contratoId });
        await cn.OpenAsync();
        var scalar = await cmd.ExecuteScalarAsync();
        return Convert.ToDecimal(scalar);
    }
    public async Task<int> AnularAsync(int idPago, int anuladoPorUsuarioId)
    {
        const string sql = @"
            UPDATE Pagos SET Estado='Anulado', AnuladoPorUsuarioId=@UserId
            WHERE Id=@Id;";
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new("@UserId", SqlDbType.Int){ Value = anuladoPorUsuarioId });
        cmd.Parameters.Add(new("@Id", SqlDbType.Int){ Value = idPago });

        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }
}