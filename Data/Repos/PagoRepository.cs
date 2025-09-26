using System.Data;
using Inmobiliaria1.Models;
using Microsoft.Data.SqlClient;

namespace Inmobiliaria1.Data.Repos;

public class PagoRepository : IPagoRepository
{
    private readonly string _cs;
    public PagoRepository(IConfiguration cfg) => _cs = cfg.GetConnectionString("DefaultConnection")!;

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
        using var rd = await cmd.ExecuteReaderAsync();

        var list = new List<Pago>();
        while (await rd.ReadAsync())
        {
            list.Add(new Pago
            {
                Id = rd.GetInt32(0),
                ContratoId = rd.GetInt32(1),
                Numero = rd.GetInt32(2),
                Fecha = rd.GetDateTime(3),
                Importe = rd.GetDecimal(4),
                Detalle = rd.IsDBNull(5) ? null : rd.GetString(5),
                Estado = Enum.Parse<EstadoPago>(rd.GetString(6), true),
                Multa = rd.GetBoolean(7),
                CreadoPorUsuarioId = rd.GetInt32(8),
                AnuladoPorUsuarioId = rd.IsDBNull(9) ? null : rd.GetInt32(9)
            });
        }
        return list;
    }

    public async Task<Pago?> ObtenerPorIdAsync(int id)
    {
        const string sql = @"
            SELECT p.Id, p.ContratoId, p.Numero, p.Fecha, p.Importe, p.Detalle, p.Estado, p.Multa,
                   p.CreadoPorUsuarioId, p.AnuladoPorUsuarioId
            FROM Pagos p
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
            Estado = Enum.Parse<EstadoPago>(dr.GetString(6), true),
            Multa = dr.GetBoolean(7),
            CreadoPorUsuarioId = dr.GetInt32(8),
            AnuladoPorUsuarioId = dr.IsDBNull(9) ? null : dr.GetInt32(9)
        };
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



    public async Task<int> AltaAsync(Pago p)
    {
      
        const string sql = @"
    DECLARE @n INT;
    SELECT @n = ISNULL(MAX(Numero),0)+1
    FROM Pagos WITH (UPDLOCK, HOLDLOCK)
    WHERE ContratoId=@ContratoId;

    INSERT INTO Pagos (ContratoId, Numero, Fecha, Importe, Detalle, Estado, Multa, CreadoPorUsuarioId)
    VALUES (@ContratoId, @n, @Fecha, @Importe, @Detalle, @Estado, @Multa, @CreadoPorUsuarioId);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NewId, @n AS Numero;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);

        cmd.Parameters.Add(new("@ContratoId", SqlDbType.Int) { Value = p.ContratoId });
        cmd.Parameters.Add(new("@Fecha", SqlDbType.Date) { Value = p.Fecha });
        var pi = cmd.Parameters.Add("@Importe", SqlDbType.Decimal); pi.Precision = 18; pi.Scale = 2; pi.Value = p.Importe;
        cmd.Parameters.Add(new("@Detalle", SqlDbType.NVarChar, 200) { Value = (object?)p.Detalle ?? DBNull.Value });
        cmd.Parameters.Add(new("@Estado", SqlDbType.NVarChar, 20) { Value = p.Estado.ToString() });
        cmd.Parameters.Add(new("@Multa", SqlDbType.Bit) { Value = p.Multa });
        cmd.Parameters.Add(new("@CreadoPorUsuarioId", SqlDbType.Int) { Value = p.CreadoPorUsuarioId });

        await cn.OpenAsync();
        try
        {
            using var rd = await cmd.ExecuteReaderAsync();
            if (await rd.ReadAsync())
            {
                p.Id = rd.GetInt32(0);
                p.Numero = rd.GetInt32(1);
            }
            return p.Id;
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
          
            throw new InvalidOperationException("El número de pago ya existe para este contrato.", ex);
        }
    }

    public async Task<int> EditarDetalleAsync(int id, string? detalle)
    {
        const string sql = @"UPDATE Pagos SET Detalle=@Detalle WHERE Id=@Id AND Estado='Activo';";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new("@Id", SqlDbType.Int) { Value = id });
        cmd.Parameters.Add(new("@Detalle", SqlDbType.NVarChar, 200) { Value = (object?)detalle ?? DBNull.Value });

        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> AnularAsync(int idPago, int anuladoPorUsuarioId)
    {
        const string sql = @"
UPDATE Pagos
SET Estado='Anulado', AnuladoPorUsuarioId=@UserId
WHERE Id=@Id AND Estado='Activo';";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new("@UserId", SqlDbType.Int) { Value = anuladoPorUsuarioId });
        cmd.Parameters.Add(new("@Id", SqlDbType.Int) { Value = idPago });

        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
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
}
