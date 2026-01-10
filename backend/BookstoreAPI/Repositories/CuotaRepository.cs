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
            const string query = "DELETE FROM cuotas WHERE comprobante_id = @ComprobanteId";
            await connection.ExecuteAsync(query, new { ComprobanteId = comprobanteId }, transaction);
        }

        public async Task DeleteByComprobanteIdAsync(int comprobanteId)
        {
            const string query = "DELETE FROM cuotas WHERE comprobante_id = @ComprobanteId";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new { ComprobanteId = comprobanteId });
        }

        public async Task<IEnumerable<CuotaListadoDto>> GetCuotasByFiltrosAsync(int? zonaId, int? mes, int? anio)
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
                WHERE 1=1";

            if (zonaId.HasValue)
            {
                query += " AND z.id = @ZonaId";
            }

            if (mes.HasValue)
            {
                query += " AND MONTH(cu.fecha) = @Mes";
            }

            if (anio.HasValue)
            {
                query += " AND YEAR(cu.fecha) = @Anio";
            }

            query += " ORDER BY FechaCuota, ClienteNombre, cu.numero_cuota, cu.id";

            using var connection = _context.CreateConnection();
            var cuotas = await connection.QueryAsync<CuotaListadoDto>(query, new { ZonaId = zonaId, Mes = mes, Anio = anio });
            return cuotas;
        }

        public async Task<bool> UpdateImportePagadoAsync(int cuotaId, decimal importePagado)
        {
            const string query = @"
                UPDATE cuotas
                SET importe_pagado = @ImportePagado,
                    estado = CASE WHEN @ImportePagado >= importe THEN 'PAG' ELSE 'PEN' END
                WHERE id = @CuotaId";

            using var connection = _context.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(query, new { CuotaId = cuotaId, ImportePagado = importePagado });
            return rowsAffected > 0;
        }

    }
}
