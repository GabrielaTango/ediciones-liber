using BookstoreAPI.Data;
using BookstoreAPI.DTOs;
using BookstoreAPI.Models;
using Dapper;
using System.Data;

namespace BookstoreAPI.Repositories
{
    public class CuotaRepository : ICuotaRepository
    {
        private readonly DapperContext _context;

        public CuotaRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Cuota>> GetByComprobanteIdAsync(int comprobanteId)
        {
            const string query = @"
                SELECT
                    id AS Id,
                    comprobante_id AS Comprobante_Id,
                    numero_cuota AS NumeroCuota,
                    fecha AS Fecha,
                    importe AS Importe,
                    importe_pagado AS ImportePagado,
                    estado AS Estado
                FROM cuotas
                WHERE comprobante_id = @ComprobanteId
                ORDER BY numero_cuota";

            using var connection = _context.CreateConnection();
            var cuotas = await connection.QueryAsync<Cuota>(query, new { ComprobanteId = comprobanteId });
            return cuotas;
        }

        public async Task CreateCuotasAsync(int comprobanteId, List<Cuota> cuotas, IDbConnection connection, IDbTransaction transaction)
        {
            const string query = @"
                INSERT INTO cuotas
                (comprobante_id, numero_cuota, fecha, importe, estado)
                VALUES
                (@Comprobante_Id, @NumeroCuota, @Fecha, @Importe, @Estado)";

            foreach (var cuota in cuotas)
            {
                cuota.Comprobante_Id = comprobanteId;
                await connection.ExecuteAsync(query, cuota, transaction);
            }
        }

        public async Task DeleteByComprobanteIdAsync(int comprobanteId, IDbConnection connection, IDbTransaction transaction)
        {
            const string deletePagos = "DELETE FROM pagos_cuotas WHERE Cuota_Id IN (SELECT Id FROM cuotas WHERE comprobante_id = @ComprobanteId)";
            const string deleteCuotas = "DELETE FROM cuotas WHERE comprobante_id = @ComprobanteId";
            await connection.ExecuteAsync(deletePagos, new { ComprobanteId = comprobanteId }, transaction);
            await connection.ExecuteAsync(deleteCuotas, new { ComprobanteId = comprobanteId }, transaction);
        }

        public async Task DeleteByComprobanteIdAsync(int comprobanteId)
        {
            const string deletePagos = "DELETE FROM pagos_cuotas WHERE Cuota_Id IN (SELECT Id FROM cuotas WHERE comprobante_id = @ComprobanteId)";
            const string deleteCuotas = "DELETE FROM cuotas WHERE comprobante_id = @ComprobanteId";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(deletePagos, new { ComprobanteId = comprobanteId });
            await connection.ExecuteAsync(deleteCuotas, new { ComprobanteId = comprobanteId });
        }

        public async Task CancelarByComprobanteIdAsync(int comprobanteId)
        {
            const string query = "UPDATE cuotas SET Estado = 'CAN' WHERE comprobante_id = @ComprobanteId";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new { ComprobanteId = comprobanteId });
        }

        public async Task<IEnumerable<CuotaListadoDto>> GetCuotasByFiltrosAsync(int? zonaId, DateTime? fechaCorte, int? vendedorId = null, string? comprobante = null, int? clienteId = null)
        {
            var query = @"
                SELECT
                    cu.id AS Id,
                    cu.comprobante_id AS ComprobanteId,
                    c.numeroComprobante AS NumeroComprobante,
                    c.fecha AS FechaComprobante,
                    cl.Id AS ClienteId,
                    cl.Nombre AS ClienteNombre,
                    z.id AS ZonaId,
                    z.descripcion AS ZonaNombre,
                    cu.fecha AS FechaCuota,
                    COALESCE(cu.importe, 0) AS Importe,
                    COALESCE(cu.importe_pagado, 0) AS ImportePagado,
                    cu.estado AS Estado,
                    cu.numero_cuota AS NumeroCuota
                FROM cuotas cu
                INNER JOIN comprobantes c ON cu.comprobante_id = c.id
                INNER JOIN clientes cl ON c.cliente_id = cl.Id
                LEFT JOIN zonas z ON cl.Zona_Id = z.id
                WHERE cu.estado != 'CAN'";

            if (zonaId.HasValue)
            {
                query += " AND z.id = @ZonaId";
            }

            if (fechaCorte.HasValue)
            {
                query += " AND cu.fecha <= @FechaCorte";
            }

            if (vendedorId.HasValue)
            {
                query += " AND c.vendedor_id = @VendedorId";
            }

            if (!string.IsNullOrEmpty(comprobante))
            {
                query += " AND c.numeroComprobante LIKE @Comprobante";
            }

            if (clienteId.HasValue)
            {
                query += " AND cl.Id = @ClienteId";
            }

            query += " ORDER BY CASE c.tipoComprobante WHEN 'PRE' THEN 0 ELSE 1 END, c.numeroComprobante ASC, cu.numero_cuota, cu.id";

            using var connection = _context.CreateConnection();
            var cuotas = (await connection.QueryAsync<CuotaListadoDto>(query, new { ZonaId = zonaId, FechaCorte = fechaCorte, VendedorId = vendedorId, Comprobante = $"%{comprobante}%", ClienteId = clienteId })).ToList();

            if (cuotas.Any())
            {
                var cuotaIds = cuotas.Select(c => c.Id).ToList();
                const string pagosQuery = @"
                    SELECT
                        Id,
                        Cuota_Id,
                        NroReferencia,
                        Fecha,
                        Importe
                    FROM pagos_cuotas
                    WHERE Cuota_Id IN @CuotaIds
                    ORDER BY Fecha, Id";

                var pagos = await connection.QueryAsync<dynamic>(pagosQuery, new { CuotaIds = cuotaIds });

                var pagosDict = pagos.GroupBy(p => (int)p.Cuota_Id)
                    .ToDictionary(g => g.Key, g => g.Select(p => new PagoCuotaDto
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

        public async Task<bool> UpdateImportePagadoAsync(int cuotaId, decimal importePagado)
        {
            const string updateCuotaQuery = @"
                UPDATE cuotas
                SET importe_pagado = @ImportePagado,
                    estado = CASE WHEN @ImportePagado >= importe THEN 'PAG' ELSE 'PEN' END
                WHERE id = @CuotaId";

            // Si todas las cuotas del comprobante están pagadas, actualizar estado a PAG
            const string autoPayQuery = @"
                UPDATE comprobantes
                SET estado = 'PAG'
                WHERE id = (SELECT comprobante_id FROM cuotas WHERE id = @CuotaId)
                  AND NOT EXISTS (
                      SELECT 1 FROM cuotas
                      WHERE comprobante_id = (SELECT comprobante_id FROM cuotas WHERE id = @CuotaId)
                      AND estado != 'PAG'
                  )";

            // Si alguna cuota vuelve a PEN, revertir el comprobante a PEN
            const string revertQuery = @"
                UPDATE comprobantes
                SET estado = 'PEN'
                WHERE id = (SELECT comprobante_id FROM cuotas WHERE id = @CuotaId)
                  AND estado = 'PAG'
                  AND EXISTS (
                      SELECT 1 FROM cuotas
                      WHERE comprobante_id = (SELECT comprobante_id FROM cuotas WHERE id = @CuotaId)
                      AND estado != 'PAG'
                  )";

            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var rowsAffected = await connection.ExecuteAsync(updateCuotaQuery, new { CuotaId = cuotaId, ImportePagado = importePagado }, transaction);
                await connection.ExecuteAsync(autoPayQuery, new { CuotaId = cuotaId }, transaction);
                await connection.ExecuteAsync(revertQuery, new { CuotaId = cuotaId }, transaction);
                transaction.Commit();
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task DeletePendientesByComprobanteIdAsync(int comprobanteId)
        {
            const string deletePagos = "DELETE FROM pagos_cuotas WHERE Cuota_Id IN (SELECT Id FROM cuotas WHERE comprobante_id = @ComprobanteId AND estado != 'PAG')";
            const string deleteCuotas = "DELETE FROM cuotas WHERE comprobante_id = @ComprobanteId AND estado != 'PAG'";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(deletePagos, new { ComprobanteId = comprobanteId });
            await connection.ExecuteAsync(deleteCuotas, new { ComprobanteId = comprobanteId });
        }

        public async Task<PagoCuota> CreatePagoAsync(int cuotaId, PagoCuota pago)
        {
            const string insertQuery = @"
                INSERT INTO pagos_cuotas
                (Cuota_Id, NroReferencia, Fecha, Importe)
                VALUES
                (@Cuota_Id, @NroReferencia, @Fecha, @Importe);
                SELECT LAST_INSERT_ID();";

            const string updateCuotaQuery = @"
                UPDATE cuotas
                SET importe_pagado = (
                        SELECT COALESCE(SUM(Importe), 0)
                        FROM pagos_cuotas
                        WHERE Cuota_Id = @CuotaId
                    ),
                    estado = CASE
                        WHEN (SELECT COALESCE(SUM(Importe), 0) FROM pagos_cuotas WHERE Cuota_Id = @CuotaId) >= importe THEN 'PAG'
                        WHEN (SELECT COALESCE(SUM(Importe), 0) FROM pagos_cuotas WHERE Cuota_Id = @CuotaId) > 0 THEN 'PAR'
                        ELSE 'PEN'
                    END
                WHERE Id = @CuotaId";

            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                pago.Cuota_Id = cuotaId;
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
            const string getCuotaIdQuery = "SELECT Cuota_Id FROM pagos_cuotas WHERE Id = @PagoId";

            const string deleteQuery = "DELETE FROM pagos_cuotas WHERE Id = @PagoId";

            const string updateCuotaQuery = @"
                UPDATE cuotas
                SET importe_pagado = (
                        SELECT COALESCE(SUM(Importe), 0)
                        FROM pagos_cuotas
                        WHERE Cuota_Id = @CuotaId
                    ),
                    estado = CASE
                        WHEN (SELECT COALESCE(SUM(Importe), 0) FROM pagos_cuotas WHERE Cuota_Id = @CuotaId) >= importe THEN 'PAG'
                        WHEN (SELECT COALESCE(SUM(Importe), 0) FROM pagos_cuotas WHERE Cuota_Id = @CuotaId) > 0 THEN 'PAR'
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

        public async Task CreatePagoComprobanteAsync(int comprobanteId, string nroReferencia, decimal importe, DateTime? fecha = null)
        {
            const string getCuotasQuery = @"
                SELECT
                    Id,
                    comprobante_id AS Comprobante_Id,
                    numero_cuota AS NumeroCuota,
                    fecha AS Fecha,
                    COALESCE(importe, 0) AS Importe,
                    COALESCE(importe_pagado, 0) AS ImportePagado,
                    estado AS Estado
                FROM cuotas
                WHERE comprobante_id = @ComprobanteId
                  AND estado != 'PAG'
                ORDER BY fecha, numero_cuota";

            const string insertPagoQuery = @"
                INSERT INTO pagos_cuotas (Cuota_Id, NroReferencia, Fecha, Importe)
                VALUES (@CuotaId, @NroReferencia, @Fecha, @Importe);";

            const string updateCuotaQuery = @"
                UPDATE cuotas
                SET importe_pagado = (
                        SELECT COALESCE(SUM(Importe), 0)
                        FROM pagos_cuotas
                        WHERE Cuota_Id = @CuotaId
                    ),
                    estado = CASE
                        WHEN (SELECT COALESCE(SUM(Importe), 0) FROM pagos_cuotas WHERE Cuota_Id = @CuotaId) >= importe THEN 'PAG'
                        WHEN (SELECT COALESCE(SUM(Importe), 0) FROM pagos_cuotas WHERE Cuota_Id = @CuotaId) > 0 THEN 'PAR'
                        ELSE 'PEN'
                    END
                WHERE Id = @CuotaId";

            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var cuotas = (await connection.QueryAsync<Cuota>(getCuotasQuery, new { ComprobanteId = comprobanteId }, transaction)).ToList();

                var restante = importe;
                var hoy = fecha ?? DateTime.Today;

                foreach (var cuota in cuotas)
                {
                    if (restante <= 0) break;

                    var saldoCuota = (cuota.Importe ?? 0) - (cuota.ImportePagado ?? 0);
                    if (saldoCuota <= 0) continue;

                    var montoAplicar = Math.Min(restante, saldoCuota);

                    await connection.ExecuteAsync(insertPagoQuery, new
                    {
                        CuotaId = cuota.Id,
                        NroReferencia = nroReferencia,
                        Fecha = hoy,
                        Importe = montoAplicar
                    }, transaction);

                    await connection.ExecuteAsync(updateCuotaQuery, new { CuotaId = cuota.Id }, transaction);

                    restante -= montoAplicar;
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

    }
}
