using BookstoreAPI.Data;
using BookstoreAPI.DTOs;
using BookstoreAPI.Models;
using Dapper;

namespace BookstoreAPI.Repositories
{
    public class CuotaProveedorRepository : ICuotaProveedorRepository
    {
        private readonly DapperContext _context;

        public CuotaProveedorRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CuotaProveedorListadoDto>> GetCuotasByFiltrosAsync(int? proveedorId, DateTime? fechaDesde, DateTime? fechaHasta, string? estado)
        {
            var query = @"
                SELECT
                    cp.Id,
                    cp.ComprobanteProveedor_Id AS ComprobanteProveedorId,
                    c.NroComprobante,
                    c.TipoComprobante,
                    c.FechaEmision,
                    p.Id AS ProveedorId,
                    p.Nombre AS ProveedorNombre,
                    cp.NumeroCuota,
                    cp.FechaVencimiento,
                    cp.Importe,
                    cp.ImportePagado,
                    cp.Estado
                FROM cuotas_proveedores cp
                INNER JOIN comprobantes_proveedores c ON cp.ComprobanteProveedor_Id = c.Id
                INNER JOIN proveedores p ON c.Proveedor_Id = p.Id
                WHERE 1=1";

            var parameters = new DynamicParameters();

            if (proveedorId.HasValue)
            {
                query += " AND p.Id = @ProveedorId";
                parameters.Add("ProveedorId", proveedorId.Value);
            }

            if (fechaDesde.HasValue)
            {
                query += " AND cp.FechaVencimiento >= @FechaDesde";
                parameters.Add("FechaDesde", fechaDesde.Value.Date);
            }

            if (fechaHasta.HasValue)
            {
                query += " AND cp.FechaVencimiento <= @FechaHasta";
                parameters.Add("FechaHasta", fechaHasta.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query += " AND cp.Estado = @Estado";
                parameters.Add("Estado", estado);
            }

            query += " ORDER BY cp.FechaVencimiento, p.Nombre, cp.NumeroCuota";

            using var connection = _context.CreateConnection();
            var cuotas = (await connection.QueryAsync<CuotaProveedorListadoDto>(query, parameters)).ToList();

            if (cuotas.Any())
            {
                var cuotaIds = cuotas.Select(c => c.Id).ToList();
                const string pagosQuery = @"
                    SELECT
                        Id,
                        CuotaProveedor_Id,
                        NroReferencia,
                        Fecha,
                        Importe
                    FROM pagos_cuotas_proveedores
                    WHERE CuotaProveedor_Id IN @CuotaIds
                    ORDER BY Fecha, Id";

                var pagos = await connection.QueryAsync<dynamic>(pagosQuery, new { CuotaIds = cuotaIds });

                var pagosDict = pagos.GroupBy(p => (int)p.CuotaProveedor_Id)
                    .ToDictionary(g => g.Key, g => g.Select(p => new PagoCuotaProveedorDto
                    {
                        Id = (int)p.Id,
                        NroReferencia = (string)p.NroReferencia,
                        Fecha = (DateTime)p.Fecha,
                        Importe = (decimal)p.Importe
                    }).ToList());

                foreach (var cuota in cuotas)
                {
                    if (pagosDict.TryGetValue(cuota.Id, out var cuotaPagos))
                    {
                        cuota.Pagos = cuotaPagos;
                    }
                }
            }

            return cuotas;
        }

        public async Task<PagoCuotaProveedor> CreatePagoAsync(int cuotaId, PagoCuotaProveedor pago)
        {
            const string insertQuery = @"
                INSERT INTO pagos_cuotas_proveedores
                (CuotaProveedor_Id, NroReferencia, Fecha, Importe)
                VALUES
                (@CuotaProveedor_Id, @NroReferencia, @Fecha, @Importe);
                SELECT LAST_INSERT_ID();";

            const string updateCuotaQuery = @"
                UPDATE cuotas_proveedores
                SET ImportePagado = (
                        SELECT COALESCE(SUM(Importe), 0)
                        FROM pagos_cuotas_proveedores
                        WHERE CuotaProveedor_Id = @CuotaId
                    ),
                    Estado = CASE
                        WHEN (SELECT COALESCE(SUM(Importe), 0) FROM pagos_cuotas_proveedores WHERE CuotaProveedor_Id = @CuotaId) >= Importe THEN 'PAG'
                        WHEN (SELECT COALESCE(SUM(Importe), 0) FROM pagos_cuotas_proveedores WHERE CuotaProveedor_Id = @CuotaId) > 0 THEN 'PAR'
                        ELSE 'PEN'
                    END
                WHERE Id = @CuotaId";

            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                pago.CuotaProveedor_Id = cuotaId;
                var id = await connection.ExecuteScalarAsync<int>(insertQuery, pago, transaction);
                pago.Id = id;

                await connection.ExecuteAsync(updateCuotaQuery, new { CuotaId = cuotaId }, transaction);

                transaction.Commit();
                return pago;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeletePagoAsync(int pagoId)
        {
            const string getCuotaIdQuery = "SELECT CuotaProveedor_Id FROM pagos_cuotas_proveedores WHERE Id = @PagoId";

            const string deleteQuery = "DELETE FROM pagos_cuotas_proveedores WHERE Id = @PagoId";

            const string updateCuotaQuery = @"
                UPDATE cuotas_proveedores
                SET ImportePagado = (
                        SELECT COALESCE(SUM(Importe), 0)
                        FROM pagos_cuotas_proveedores
                        WHERE CuotaProveedor_Id = @CuotaId
                    ),
                    Estado = CASE
                        WHEN (SELECT COALESCE(SUM(Importe), 0) FROM pagos_cuotas_proveedores WHERE CuotaProveedor_Id = @CuotaId) >= Importe THEN 'PAG'
                        WHEN (SELECT COALESCE(SUM(Importe), 0) FROM pagos_cuotas_proveedores WHERE CuotaProveedor_Id = @CuotaId) > 0 THEN 'PAR'
                        ELSE 'PEN'
                    END
                WHERE Id = @CuotaId";

            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var cuotaId = await connection.QueryFirstOrDefaultAsync<int?>(getCuotaIdQuery, new { PagoId = pagoId }, transaction);
                if (!cuotaId.HasValue) return false;

                var rows = await connection.ExecuteAsync(deleteQuery, new { PagoId = pagoId }, transaction);

                await connection.ExecuteAsync(updateCuotaQuery, new { CuotaId = cuotaId.Value }, transaction);

                transaction.Commit();
                return rows > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
