using BookstoreAPI.Data;
using BookstoreAPI.DTOs;
using BookstoreAPI.Models;
using Dapper;
using System.Data;

namespace BookstoreAPI.Repositories
{
    public class ComprobanteRepository : IComprobanteRepository
    {
        private readonly DapperContext _context;
        private readonly ICuotaRepository _cuotaRepository;

        public ComprobanteRepository(DapperContext context, ICuotaRepository cuotaRepository)
        {
            _context = context;
            _cuotaRepository = cuotaRepository;
        }

        public async Task<IEnumerable<ComprobanteConDetallesDto>> GetAllAsync()
        {
            const string query = @"
                SELECT
                    c.id AS Id,
                    c.cliente_id AS Cliente_Id,
                    cl.Nombre AS ClienteNombre,
                    c.fecha AS Fecha,
                    c.tipoComprobante AS TipoComprobante,
                    c.numeroComprobante AS NumeroComprobante,
                    c.total AS Total,
                    c.CAE,
                    c.VTO,
                    c.Bonificacion,
                    c.PorcentajeBonif,
                    c.Anticipo,
                    c.ContraEntrega,
                    c.Cuotas,
                    c.ValorCuota,
                    c.vendedor_id AS Vendedor_Id,
                    v.descripcion AS VendedorNombre,
                    c.GastosEnvio,
                    c.EsElectronica,
                    c.EsPresupuesto,
                    c.estado AS Estado,
                    c.comprobante_asociado_id AS ComprobanteAsociado_Id,
                    ca.numeroComprobante AS ComprobanteAsociadoNumero,
                    CASE WHEN nc.id IS NOT NULL THEN 1 ELSE 0 END AS EstaCancelado,
                    nc.numeroComprobante AS NotaCreditoNumero
                FROM comprobantes c
                INNER JOIN clientes cl ON c.cliente_id = cl.Id
                LEFT JOIN vendedores v ON c.vendedor_id = v.id
                LEFT JOIN comprobantes ca ON c.comprobante_asociado_id = ca.id
                LEFT JOIN comprobantes nc ON nc.comprobante_asociado_id = c.id AND nc.tipoComprobante = 'NC'
                ORDER BY c.fecha DESC, c.id DESC";

            using var connection = _context.CreateConnection();
            var comprobantes = await connection.QueryAsync<ComprobanteConDetallesDto>(query);

            // Cargar detalles para cada comprobante
            foreach (var comprobante in comprobantes)
            {
                comprobante.Detalles = (await GetDetallesByComprobanteIdAsync(comprobante.Id, connection)).ToList();
            }

            return comprobantes;
        }

        public async Task<IEnumerable<ComprobanteConDetallesDto>> GetAllFilteredAsync(
            int? zonaId, int? clienteId, string? tipoComprobante, DateTime? fechaDesde, DateTime? fechaHasta, int? vendedorId = null, string? numeroComprobante = null)
        {
            var query = @"
                SELECT
                    c.id AS Id,
                    c.cliente_id AS Cliente_Id,
                    cl.Nombre AS ClienteNombre,
                    c.fecha AS Fecha,
                    c.tipoComprobante AS TipoComprobante,
                    c.numeroComprobante AS NumeroComprobante,
                    c.total AS Total,
                    c.CAE,
                    c.VTO,
                    c.Bonificacion,
                    c.PorcentajeBonif,
                    c.Anticipo,
                    c.ContraEntrega,
                    c.Cuotas,
                    c.ValorCuota,
                    c.vendedor_id AS Vendedor_Id,
                    v.descripcion AS VendedorNombre,
                    c.GastosEnvio,
                    c.EsElectronica,
                    c.EsPresupuesto,
                    c.estado AS Estado,
                    c.comprobante_asociado_id AS ComprobanteAsociado_Id,
                    ca.numeroComprobante AS ComprobanteAsociadoNumero,
                    CASE WHEN nc_check.id IS NOT NULL THEN 1 ELSE 0 END AS EstaCancelado,
                    nc_check.numeroComprobante AS NotaCreditoNumero
                FROM comprobantes c
                INNER JOIN clientes cl ON c.cliente_id = cl.Id
                LEFT JOIN vendedores v ON c.vendedor_id = v.id
                LEFT JOIN zonas z ON cl.Zona_Id = z.id
                LEFT JOIN comprobantes ca ON c.comprobante_asociado_id = ca.id
                LEFT JOIN comprobantes nc_check ON nc_check.comprobante_asociado_id = c.id AND nc_check.tipoComprobante = 'NC'
                WHERE 1=1";

            var parameters = new DynamicParameters();

            if (zonaId.HasValue)
            {
                query += " AND z.id = @ZonaId";
                parameters.Add("ZonaId", zonaId.Value);
            }

            if (clienteId.HasValue)
            {
                query += " AND c.cliente_id = @ClienteId";
                parameters.Add("ClienteId", clienteId.Value);
            }

            if (!string.IsNullOrWhiteSpace(tipoComprobante))
            {
                query += " AND c.tipoComprobante = @TipoComprobante";
                parameters.Add("TipoComprobante", tipoComprobante);
            }

            if (fechaDesde.HasValue)
            {
                query += " AND DATE(c.fecha) >= @FechaDesde";
                parameters.Add("FechaDesde", fechaDesde.Value.Date);
            }

            if (fechaHasta.HasValue)
            {
                query += " AND DATE(c.fecha) <= @FechaHasta";
                parameters.Add("FechaHasta", fechaHasta.Value.Date);
            }

            if (vendedorId.HasValue)
            {
                query += " AND c.vendedor_id = @VendedorId";
                parameters.Add("VendedorId", vendedorId.Value);
            }

            if (!string.IsNullOrWhiteSpace(numeroComprobante))
            {
                query += " AND c.numeroComprobante LIKE @NumeroComprobante";
                parameters.Add("NumeroComprobante", $"%{numeroComprobante}%");
            }

            query += " ORDER BY c.fecha DESC, c.id DESC";

            using var connection = _context.CreateConnection();
            var comprobantes = await connection.QueryAsync<ComprobanteConDetallesDto>(query, parameters);

            // Cargar detalles para cada comprobante
            foreach (var comprobante in comprobantes)
            {
                comprobante.Detalles = (await GetDetallesByComprobanteIdAsync(comprobante.Id, connection)).ToList();
            }

            return comprobantes;
        }

        public async Task<ComprobanteConDetallesDto?> GetByIdAsync(int id)
        {
            const string query = @"
                SELECT
                    c.id AS Id,
                    c.cliente_id AS Cliente_Id,
                    cl.Nombre AS ClienteNombre,
                    c.fecha AS Fecha,
                    c.tipoComprobante AS TipoComprobante,
                    c.numeroComprobante AS NumeroComprobante,
                    c.total AS Total,
                    c.CAE,
                    c.VTO,
                    c.Bonificacion,
                    c.PorcentajeBonif,
                    c.Anticipo,
                    c.ContraEntrega,
                    c.Cuotas,
                    c.ValorCuota,
                    c.vendedor_id AS Vendedor_Id,
                    v.descripcion AS VendedorNombre,
                    c.GastosEnvio,
                    c.EsElectronica,
                    c.EsPresupuesto,
                    c.estado AS Estado,
                    c.comprobante_asociado_id AS ComprobanteAsociado_Id,
                    ca.numeroComprobante AS ComprobanteAsociadoNumero,
                    CASE WHEN nc.id IS NOT NULL THEN 1 ELSE 0 END AS EstaCancelado,
                    nc.numeroComprobante AS NotaCreditoNumero
                FROM comprobantes c
                INNER JOIN clientes cl ON c.cliente_id = cl.Id
                LEFT JOIN vendedores v ON c.vendedor_id = v.id
                LEFT JOIN comprobantes ca ON c.comprobante_asociado_id = ca.id
                LEFT JOIN comprobantes nc ON nc.comprobante_asociado_id = c.id AND nc.tipoComprobante = 'NC'
                WHERE c.id = @Id";

            using var connection = _context.CreateConnection();
            var comprobante = await connection.QueryFirstOrDefaultAsync<ComprobanteConDetallesDto>(query, new { Id = id });

            if (comprobante != null)
            {
                comprobante.Detalles = (await GetDetallesByComprobanteIdAsync(id, connection)).ToList();
            }

            return comprobante;
        }

        public async Task<Comprobante?> GetComprobanteByIdAsync(int id)
        {
            const string query = @"
                SELECT
                    id AS Id,
                    cliente_id AS Cliente_Id,
                    fecha AS Fecha,
                    tipoComprobante AS TipoComprobante,
                    numeroComprobante AS NumeroComprobante,
                    total AS Total,
                    CAE,
                    VTO,
                    Bonificacion,
                    PorcentajeBonif,
                    Anticipo,
                    ContraEntrega,
                    Cuotas,
                    ValorCuota,
                    vendedor_id AS Vendedor_Id,
                    GastosEnvio,
                    EsElectronica,
                    EsPresupuesto,
                    estado AS Estado
                FROM comprobantes
                WHERE id = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Comprobante>(query, new { Id = id });
        }

        private async Task<IEnumerable<ComprobanteDetalleConArticuloDto>> GetDetallesByComprobanteIdAsync(int comprobanteId, IDbConnection connection)
        {
            const string query = @"
                SELECT
                    cd.id AS Id,
                    cd.articulo_id AS Articulo_Id,
                    a.Codigo AS ArticuloCodigo,
                    a.Descripcion AS ArticuloDescripcion,
                    cd.cantidad AS Cantidad,
                    cd.precio_unitario AS Precio_Unitario,
                    cd.subtotal AS Subtotal
                FROM comprobante_detalle cd
                INNER JOIN articulos a ON cd.articulo_id = a.Id
                WHERE cd.factura_id = @ComprobanteId
                ORDER BY cd.id";

            return await connection.QueryAsync<ComprobanteDetalleConArticuloDto>(query, new { ComprobanteId = comprobanteId });
        }

        public async Task<Comprobante> CreateAsync(Comprobante comprobante, List<ComprobanteDetalle> detalles)
        {
            const string comprobanteQuery = @"
                INSERT INTO comprobantes
                (cliente_id, fecha, tipoComprobante, numeroComprobante, total, CAE, VTO,
                 Bonificacion, PorcentajeBonif, Anticipo, ContraEntrega, Cuotas, ValorCuota, vendedor_id, GastosEnvio,
                 EsElectronica, EsPresupuesto, comprobante_asociado_id)
                VALUES
                (@Cliente_Id, @Fecha, @TipoComprobante, @NumeroComprobante, @Total, @CAE, @VTO,
                 @Bonificacion, @PorcentajeBonif, @Anticipo, @ContraEntrega, @Cuotas, @ValorCuota, @Vendedor_Id, @GastosEnvio,
                 @EsElectronica, @EsPresupuesto, @ComprobanteAsociado_Id);
                SELECT LAST_INSERT_ID();";

            const string detalleQuery = @"
                INSERT INTO comprobante_detalle
                (factura_id, articulo_id, cantidad, precio_unitario, subtotal)
                VALUES
                (@Factura_Id, @Articulo_Id, @Cantidad, @Precio_Unitario, @Subtotal)";

            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var id = await connection.ExecuteScalarAsync<int>(comprobanteQuery, comprobante, transaction);
                comprobante.Id = id;

                foreach (var detalle in detalles)
                {
                    detalle.Factura_Id = id;
                    await connection.ExecuteAsync(detalleQuery, detalle, transaction);
                }

                // Guardar cuotas si hay cuotas definidas o contraentrega (dentro de la transacción)
                var tieneCuotas = comprobante.Cuotas > 0 && comprobante.ValorCuota > 0;
                var tieneContraEntrega = comprobante.ContraEntrega.HasValue && comprobante.ContraEntrega.Value > 0;
                if (tieneCuotas || tieneContraEntrega)
                {
                    var cuotas = GenerarCuotas(comprobante);
                    await _cuotaRepository.CreateCuotasAsync(id, cuotas, connection, transaction);
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

        private List<Cuota> GenerarCuotas(Comprobante comprobante)
        {
            var cuotas = new List<Cuota>();
            var fechaBase = comprobante.Fecha;

            // Cuota 0: Contraentrega (si existe)
            if (comprobante.ContraEntrega.HasValue && comprobante.ContraEntrega.Value > 0)
            {
                cuotas.Add(new Cuota
                {
                    Comprobante_Id = comprobante.Id,
                    NumeroCuota = 0,
                    Fecha = fechaBase, // Misma fecha del comprobante
                    Importe = comprobante.ContraEntrega.Value,
                    Estado = "PEN"
                });
            }

            // Cuotas 1 a N: Cuotas regulares
            for (int i = 1; i <= comprobante.Cuotas; i++)
            {
                var cuota = new Cuota
                {
                    Comprobante_Id = comprobante.Id,
                    NumeroCuota = i,
                    Fecha = fechaBase.AddMonths(i),
                    Importe = comprobante.ValorCuota,
                    Estado = "PEN"
                };
                cuotas.Add(cuota);
            }

            return cuotas;
        }

        public async Task<Comprobante?> UpdateAsync(int id, Comprobante comprobante, List<ComprobanteDetalle> detalles)
        {
            const string comprobanteQuery = @"
                UPDATE comprobantes
                SET cliente_id = @Cliente_Id,
                    fecha = @Fecha,
                    tipoComprobante = @TipoComprobante,
                    numeroComprobante = @NumeroComprobante,
                    total = @Total,
                    CAE = @CAE,
                    VTO = @VTO,
                    Bonificacion = @Bonificacion,
                    PorcentajeBonif = @PorcentajeBonif,
                    Anticipo = @Anticipo,
                    ContraEntrega = @ContraEntrega,
                    Cuotas = @Cuotas,
                    ValorCuota = @ValorCuota,
                    vendedor_id = @Vendedor_Id,
                    GastosEnvio = @GastosEnvio,
                    EsElectronica = @EsElectronica,
                    EsPresupuesto = @EsPresupuesto
                WHERE id = @Id";

            const string deleteDetallesQuery = "DELETE FROM comprobante_detalle WHERE factura_id = @Id";

            const string detalleQuery = @"
                INSERT INTO comprobante_detalle
                (factura_id, articulo_id, cantidad, precio_unitario, subtotal)
                VALUES
                (@Factura_Id, @Articulo_Id, @Cantidad, @Precio_Unitario, @Subtotal)";

            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var affectedRows = await connection.ExecuteAsync(comprobanteQuery,
                    new
                    {
                        Id = id,
                        comprobante.Cliente_Id,
                        comprobante.Fecha,
                        comprobante.TipoComprobante,
                        comprobante.NumeroComprobante,
                        comprobante.Total,
                        comprobante.CAE,
                        comprobante.VTO,
                        comprobante.Bonificacion,
                        comprobante.PorcentajeBonif,
                        comprobante.Anticipo,
                        comprobante.ContraEntrega,
                        comprobante.Cuotas,
                        comprobante.ValorCuota,
                        comprobante.Vendedor_Id,
                        comprobante.GastosEnvio,
                        comprobante.EsElectronica,
                        comprobante.EsPresupuesto
                    }, transaction);

                if (affectedRows == 0)
                {
                    transaction.Rollback();
                    return null;
                }

                // Eliminar detalles existentes
                await connection.ExecuteAsync(deleteDetallesQuery, new { Id = id }, transaction);

                // Insertar nuevos detalles
                foreach (var detalle in detalles)
                {
                    detalle.Factura_Id = id;
                    await connection.ExecuteAsync(detalleQuery, detalle, transaction);
                }

                // Actualizar cuotas (dentro de la transacción)
                await _cuotaRepository.DeleteByComprobanteIdAsync(id, connection, transaction);
                var tieneCuotas = comprobante.Cuotas > 0 && comprobante.ValorCuota > 0;
                var tieneContraEntrega = comprobante.ContraEntrega.HasValue && comprobante.ContraEntrega.Value > 0;
                if (tieneCuotas || tieneContraEntrega)
                {
                    comprobante.Id = id;
                    var cuotas = GenerarCuotas(comprobante);
                    await _cuotaRepository.CreateCuotasAsync(id, cuotas, connection, transaction);
                }

                transaction.Commit();
                comprobante.Id = id;
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
            const string deleteCuotasQuery = "DELETE FROM cuotas WHERE comprobante_id = @Id";
            const string deleteDetallesQuery = "DELETE FROM comprobante_detalle WHERE factura_id = @Id";
            const string deleteComprobanteQuery = "DELETE FROM comprobantes WHERE id = @Id";

            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync(deleteCuotasQuery, new { Id = id }, transaction);
                await connection.ExecuteAsync(deleteDetallesQuery, new { Id = id }, transaction);
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

        public async Task<List<ComprobanteDetalle>> GetDetallesByComprobanteIdAsync(int comprobanteId)
        {
            const string query = @"
                SELECT
                    cd.id AS Id,
                    cd.factura_id AS Factura_Id,
                    cd.articulo_id AS Articulo_Id,
                    cd.cantidad AS Cantidad,
                    cd.precio_unitario AS Precio_Unitario,
                    cd.subtotal AS Subtotal,
                    a.Descripcion AS ArticuloDescripcion
                FROM comprobante_detalle cd
                LEFT JOIN articulos a ON cd.articulo_id = a.id
                WHERE cd.factura_id = @ComprobanteId
                ORDER BY cd.id";

            using var connection = _context.CreateConnection();
            var detalles = await connection.QueryAsync<ComprobanteDetalle>(query, new { ComprobanteId = comprobanteId });
            return detalles.ToList();
        }

        public async Task<IEnumerable<IvaVentasDto>> GetIvaVentasAsync(DateTime fechaDesde, DateTime fechaHasta)
        {
            const string query = @"
                SELECT
                    c.fecha AS Fecha,
                    c.numeroComprobante AS NumeroComprobante,
                    cl.Nombre AS Nombre,
                    cl.NroDocumento AS NroDocumento,
                    c.total AS Total
                FROM comprobantes c
                INNER JOIN clientes cl ON c.cliente_id = cl.Id
                WHERE c.fecha >= @FechaDesde AND c.fecha <= @FechaHasta
                ORDER BY c.fecha, c.numeroComprobante";

            using var connection = _context.CreateConnection();
            var ventas = await connection.QueryAsync<IvaVentasDto>(query, new { FechaDesde = fechaDesde, FechaHasta = fechaHasta });
            return ventas;
        }

        public async Task<DeudoresReporteDto> GetDeudoresAsync(int mes, int anio, int? zonaId = null, int? vendedorId = null)
        {
            // Query para obtener comprobantes del mes/año especificado
            // Excluye comprobantes cancelados (que tienen una NC asociada) y las propias NC
            var comprobantesQuery = @"
                SELECT
                    c.id AS Id,
                    c.numeroComprobante AS NumeroComprobante,
                    cl.Nombre AS RazonSocial,
                    v.codigo AS CodigoVendedor,
                    COALESCE(c.Cuotas, 0) AS CantidadCuotas,
                    c.total AS TotalComprobante,
                    COALESCE(c.Anticipo, 0) AS Anticipo,
                    c.fecha AS Fecha
                FROM comprobantes c
                INNER JOIN clientes cl ON c.cliente_id = cl.Id
                LEFT JOIN vendedores v ON c.vendedor_id = v.id
                LEFT JOIN zonas z ON cl.Zona_Id = z.id
                WHERE MONTH(c.fecha) = @Mes AND YEAR(c.fecha) = @Anio
                  AND c.tipoComprobante != 'NC'
                  AND NOT EXISTS (
                      SELECT 1 FROM comprobantes nc
                      WHERE nc.comprobante_asociado_id = c.id
                      AND nc.tipoComprobante = 'NC'
                  )";

            if (zonaId.HasValue)
            {
                comprobantesQuery += " AND z.id = @ZonaId";
            }

            if (vendedorId.HasValue)
            {
                comprobantesQuery += " AND v.id = @VendedorId";
            }

            comprobantesQuery += " ORDER BY c.fecha, c.numeroComprobante";

            // Query para obtener todas las cuotas (incluyendo cuota 0 = contraentrega)
            // Excluye cuotas de comprobantes cancelados
            var cuotasQuery = @"
                SELECT
                    cu.Id,
                    cu.Comprobante_Id,
                    cu.numero_cuota AS NumeroCuota,
                    cu.Fecha,
                    COALESCE(cu.Importe, 0) AS Importe,
                    COALESCE(cu.importe_pagado, 0) AS ImportePagado,
                    cu.Estado
                FROM cuotas cu
                INNER JOIN comprobantes c ON cu.Comprobante_Id = c.id
                INNER JOIN clientes cl ON c.cliente_id = cl.Id
                LEFT JOIN zonas z ON cl.Zona_Id = z.id
                WHERE MONTH(c.fecha) = @Mes AND YEAR(c.fecha) = @Anio
                  AND c.tipoComprobante != 'NC'
                  AND NOT EXISTS (
                      SELECT 1 FROM comprobantes nc
                      WHERE nc.comprobante_asociado_id = c.id
                      AND nc.tipoComprobante = 'NC'
                  )";

            if (zonaId.HasValue)
            {
                cuotasQuery += " AND z.id = @ZonaId";
            }

            if (vendedorId.HasValue)
            {
                cuotasQuery += " AND c.vendedor_id = @VendedorId";
            }

            cuotasQuery += " ORDER BY cu.Comprobante_Id, cu.numero_cuota";

            // Query para obtener pagos agrupados por comprobante y mes de pago
            var pagosQuery = @"
                SELECT
                    cu.Comprobante_Id,
                    pc.Fecha AS FechaPago,
                    pc.Importe AS ImportePago
                FROM pagos_cuotas pc
                INNER JOIN cuotas cu ON pc.Cuota_Id = cu.Id
                INNER JOIN comprobantes c ON cu.Comprobante_Id = c.id
                INNER JOIN clientes cl ON c.cliente_id = cl.Id
                LEFT JOIN zonas z ON cl.Zona_Id = z.id
                WHERE MONTH(c.fecha) = @Mes AND YEAR(c.fecha) = @Anio
                  AND c.tipoComprobante != 'NC'
                  AND NOT EXISTS (
                      SELECT 1 FROM comprobantes nc
                      WHERE nc.comprobante_asociado_id = c.id
                      AND nc.tipoComprobante = 'NC'
                  )";

            if (zonaId.HasValue)
            {
                pagosQuery += " AND z.id = @ZonaId";
            }

            if (vendedorId.HasValue)
            {
                pagosQuery += " AND c.vendedor_id = @VendedorId";
            }

            using var connection = _context.CreateConnection();

            var queryParams = new { Mes = mes, Anio = anio, ZonaId = zonaId, VendedorId = vendedorId };
            var comprobantesData = await connection.QueryAsync<dynamic>(comprobantesQuery, queryParams);
            var cuotasData = await connection.QueryAsync<dynamic>(cuotasQuery, queryParams);
            var pagosData = await connection.QueryAsync<dynamic>(pagosQuery, queryParams);

            // Obtener nombre de la zona
            var zonaNombre = "Todas las zonas";
            if (zonaId.HasValue)
            {
                var zonaQuery = "SELECT descripcion FROM zonas WHERE id = @ZonaId";
                zonaNombre = await connection.QueryFirstOrDefaultAsync<string>(zonaQuery, new { ZonaId = zonaId }) ?? "Zona desconocida";
            }

            var resultado = new DeudoresReporteDto
            {
                Mes = mes,
                Anio = anio,
                ZonaNombre = zonaNombre,
                PeriodosCuotas = new List<string>(),
                Deudores = new List<DeudorItemDto>()
            };

            // Agrupar cuotas por comprobante
            var cuotasPorComprobante = cuotasData
                .GroupBy(c => (int)c.Comprobante_Id)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Agrupar pagos por comprobante
            var pagosPorComprobante = pagosData
                .GroupBy(p => (int)p.Comprobante_Id)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Columnas: C.Entrega (mes del filtro) + 10 meses consecutivos + Otros
            var periodoFiltro = new DateTime(anio, mes, 1).ToString("MM/yyyy");
            var periodosColumnas = new List<string>();
            periodosColumnas.Add("C.Entrega"); // mes del filtro = contraentrega
            var periodosMap = new Dictionary<string, string>(); // MM/yyyy -> nombre columna
            periodosMap[periodoFiltro] = "C.Entrega";
            for (int i = 1; i <= 10; i++)
            {
                var fecha = new DateTime(anio, mes, 1).AddMonths(i);
                var key = fecha.ToString("MM/yyyy");
                periodosColumnas.Add(key);
                periodosMap[key] = key;
            }

            foreach (var comp in comprobantesData)
            {
                var comprobanteId = (int)comp.Id;

                var cuotasComprobante = cuotasPorComprobante.TryGetValue(comprobanteId, out var cuotas)
                    ? cuotas.ToList()
                    : new List<dynamic>();

                // Buscar cuota 0 (contraentrega)
                var cuotaCero = cuotasComprobante.FirstOrDefault(c => (int)c.NumeroCuota == 0);
                decimal contraEntrega = cuotaCero != null ? (decimal)cuotaCero.Importe : 0;
                decimal contraEntregaPagado = cuotaCero != null ? (decimal)cuotaCero.ImportePagado : 0;

                var deudor = new DeudorItemDto
                {
                    ComprobanteId = comprobanteId,
                    NumeroComprobante = comp.NumeroComprobante ?? "",
                    RazonSocial = comp.RazonSocial ?? "",
                    CodigoVendedor = comp.CodigoVendedor,
                    CantidadCuotas = (int)comp.CantidadCuotas,
                    TotalComprobante = (decimal)comp.TotalComprobante,
                    Anticipo = (decimal)comp.Anticipo,
                    ContraEntrega = contraEntrega,
                    ContraEntregaPagado = contraEntregaPagado,
                    Cuotas = new List<CuotaDeudorDto>()
                };

                // Agrupar pagos por período (mes/año de fecha de pago)
                var pagosComprobante = pagosPorComprobante.TryGetValue(comprobanteId, out var pagos)
                    ? pagos.ToList()
                    : new List<dynamic>();

                var pagadoPorPeriodo = new Dictionary<string, decimal>();
                decimal pagadoOtros = 0;

                foreach (var pago in pagosComprobante)
                {
                    var fechaPago = (DateTime)pago.FechaPago;
                    var importePago = (decimal)pago.ImportePago;
                    var periodoRaw = fechaPago.ToString("MM/yyyy");

                    if (periodosMap.TryGetValue(periodoRaw, out var periodoCol))
                    {
                        if (!pagadoPorPeriodo.ContainsKey(periodoCol))
                            pagadoPorPeriodo[periodoCol] = 0;
                        pagadoPorPeriodo[periodoCol] += importePago;
                    }
                    else
                    {
                        pagadoOtros += importePago;
                    }
                }

                foreach (var kvp in pagadoPorPeriodo)
                {
                    deudor.Cuotas.Add(new CuotaDeudorDto
                    {
                        Periodo = kvp.Key,
                        ImportePagado = kvp.Value
                    });
                }

                if (pagadoOtros != 0)
                {
                    deudor.Cuotas.Add(new CuotaDeudorDto
                    {
                        Periodo = "Otros",
                        ImportePagado = pagadoOtros
                    });
                }

                // Calcular saldo total
                var totalPagadoCuotas = cuotasComprobante.Sum(c => (decimal)c.ImportePagado);
                deudor.Saldo = deudor.TotalComprobante - deudor.Anticipo - totalPagadoCuotas;

                resultado.Deudores.Add(deudor);
            }

            // Siempre mostrar todas las columnas: C.Entrega + 10 meses + Otros
            var periodosFinales = new List<string>(periodosColumnas);
            periodosFinales.Add("Otros");

            resultado.PeriodosCuotas = periodosFinales;

            return resultado;
        }

        public async Task<string> GetSiguienteNumeroPresupuestoAsync(string puntoVenta)
        {
            // Formato: 0001-00000001 (igual que factura electrónica, 14 caracteres)
            // El tipo PRE lo diferencia de las facturas electrónicas
            var patron = $"{puntoVenta}-%";

            const string query = @"
                SELECT numeroComprobante
                FROM comprobantes
                WHERE numeroComprobante LIKE @Patron
                  AND EsPresupuesto = 1
                ORDER BY numeroComprobante DESC
                LIMIT 1";

            using var connection = _context.CreateConnection();
            var ultimoNumero = await connection.QueryFirstOrDefaultAsync<string>(query, new { Patron = patron });

            int siguienteNumero = 1;
            if (!string.IsNullOrEmpty(ultimoNumero))
            {
                var partes = ultimoNumero.Split('-');
                if (partes.Length == 2 && int.TryParse(partes[1], out int numero))
                {
                    siguienteNumero = numero + 1;
                }
            }

            return $"{puntoVenta}-{siguienteNumero:D8}";
        }

        public async Task UpdateEstadoAsync(int comprobanteId, string estado)
        {
            const string query = "UPDATE comprobantes SET estado = @Estado WHERE id = @Id";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new { Id = comprobanteId, Estado = estado });
        }

        public async Task<decimal> GetUltimoGastoEnvioAsync()
        {
            const string query = @"
                SELECT COALESCE(GastosEnvio, 0)
                FROM comprobantes
                WHERE GastosEnvio IS NOT NULL AND GastosEnvio > 0
                ORDER BY id DESC
                LIMIT 1";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<decimal>(query);
        }

        public async Task<ArticulosVendidosZonaReporteDto> GetArticulosVendidosPorZonaAsync(int? zonaId)
        {
            // Fecha límite: últimos 3 años
            var fechaLimite = DateTime.Now.AddYears(-3);

            var query = @"
                SELECT
                    LEFT(COALESCE(v.descripcion, ''), 1) AS VendedorInicial,
                    COALESCE(cl.Codigo, '') AS CodigoCliente,
                    cl.Nombre AS RazonSocial,
                    COALESCE(cl.DomicilioParticular, '') AS Direccion,
                    COALESCE(cl.DomicilioComercial, '') AS DireccionComercial,
                    COALESCE(cl.Telefono, '') AS Telefono,
                    COALESCE(a.Descripcion, '') AS DescripcionArticulo,
                    c.fecha AS FechaFactura,
                    COALESCE(c.numeroComprobante, '') AS NumeroFactura
                FROM comprobantes c
                INNER JOIN clientes cl ON c.cliente_id = cl.Id
                LEFT JOIN vendedores v ON c.vendedor_id = v.id
                LEFT JOIN zonas z ON cl.Zona_Id = z.id
                INNER JOIN comprobante_detalle cd ON cd.factura_id = c.id
                INNER JOIN articulos a ON cd.articulo_id = a.Id
                WHERE c.fecha >= @FechaLimite
                  AND c.tipoComprobante IN ('FC', 'PRE')
                  AND NOT EXISTS (
                      SELECT 1 FROM comprobantes nc
                      WHERE nc.comprobante_asociado_id = c.id
                      AND nc.tipoComprobante = 'NC'
                  )";

            var parameters = new DynamicParameters();
            parameters.Add("FechaLimite", fechaLimite);

            if (zonaId.HasValue)
            {
                query += " AND z.id = @ZonaId";
                parameters.Add("ZonaId", zonaId.Value);
            }

            query += " ORDER BY cl.Nombre, c.fecha ASC, c.id, cd.id";

            using var connection = _context.CreateConnection();
            var items = await connection.QueryAsync<ArticuloVendidoZonaItemDto>(query, parameters);

            // Obtener nombre de la zona
            var zonaNombre = "Todas las zonas";
            if (zonaId.HasValue)
            {
                var zonaQuery = "SELECT descripcion FROM zonas WHERE id = @ZonaId";
                zonaNombre = await connection.QueryFirstOrDefaultAsync<string>(zonaQuery, new { ZonaId = zonaId }) ?? "Zona desconocida";
            }

            return new ArticulosVendidosZonaReporteDto
            {
                ZonaId = zonaId,
                ZonaNombre = zonaNombre,
                Items = items.ToList()
            };
        }
    }
}
