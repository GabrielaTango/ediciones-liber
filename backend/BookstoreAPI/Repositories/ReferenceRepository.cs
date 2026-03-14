using BookstoreAPI.Data;
using BookstoreAPI.Models;
using Dapper;

namespace BookstoreAPI.Repositories
{
    public class ReferenceRepository : IReferenceRepository
    {
        private readonly DapperContext _context;

        public ReferenceRepository(DapperContext context)
        {
            _context = context;
        }

        // Zona - CRUD Operations
        public async Task<IEnumerable<Zona>> GetAllZonasAsync()
        {
            const string query = "SELECT id AS Id, descripcion AS Descripcion FROM zonas ORDER BY descripcion";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Zona>(query);
        }

        public async Task<Zona?> GetZonaByIdAsync(int id)
        {
            const string query = "SELECT id AS Id, descripcion AS Descripcion FROM zonas WHERE id = @Id";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Zona>(query, new { Id = id });
        }

        public async Task<Zona> CreateZonaAsync(Zona zona)
        {
            const string checkQuery = "SELECT COUNT(*) FROM zonas WHERE LOWER(descripcion) = LOWER(@Descripcion)";
            const string query = @"
                INSERT INTO zonas (descripcion)
                VALUES (@Descripcion);
                SELECT LAST_INSERT_ID();";
            using var connection = _context.CreateConnection();
            var exists = await connection.ExecuteScalarAsync<int>(checkQuery, new { zona.Descripcion });
            if (exists > 0)
                throw new InvalidOperationException("Ya existe una zona con ese nombre");
            var id = await connection.ExecuteScalarAsync<int>(query, zona);
            zona.Id = id;
            return zona;
        }

        public async Task<Zona?> UpdateZonaAsync(int id, Zona zona)
        {
            const string checkQuery = "SELECT COUNT(*) FROM zonas WHERE LOWER(descripcion) = LOWER(@Descripcion) AND id != @Id";
            const string query = @"
                UPDATE zonas
                SET descripcion = @Descripcion
                WHERE id = @Id";
            using var connection = _context.CreateConnection();
            var exists = await connection.ExecuteScalarAsync<int>(checkQuery, new { Id = id, zona.Descripcion });
            if (exists > 0)
                throw new InvalidOperationException("Ya existe una zona con ese nombre");
            var affectedRows = await connection.ExecuteAsync(query, new { Id = id, zona.Descripcion });
            if (affectedRows == 0) return null;
            zona.Id = id;
            return zona;
        }

        public async Task<bool> DeleteZonaAsync(int id)
        {
            const string query = "DELETE FROM zonas WHERE id = @Id";
            using var connection = _context.CreateConnection();
            var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
            return affectedRows > 0;
        }

        // SubZona - CRUD Operations
        public async Task<IEnumerable<SubZona>> GetAllSubZonasAsync()
        {
            const string query = @"
                SELECT s.id AS Id, s.descripcion AS Descripcion, s.zona_id AS ZonaId,
                       s.provincia_id AS ProvinciaId, s.codigo_postal AS CodigoPostal, s.localidad AS Localidad,
                       p.descripcion AS ProvinciaDescripcion
                FROM subzonas s
                LEFT JOIN provincias p ON s.provincia_id = p.id
                ORDER BY s.descripcion";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<SubZona>(query);
        }

        public async Task<IEnumerable<SubZona>> GetSubZonasByZonaIdAsync(int zonaId)
        {
            const string query = @"
                SELECT s.id AS Id, s.descripcion AS Descripcion, s.zona_id AS ZonaId,
                       s.provincia_id AS ProvinciaId, s.codigo_postal AS CodigoPostal, s.localidad AS Localidad,
                       p.descripcion AS ProvinciaDescripcion
                FROM subzonas s
                LEFT JOIN provincias p ON s.provincia_id = p.id
                WHERE s.zona_id = @ZonaId
                ORDER BY s.descripcion";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<SubZona>(query, new { ZonaId = zonaId });
        }

        public async Task<SubZona?> GetSubZonaByIdAsync(int id)
        {
            const string query = @"
                SELECT s.id AS Id, s.descripcion AS Descripcion, s.zona_id AS ZonaId,
                       s.provincia_id AS ProvinciaId, s.codigo_postal AS CodigoPostal, s.localidad AS Localidad,
                       p.descripcion AS ProvinciaDescripcion
                FROM subzonas s
                LEFT JOIN provincias p ON s.provincia_id = p.id
                WHERE s.id = @Id";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<SubZona>(query, new { Id = id });
        }

        public async Task<SubZona> CreateSubZonaAsync(SubZona subZona)
        {
            const string checkQuery = "SELECT COUNT(*) FROM subzonas WHERE LOWER(descripcion) = LOWER(@Descripcion)";
            const string query = @"
                INSERT INTO subzonas (descripcion, zona_id, provincia_id, codigo_postal, localidad)
                VALUES (@Descripcion, @ZonaId, @ProvinciaId, @CodigoPostal, @Localidad);
                SELECT LAST_INSERT_ID();";
            using var connection = _context.CreateConnection();
            var exists = await connection.ExecuteScalarAsync<int>(checkQuery, new { subZona.Descripcion });
            if (exists > 0)
                throw new InvalidOperationException("Ya existe una subzona con ese nombre");
            var id = await connection.ExecuteScalarAsync<int>(query, subZona);
            subZona.Id = id;
            return subZona;
        }

        public async Task<SubZona?> UpdateSubZonaAsync(int id, SubZona subZona)
        {
            const string checkQuery = "SELECT COUNT(*) FROM subzonas WHERE LOWER(descripcion) = LOWER(@Descripcion) AND id != @Id";
            const string query = @"
                UPDATE subzonas
                SET descripcion = @Descripcion, zona_id = @ZonaId,
                    provincia_id = @ProvinciaId, codigo_postal = @CodigoPostal, localidad = @Localidad
                WHERE id = @Id";
            using var connection = _context.CreateConnection();
            var exists = await connection.ExecuteScalarAsync<int>(checkQuery, new { Id = id, subZona.Descripcion });
            if (exists > 0)
                throw new InvalidOperationException("Ya existe una subzona con ese nombre");
            var affectedRows = await connection.ExecuteAsync(query, new {
                Id = id,
                subZona.Descripcion,
                subZona.ZonaId,
                subZona.ProvinciaId,
                subZona.CodigoPostal,
                subZona.Localidad
            });
            if (affectedRows == 0) return null;
            subZona.Id = id;
            return subZona;
        }

        public async Task<bool> DeleteSubZonaAsync(int id)
        {
            const string query = "DELETE FROM subzonas WHERE id = @Id";
            using var connection = _context.CreateConnection();
            var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
            return affectedRows > 0;
        }

        // Provincia - CRUD Operations
        public async Task<IEnumerable<Provincia>> GetAllProvinciasAsync()
        {
            const string query = "SELECT id AS Id, descripcion AS Descripcion FROM provincias ORDER BY descripcion";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Provincia>(query);
        }

        public async Task<Provincia?> GetProvinciaByIdAsync(int id)
        {
            const string query = "SELECT id AS Id, descripcion AS Descripcion FROM provincias WHERE id = @Id";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Provincia>(query, new { Id = id });
        }

        public async Task<Provincia> CreateProvinciaAsync(Provincia provincia)
        {
            const string checkQuery = "SELECT COUNT(*) FROM provincias WHERE LOWER(descripcion) = LOWER(@Descripcion)";
            const string query = @"
                INSERT INTO provincias (descripcion)
                VALUES (@Descripcion);
                SELECT LAST_INSERT_ID();";
            using var connection = _context.CreateConnection();
            var exists = await connection.ExecuteScalarAsync<int>(checkQuery, new { provincia.Descripcion });
            if (exists > 0)
                throw new InvalidOperationException("Ya existe una provincia con ese nombre");
            var id = await connection.ExecuteScalarAsync<int>(query, provincia);
            provincia.Id = id;
            return provincia;
        }

        public async Task<Provincia?> UpdateProvinciaAsync(int id, Provincia provincia)
        {
            const string checkQuery = "SELECT COUNT(*) FROM provincias WHERE LOWER(descripcion) = LOWER(@Descripcion) AND id != @Id";
            const string query = @"
                UPDATE provincias
                SET descripcion = @Descripcion
                WHERE id = @Id";
            using var connection = _context.CreateConnection();
            var exists = await connection.ExecuteScalarAsync<int>(checkQuery, new { Id = id, provincia.Descripcion });
            if (exists > 0)
                throw new InvalidOperationException("Ya existe una provincia con ese nombre");
            var affectedRows = await connection.ExecuteAsync(query, new { Id = id, provincia.Descripcion });
            if (affectedRows == 0) return null;
            provincia.Id = id;
            return provincia;
        }

        public async Task<bool> DeleteProvinciaAsync(int id)
        {
            const string query = "DELETE FROM provincias WHERE id = @Id";
            using var connection = _context.CreateConnection();
            var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
            return affectedRows > 0;
        }

        // Vendedor - CRUD Operations
        public async Task<IEnumerable<Vendedor>> GetAllVendedoresAsync()
        {
            const string query = "SELECT id AS Id, descripcion AS Descripcion FROM vendedores ORDER BY descripcion";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Vendedor>(query);
        }

        public async Task<Vendedor?> GetVendedorByIdAsync(int id)
        {
            const string query = "SELECT id AS Id, descripcion AS Descripcion FROM vendedores WHERE id = @Id";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Vendedor>(query, new { Id = id });
        }

        public async Task<Vendedor> CreateVendedorAsync(Vendedor vendedor)
        {
            const string checkQuery = "SELECT COUNT(*) FROM vendedores WHERE LOWER(descripcion) = LOWER(@Descripcion)";
            const string query = @"
                INSERT INTO vendedores (descripcion)
                VALUES (@Descripcion);
                SELECT LAST_INSERT_ID();";
            using var connection = _context.CreateConnection();
            var exists = await connection.ExecuteScalarAsync<int>(checkQuery, new { vendedor.Descripcion });
            if (exists > 0)
                throw new InvalidOperationException("Ya existe un vendedor con ese nombre");
            var id = await connection.ExecuteScalarAsync<int>(query, vendedor);
            vendedor.Id = id;
            return vendedor;
        }

        public async Task<Vendedor?> UpdateVendedorAsync(int id, Vendedor vendedor)
        {
            const string checkQuery = "SELECT COUNT(*) FROM vendedores WHERE LOWER(descripcion) = LOWER(@Descripcion) AND id != @Id";
            const string query = @"
                UPDATE vendedores
                SET descripcion = @Descripcion
                WHERE id = @Id";
            using var connection = _context.CreateConnection();
            var exists = await connection.ExecuteScalarAsync<int>(checkQuery, new { Id = id, vendedor.Descripcion });
            if (exists > 0)
                throw new InvalidOperationException("Ya existe un vendedor con ese nombre");
            var affectedRows = await connection.ExecuteAsync(query, new { Id = id, vendedor.Descripcion });
            if (affectedRows == 0) return null;
            vendedor.Id = id;
            return vendedor;
        }

        public async Task<bool> DeleteVendedorAsync(int id)
        {
            const string query = "DELETE FROM vendedores WHERE id = @Id";
            using var connection = _context.CreateConnection();
            var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
            return affectedRows > 0;
        }

        // Transporte - CRUD Operations
        public async Task<IEnumerable<Transporte>> GetAllTransportesAsync()
        {
            const string query = @"
                SELECT t.id AS Id, t.nombre AS Nombre,
                       t.direccion AS Direccion, t.localidad AS Localidad,
                       t.provincia_id AS ProvinciaId, t.cuit AS Cuit,
                       p.descripcion AS ProvinciaDescripcion
                FROM transportes t
                LEFT JOIN provincias p ON t.provincia_id = p.id
                ORDER BY t.nombre";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Transporte>(query);
        }

        public async Task<Transporte?> GetTransporteByIdAsync(int id)
        {
            const string query = @"
                SELECT t.id AS Id, t.nombre AS Nombre,
                       t.direccion AS Direccion, t.localidad AS Localidad,
                       t.provincia_id AS ProvinciaId, t.cuit AS Cuit,
                       p.descripcion AS ProvinciaDescripcion
                FROM transportes t
                LEFT JOIN provincias p ON t.provincia_id = p.id
                WHERE t.id = @Id";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Transporte>(query, new { Id = id });
        }

        public async Task<Transporte> CreateTransporteAsync(Transporte transporte)
        {
            const string checkQuery = "SELECT COUNT(*) FROM transportes WHERE LOWER(nombre) = LOWER(@Nombre)";
            const string query = @"
                INSERT INTO transportes (nombre, direccion, localidad, provincia_id, cuit)
                VALUES (@Nombre, @Direccion, @Localidad, @ProvinciaId, @Cuit);
                SELECT LAST_INSERT_ID();";
            using var connection = _context.CreateConnection();
            var exists = await connection.ExecuteScalarAsync<int>(checkQuery, new { transporte.Nombre });
            if (exists > 0)
                throw new InvalidOperationException("Ya existe un transporte con ese nombre");
            var id = await connection.ExecuteScalarAsync<int>(query, transporte);
            transporte.Id = id;
            return transporte;
        }

        public async Task<Transporte?> UpdateTransporteAsync(int id, Transporte transporte)
        {
            const string checkQuery = "SELECT COUNT(*) FROM transportes WHERE LOWER(nombre) = LOWER(@Nombre) AND id != @Id";
            const string query = @"
                UPDATE transportes
                SET nombre = @Nombre, direccion = @Direccion,
                    localidad = @Localidad, provincia_id = @ProvinciaId, cuit = @Cuit
                WHERE id = @Id";
            using var connection = _context.CreateConnection();
            var exists = await connection.ExecuteScalarAsync<int>(checkQuery, new { Id = id, transporte.Nombre });
            if (exists > 0)
                throw new InvalidOperationException("Ya existe un transporte con ese nombre");
            var affectedRows = await connection.ExecuteAsync(query, new {
                Id = id,
                transporte.Nombre,
                transporte.Direccion,
                transporte.Localidad,
                transporte.ProvinciaId,
                transporte.Cuit
            });
            if (affectedRows == 0) return null;
            transporte.Id = id;
            return transporte;
        }

        public async Task<bool> DeleteTransporteAsync(int id)
        {
            const string query = "DELETE FROM transportes WHERE id = @Id";
            using var connection = _context.CreateConnection();
            var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
            return affectedRows > 0;
        }
    }
}
