using BookstoreAPI.Data;
using BookstoreAPI.DTOs;
using BookstoreAPI.Models;
using Dapper;

namespace BookstoreAPI.Repositories
{
    public class ComprobanteProveedorRepository : IComprobanteProveedorRepository
    {
        private readonly DapperContext _context;

        public ComprobanteProveedorRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ComprobanteProveedorListDto>> GetAllAsync()
        {
            const string query = @"
                SELECT
                    cp.Id,
                    cp.Proveedor_Id,
                    p.Nombre AS ProveedorNombre,
                    cp.TipoComprobante,
                    cp.FechaEmision,
                    cp.NroComprobante,
                    cp.ImporteTotal,
                    cp.CantidadCuotas,
                    cp.FechaPrimerVencimiento
                FROM comprobantes_proveedores cp
                INNER JOIN proveedores p ON cp.Proveedor_Id = p.Id
                ORDER BY cp.FechaEmision DESC, cp.Id DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<ComprobanteProveedorListDto>(query);
        }

        public async Task<ComprobanteProveedorListDto?> GetByIdAsync(int id)
        {
            const string query = @"
                SELECT
                    cp.Id,
                    cp.Proveedor_Id,
                    p.Nombre AS ProveedorNombre,
                    cp.TipoComprobante,
                    cp.FechaEmision,
                    cp.NroComprobante,
                    cp.ImporteTotal,
                    cp.CantidadCuotas,
                    cp.FechaPrimerVencimiento
                FROM comprobantes_proveedores cp
                INNER JOIN proveedores p ON cp.Proveedor_Id = p.Id
                WHERE cp.Id = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<ComprobanteProveedorListDto>(query, new { Id = id });
        }

        public async Task<ComprobanteProveedorDetailDto?> GetDetailByIdAsync(int id)
        {
            const string query = @"
                SELECT
                    cp.Id,
                    cp.Proveedor_Id,
                    p.Nombre AS ProveedorNombre,
                    cp.TipoComprobante,
                    cp.FechaEmision,
                    cp.NroComprobante,
                    cp.ImporteTotal,
                    cp.CantidadCuotas,
                    cp.FechaPrimerVencimiento
                FROM comprobantes_proveedores cp
                INNER JOIN proveedores p ON cp.Proveedor_Id = p.Id
                WHERE cp.Id = @Id";

            const string cuotasQuery = @"
                SELECT
                    Id,
                    NumeroCuota,
                    FechaVencimiento,
                    Importe,
                    ImportePagado,
                    Estado
                FROM cuotas_proveedores
                WHERE ComprobanteProveedor_Id = @Id
                ORDER BY NumeroCuota";

            using var connection = _context.CreateConnection();
            var comprobante = await connection.QueryFirstOrDefaultAsync<ComprobanteProveedorDetailDto>(query, new { Id = id });

            if (comprobante != null)
            {
                var cuotas = await connection.QueryAsync<CuotaProveedorDto>(cuotasQuery, new { Id = id });
                comprobante.Cuotas = cuotas.ToList();
            }

            return comprobante;
        }

        public async Task<ComprobanteProveedor> CreateAsync(ComprobanteProveedor comprobante, List<CuotaProveedor> cuotas)
        {
            const string comprobanteQuery = @"
                INSERT INTO comprobantes_proveedores
                (Proveedor_Id, TipoComprobante, FechaEmision, NroComprobante, ImporteTotal, CantidadCuotas, FechaPrimerVencimiento)
                VALUES
                (@Proveedor_Id, @TipoComprobante, @FechaEmision, @NroComprobante, @ImporteTotal, @CantidadCuotas, @FechaPrimerVencimiento);
                SELECT LAST_INSERT_ID();";

            const string cuotaQuery = @"
                INSERT INTO cuotas_proveedores
                (ComprobanteProveedor_Id, NumeroCuota, FechaVencimiento, Importe, ImportePagado, Estado)
                VALUES
                (@ComprobanteProveedor_Id, @NumeroCuota, @FechaVencimiento, @Importe, @ImportePagado, @Estado)";

            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var id = await connection.ExecuteScalarAsync<int>(comprobanteQuery, comprobante, transaction);
                comprobante.Id = id;

                foreach (var cuota in cuotas)
                {
                    cuota.ComprobanteProveedor_Id = id;
                    await connection.ExecuteAsync(cuotaQuery, cuota, transaction);
                }

                transaction.Commit();
                return comprobante;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string deleteCuotasQuery = "DELETE FROM cuotas_proveedores WHERE ComprobanteProveedor_Id = @Id";
            const string deleteComprobanteQuery = "DELETE FROM comprobantes_proveedores WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync(deleteCuotasQuery, new { Id = id }, transaction);
                var affectedRows = await connection.ExecuteAsync(deleteComprobanteQuery, new { Id = id }, transaction);

                transaction.Commit();
                return affectedRows > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public async Task<IEnumerable<IvaComprasDto>> GetIvaComprasAsync(DateTime fechaDesde, DateTime fechaHasta)
        {
            const string query = @"
                SELECT
                    cp.FechaEmision AS Fecha,
                    cp.TipoComprobante,
                    cp.NroComprobante AS NumeroComprobante,
                    p.Nombre,
                    p.Cuit,
                    cp.ImporteTotal AS Total
                FROM comprobantes_proveedores cp
                INNER JOIN proveedores p ON cp.Proveedor_Id = p.Id
                WHERE cp.FechaEmision >= @FechaDesde AND cp.FechaEmision <= @FechaHasta
                ORDER BY cp.FechaEmision, cp.NroComprobante";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<IvaComprasDto>(query, new { FechaDesde = fechaDesde, FechaHasta = fechaHasta });
        }
    }
}
