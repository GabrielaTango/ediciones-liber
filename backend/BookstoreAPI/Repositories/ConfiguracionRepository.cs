using BookstoreAPI.Data;
using Dapper;

namespace BookstoreAPI.Repositories
{
    public class ConfiguracionRepository : IConfiguracionRepository
    {
        private readonly DapperContext _context;

        public ConfiguracionRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, string>> GetAllAsync()
        {
            const string query = "SELECT Clave, Valor FROM configuracion WHERE Valor IS NOT NULL";
            using var connection = _context.CreateConnection();
            var rows = await connection.QueryAsync<(string Clave, string Valor)>(query);
            return rows.ToDictionary(r => r.Clave, r => r.Valor);
        }

        public async Task<string?> GetValueAsync(string clave)
        {
            const string query = "SELECT Valor FROM configuracion WHERE Clave = @Clave";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<string?>(query, new { Clave = clave });
        }

        public async Task<byte[]?> GetBinaryValueAsync(string clave)
        {
            const string query = "SELECT ValorBinario FROM configuracion WHERE Clave = @Clave";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<byte[]?>(query, new { Clave = clave });
        }

        public async Task SetValueAsync(string clave, string valor)
        {
            const string query = @"
                INSERT INTO configuracion (Clave, Valor) VALUES (@Clave, @Valor)
                ON DUPLICATE KEY UPDATE Valor = @Valor";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new { Clave = clave, Valor = valor });
        }

        public async Task SetBinaryValueAsync(string clave, byte[] valor)
        {
            const string query = @"
                INSERT INTO configuracion (Clave, ValorBinario) VALUES (@Clave, @Valor)
                ON DUPLICATE KEY UPDATE ValorBinario = @Valor";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new { Clave = clave, Valor = valor });
        }

        public async Task SaveAllAsync(Dictionary<string, string> valores)
        {
            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string query = @"
                    INSERT INTO configuracion (Clave, Valor) VALUES (@Clave, @Valor)
                    ON DUPLICATE KEY UPDATE Valor = @Valor";

                foreach (var kvp in valores)
                {
                    await connection.ExecuteAsync(query, new { Clave = kvp.Key, Valor = kvp.Value }, transaction);
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
