using BookstoreAPI.Data;
using BookstoreAPI.Models;
using Dapper;

namespace BookstoreAPI.Repositories
{
    public class ProveedorRepository : IProveedorRepository
    {
        private readonly DapperContext _context;

        public ProveedorRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Proveedor>> GetAllAsync()
        {
            const string query = @"
                SELECT Id, Codigo, Nombre, RazonSocial, Cuit, Domicilio,
                       Telefono, Mail, FechaInhabilitacion
                FROM proveedores
                ORDER BY Nombre";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Proveedor>(query);
        }

        public async Task<Proveedor?> GetByIdAsync(int id)
        {
            const string query = @"
                SELECT Id, Codigo, Nombre, RazonSocial, Cuit, Domicilio,
                       Telefono, Mail, FechaInhabilitacion
                FROM proveedores
                WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Proveedor>(query, new { Id = id });
        }

        public async Task<int> CreateAsync(Proveedor proveedor)
        {
            const string query = @"
                INSERT INTO proveedores (
                    Codigo, Nombre, RazonSocial, Cuit, Domicilio,
                    Telefono, Mail, FechaInhabilitacion
                ) VALUES (
                    @Codigo, @Nombre, @RazonSocial, @Cuit, @Domicilio,
                    @Telefono, @Mail, @FechaInhabilitacion
                );
                SELECT LAST_INSERT_ID();";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(query, proveedor);
        }

        public async Task<bool> UpdateAsync(int id, Proveedor proveedor)
        {
            const string query = @"
                UPDATE proveedores SET
                    Codigo = @Codigo,
                    Nombre = @Nombre,
                    RazonSocial = @RazonSocial,
                    Cuit = @Cuit,
                    Domicilio = @Domicilio,
                    Telefono = @Telefono,
                    Mail = @Mail,
                    FechaInhabilitacion = @FechaInhabilitacion
                WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            var affectedRows = await connection.ExecuteAsync(query, new
            {
                Id = id,
                proveedor.Codigo,
                proveedor.Nombre,
                proveedor.RazonSocial,
                proveedor.Cuit,
                proveedor.Domicilio,
                proveedor.Telefono,
                proveedor.Mail,
                proveedor.FechaInhabilitacion
            });

            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string query = "DELETE FROM proveedores WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
            return affectedRows > 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            const string query = "SELECT COUNT(1) FROM proveedores WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

        public async Task<int> GetNextCodigoAsync()
        {
            const string query = @"
                SELECT COALESCE(MAX(CAST(Codigo AS UNSIGNED)), 0) + 1
                FROM proveedores
                WHERE Codigo REGEXP '^[0-9]+$'";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(query);
        }
    }
}
