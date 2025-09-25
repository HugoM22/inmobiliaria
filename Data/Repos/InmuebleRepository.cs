
using System.Data;
using Inmobiliaria1.Models;
using Microsoft.Data.SqlClient;

namespace Inmobiliaria1.Data.Repos;

public class InmuebleRepository : IInmuebleRepository
{
    private readonly string _cs;
    public InmuebleRepository(IConfiguration config) =>
        _cs = config.GetConnectionString("DefaultConnection")!;
    public async Task<int> AltaAsync(Inmueble i)
    {
        const string sql = @"
        INSERT INTO Inmuebles
        (Direccion, Uso, TipoInmuebleId,Ambientes,Precio, Estado, Latitud, Longitud,PropietarioId)
        VALUES (@Direccion, @Uso, @TipoInmuebleId,@Ambientes,@Precio, @Estado, @Latitud, @Longitud,@PropietarioId);
        SELECT CAST (SCOPE_IDENTITY() AS int);";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new("@Direccion", SqlDbType.NVarChar, 200) { Value = i.Direccion });
        cmd.Parameters.Add(new("@Uso", SqlDbType.NVarChar, 50) { Value = i.Uso.ToString() });
        cmd.Parameters.Add(new("@TipoInmuebleId", SqlDbType.Int) { Value = i.TipoInmuebleId });
        cmd.Parameters.Add(new("@Ambientes", SqlDbType.Int) { Value = i.Ambientes });
        cmd.Parameters.Add(new("@Precio", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = i.Precio });
        cmd.Parameters.Add(new("@Estado", SqlDbType.NVarChar, 50) { Value = i.Estado.ToString() });
        cmd.Parameters.Add(new("@Latitud", SqlDbType.Float) { Precision = 9, Scale = 6, Value = (object?)i.Latitud ?? DBNull.Value });
        cmd.Parameters.Add(new("@Longitud", SqlDbType.Float) { Precision = 9, Scale = 6, Value = (object?)i.Longitud ?? DBNull.Value });
        cmd.Parameters.Add(new("@PropietarioId", SqlDbType.Int) { Value = i.PropietarioId });
        await cn.OpenAsync();
        var scalar = await cmd.ExecuteScalarAsync();
        if (scalar is null || scalar is DBNull) throw new InvalidCastException("No se pudo obtener el Id.");
        i.Id = Convert.ToInt32(scalar);
        return i.Id;
    }
    public async Task<int> ModificarAsync(Inmueble i)
    {
        const string sql = @"
        UPDATE Inmuebles
        SET Direccion = @Direccion,
            Uso = @Uso,
            TipoInmuebleId = @TipoInmuebleId,
            Ambientes = @Ambientes,
            Precio = @Precio,
            Estado = @Estado,
            Latitud = @Latitud,
            Longitud = @Longitud,
            PropietarioId = @PropietarioId
        WHERE Id = @Id;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new("@Id", SqlDbType.Int) { Value = i.Id });
        cmd.Parameters.Add(new("@Direccion", SqlDbType.NVarChar, 200) { Value = i.Direccion });
        cmd.Parameters.Add(new("@Uso", SqlDbType.NVarChar, 50) { Value = i.Uso.ToString() });
        cmd.Parameters.Add(new("@TipoInmuebleId", SqlDbType.Int) { Value = i.TipoInmuebleId });
        cmd.Parameters.Add(new("@Ambientes", SqlDbType.Int) { Value = i.Ambientes });
        cmd.Parameters.Add(new("@Precio", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = i.Precio });
        cmd.Parameters.Add(new("@Estado", SqlDbType.NVarChar, 50) { Value = i.Estado.ToString() });
        cmd.Parameters.Add(new("@Latitud", SqlDbType.Float) { Precision = 9, Scale = 6, Value = (object?)i.Latitud ?? DBNull.Value });
        cmd.Parameters.Add(new("@Longitud", SqlDbType.Float) { Precision = 9, Scale = 6, Value = (object?)i.Longitud ?? DBNull.Value });
        cmd.Parameters.Add(new("@PropietarioId", SqlDbType.Int) { Value = i.PropietarioId });
        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }
    public async Task<int> BajaAsync(int id)
    {
        const string sql = "DELETE FROM Inmuebles WHERE Id = @Id;";
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new("@Id", SqlDbType.Int) { Value = id });
        await cn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }
    public async Task<Inmueble?> ObtenerPorIdAsync(int id)
{
    const string sql = @"
    SELECT 
      i.Id,          
      i.Direccion,     
      i.Uso,        
      i.TipoInmuebleId,
      i.Ambientes,     
      i.Precio,        
      i.Estado,       
      i.Latitud,       
      i.Longitud,      
      i.PropietarioId  
    FROM Inmuebles i
    JOIN TipoInmuebles t ON t.Id = i.TipoInmuebleId
    JOIN Propietarios   p ON p.Id = i.PropietarioId
    WHERE i.Id = @Id;";

    using var cn = new SqlConnection(_cs);
    using var cmd = new SqlCommand(sql, cn);
    cmd.Parameters.Add(new("@Id", SqlDbType.Int) { Value = id });

    await cn.OpenAsync();
    using var dr = await cmd.ExecuteReaderAsync();
    if (!await dr.ReadAsync()) return null;

    return new Inmueble
    {
        Id             = dr.GetInt32(0),
        Direccion      = dr.GetString(1),
        Uso            = Enum.Parse<Uso>(dr.GetString(2), true),
        TipoInmuebleId = dr.GetInt32(3),
        Ambientes      = dr.GetInt32(4),
        Precio         = dr.GetDecimal(5),
        Estado         = Enum.Parse<EstadoInmueble>(dr.GetString(6), true),
        Latitud        = dr.IsDBNull(7) ? null : dr.GetDecimal(7),
        Longitud       = dr.IsDBNull(8) ? null : dr.GetDecimal(8),
        PropietarioId  = dr.GetInt32(9)
    };
}
    public async Task<List<Inmueble>> ObtenerTodosAsync(string? q = null, int? propietarioId = null, int? tipoId = null)
    {
        var sql = @"
        SELECT i.Id, i.Direccion, i.Uso, i.TipoInmuebleId, i.Ambientes, i.Precio, i.Estado,
            i.PropietarioId, t.Descripcion AS TipoNombre
        FROM Inmuebles i
        JOIN TipoInmuebles t ON t.Id = i.TipoInmuebleId
        WHERE 1=1";
        if (!string.IsNullOrWhiteSpace(q)) sql += " AND (i.Direccion LIKE @q OR i.Uso LIKE @q)";
        if (propietarioId.HasValue) sql += " AND i.PropietarioId=@PropietarioId";
        if (tipoId.HasValue)       sql += " AND i.TipoInmuebleId=@TipoId";
        sql += " ORDER BY i.Direccion;";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(sql, cn);
        if (!string.IsNullOrWhiteSpace(q)) cmd.Parameters.Add(new("@q", SqlDbType.NVarChar, 120){ Value = $"%{q}%" });
        if (propietarioId.HasValue) cmd.Parameters.Add(new("@PropietarioId", SqlDbType.Int){ Value = propietarioId.Value });
        if (tipoId.HasValue)        cmd.Parameters.Add(new("@TipoId", SqlDbType.Int){ Value = tipoId.Value });

        await cn.OpenAsync();
        using var rd = await cmd.ExecuteReaderAsync();
        var list = new List<Inmueble>();
        while (await rd.ReadAsync())
        {
            list.Add(new Inmueble {
                Id = rd.GetInt32(0),
                Direccion = rd.GetString(1),
                Uso = Enum.Parse<Uso>(rd.GetString(2)),
                TipoInmuebleId = rd.GetInt32(3),
                Ambientes = rd.GetInt32(4),
                Precio = rd.GetDecimal(5),
                Estado = Enum.Parse<EstadoInmueble>(rd.GetString(6)),
                PropietarioId = rd.GetInt32(7)
            });
        }
        return list;
    }
}