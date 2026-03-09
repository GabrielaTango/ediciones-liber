using Dapper;
using BookstoreAPI.Data;
using BookstoreAPI.Models;

namespace BookstoreAPI.Repositories
{
    public class GastoRepository : IGastoRepository
    {
        private readonly DapperContext _context;

        public GastoRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Gasto>> GetAllAsync()
        {
            const string query = @"
                SELECT Id, NroComprobante, Importe, Categoria, Descripcion, Fecha
                FROM gastos
                ORDER BY Fecha DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Gasto>(query);
        }

        public async Task<Gasto?> GetByIdAsync(int id)
        {
            const string query = @"
                SELECT Id, NroComprobante, Importe, Categoria, Descripcion, Fecha
                FROM gastos
                WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Gasto>(query, new { Id = id });
        }

        public async Task<int> CreateAsync(Gasto gasto)
        {
            const string query = @"
                INSERT INTO gastos (NroComprobante, Importe, Categoria, Descripcion, Fecha)
                VALUES (@NroComprobante, @Importe, @Categoria, @Descripcion, @Fecha);
                SELECT LAST_INSERT_ID();";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(query, gasto);
        }

        public async Task<bool> UpdateAsync(int id, Gasto gasto)
        {
            const string query = @"
                UPDATE gastos
                SET NroComprobante = @NroComprobante,
                    Importe = @Importe,
                    Categoria = @Categoria,
                    Descripcion = @Descripcion,
                    Fecha = @Fecha
                WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(query, new
            {
                Id = id,
                gasto.NroComprobante,
                gasto.Importe,
                gasto.Categoria,
                gasto.Descripcion,
                gasto.Fecha
            });
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string query = "DELETE FROM gastos WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            const string query = "SELECT COUNT(1) FROM gastos WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

        public async Task<IEnumerable<Gasto>> GetByFechaRangoAsync(DateTime fechaDesde, DateTime fechaHasta)
        {
            const string query = @"
                SELECT Id, NroComprobante, Importe, Categoria, Descripcion, Fecha
                FROM gastos
                WHERE Fecha >= @FechaDesde AND Fecha <= @FechaHasta
                ORDER BY Fecha DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Gasto>(query, new { FechaDesde = fechaDesde, FechaHasta = fechaHasta });
        }

        public async Task<IEnumerable<string>> GetCategoriasAsync()
        {
            const string query = @"
                SELECT DISTINCT Categoria
                FROM gastos
                WHERE Categoria IS NOT NULL AND Categoria != ''
                ORDER BY Categoria";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<string>(query);
        }
    }
}
