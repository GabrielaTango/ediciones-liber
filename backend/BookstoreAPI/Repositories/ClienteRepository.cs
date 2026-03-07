using BookstoreAPI.Data;
using BookstoreAPI.Models;
using Dapper;

namespace BookstoreAPI.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly DapperContext _context;

        public ClienteRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Cliente>> GetAllAsync()
        {
            const string query = @"
                SELECT Id, Codigo, Nombre, Zona_Id, SubZona_Id, Vendedor_Id,
                       DomicilioComercial, DomicilioParticular, Provincia_Id, CodigoPostal,
                       FechaAlta, FechaInha, SoloContado, Telefono, TelefonoMovil, EMail,
                       Contacto, TipoDocumento, NroDocumento, NroIIBB, CategoriaIva,
                       CondicionPago, Descuento, Observaciones, TipoDocArca
                FROM clientes
                ORDER BY Nombre";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Cliente>(query);
        }

        public async Task<Cliente?> GetByIdAsync(int id)
        {
            const string query = @"
                SELECT c.Id, c.Codigo, c.Nombre, c.Zona_Id, c.SubZona_Id, c.Vendedor_Id,
                       c.DomicilioComercial, c.DomicilioParticular, c.Provincia_Id, c.CodigoPostal,
                       c.FechaAlta, c.FechaInha, c.SoloContado, c.Telefono, c.TelefonoMovil, c.EMail,
                       c.Contacto, c.TipoDocumento, c.NroDocumento, c.NroIIBB, c.CategoriaIva,
                       c.CondicionPago, c.Descuento, c.Observaciones, c.TipoDocArca,
                       p.descripcion AS ProvinciaDescripcion,
                       sz.localidad AS Localidad,
                       z.descripcion AS ZonaDescripcion,
                       sz.descripcion AS SubZonaDescripcion
                FROM clientes c
                LEFT JOIN provincias p ON c.Provincia_Id = p.id
                LEFT JOIN subzonas sz ON c.SubZona_Id = sz.id
                LEFT JOIN zonas z ON c.Zona_Id = z.id
                WHERE c.Id = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Cliente>(query, new { Id = id });
        }

        public async Task<Cliente?> GetByCodigoAsync(string codigo)
        {
            const string query = @"
                SELECT Id, Codigo, Nombre, Zona_Id, SubZona_Id, Vendedor_Id,
                       DomicilioComercial, DomicilioParticular, Provincia_Id, CodigoPostal,
                       FechaAlta, FechaInha, SoloContado, Telefono, TelefonoMovil, EMail,
                       Contacto, TipoDocumento, NroDocumento, NroIIBB, CategoriaIva,
                       CondicionPago, Descuento, Observaciones, TipoDocArca
                FROM clientes
                WHERE Codigo = @Codigo";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Cliente>(query, new { Codigo = codigo });
        }

        public async Task<int> CreateAsync(Cliente cliente)
        {
            const string query = @"
                INSERT INTO clientes (
                    Codigo, Nombre, Zona_Id, SubZona_Id, Vendedor_Id,
                    DomicilioComercial, DomicilioParticular, Provincia_Id, CodigoPostal,
                    FechaAlta, FechaInha, SoloContado, Telefono, TelefonoMovil, EMail,
                    Contacto, TipoDocumento, NroDocumento, NroIIBB, CategoriaIva,
                    CondicionPago, Descuento, Observaciones, TipoDocArca
                ) VALUES (
                    @Codigo, @Nombre, @Zona_Id, @SubZona_Id, @Vendedor_Id,
                    @DomicilioComercial, @DomicilioParticular, @Provincia_Id, @CodigoPostal,
                    @FechaAlta, @FechaInha, @SoloContado, @Telefono, @TelefonoMovil, @EMail,
                    @Contacto, @TipoDocumento, @NroDocumento, @NroIIBB, @CategoriaIva,
                    @CondicionPago, @Descuento, @Observaciones, @TipoDocArca
                );
                SELECT LAST_INSERT_ID();";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(query, cliente);
        }

        public async Task<bool> UpdateAsync(int id, Cliente cliente)
        {
            const string query = @"
                UPDATE clientes SET
                    Codigo = @Codigo,
                    Nombre = @Nombre,
                    Zona_Id = @Zona_Id,
                    SubZona_Id = @SubZona_Id,
                    Vendedor_Id = @Vendedor_Id,
                    DomicilioComercial = @DomicilioComercial,
                    DomicilioParticular = @DomicilioParticular,
                    Provincia_Id = @Provincia_Id,
                    CodigoPostal = @CodigoPostal,
                    FechaInha = @FechaInha,
                    SoloContado = @SoloContado,
                    Telefono = @Telefono,
                    TelefonoMovil = @TelefonoMovil,
                    EMail = @EMail,
                    Contacto = @Contacto,
                    TipoDocumento = @TipoDocumento,
                    NroDocumento = @NroDocumento,
                    NroIIBB = @NroIIBB,
                    CategoriaIva = @CategoriaIva,
                    CondicionPago = @CondicionPago,
                    Descuento = @Descuento,
                    Observaciones = @Observaciones,
                    TipoDocArca = @TipoDocArca
                WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            var affectedRows = await connection.ExecuteAsync(query, new
            {
                Id = id,
                cliente.Codigo,
                cliente.Nombre,
                cliente.Zona_Id,
                cliente.SubZona_Id,
                cliente.Vendedor_Id,
                cliente.DomicilioComercial,
                cliente.DomicilioParticular,
                cliente.Provincia_Id,
                cliente.CodigoPostal,
                cliente.FechaInha,
                cliente.SoloContado,
                cliente.Telefono,
                cliente.TelefonoMovil,
                cliente.EMail,
                cliente.Contacto,
                cliente.TipoDocumento,
                cliente.NroDocumento,
                cliente.NroIIBB,
                cliente.CategoriaIva,
                cliente.CondicionPago,
                cliente.Descuento,
                cliente.Observaciones,
                cliente.TipoDocArca
            });

            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string query = "DELETE FROM clientes WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
            return affectedRows > 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            const string query = "SELECT COUNT(1) FROM clientes WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

        public async Task<int> GetNextCodigoAsync()
        {
            const string query = @"
                SELECT COALESCE(MAX(CAST(Codigo AS UNSIGNED)), 0) + 1
                FROM clientes
                WHERE Codigo REGEXP '^[0-9]+$'";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(query);
        }
    }
}
