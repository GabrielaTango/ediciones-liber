using BookstoreAPI.Data;
using BookstoreAPI.Models;
using Dapper;

namespace BookstoreAPI.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly DapperContext _context;

        public UsuarioRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Usuario>> GetAllAsync()
        {
            const string query = @"
                SELECT Id, Username, NombreCompleto, Email, Rol, Activo, FechaCreacion, UltimoAcceso
                FROM usuarios
                ORDER BY NombreCompleto";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Usuario>(query);
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            const string query = @"
                SELECT Id, Username, NombreCompleto, Email, Rol, Activo, FechaCreacion, UltimoAcceso
                FROM usuarios
                WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Usuario>(query, new { Id = id });
        }

        public async Task<Usuario?> GetByUsernameAsync(string username)
        {
            const string query = @"
                SELECT Id, Username, PasswordHash, NombreCompleto, Email, Rol, Activo, FechaCreacion, UltimoAcceso
                FROM usuarios
                WHERE Username = @Username";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Usuario>(query, new { Username = username });
        }

        public async Task<int> CreateAsync(Usuario usuario)
        {
            const string query = @"
                INSERT INTO usuarios (Username, PasswordHash, NombreCompleto, Email, Rol, Activo, FechaCreacion)
                VALUES (@Username, @PasswordHash, @NombreCompleto, @Email, @Rol, @Activo, @FechaCreacion);
                SELECT LAST_INSERT_ID();";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(query, usuario);
        }

        public async Task<bool> UpdateAsync(int id, Usuario usuario)
        {
            const string query = @"
                UPDATE usuarios
                SET NombreCompleto = @NombreCompleto,
                    Email = @Email,
                    Rol = @Rol,
                    Activo = @Activo
                WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(query, new
            {
                Id = id,
                usuario.NombreCompleto,
                usuario.Email,
                usuario.Rol,
                usuario.Activo
            });
            return rowsAffected > 0;
        }

        public async Task<bool> UpdatePasswordAsync(int id, string passwordHash)
        {
            const string query = @"
                UPDATE usuarios SET PasswordHash = @PasswordHash WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(query, new { Id = id, PasswordHash = passwordHash });
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string query = "DELETE FROM usuarios WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateUltimoAccesoAsync(int id)
        {
            const string query = @"
                UPDATE usuarios SET UltimoAcceso = NOW() WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });
            return rowsAffected > 0;
        }
    }
}
