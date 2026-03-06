namespace BookstoreAPI.Repositories
{
    public interface IConfiguracionRepository
    {
        Task<Dictionary<string, string>> GetAllAsync();
        Task<string?> GetValueAsync(string clave);
        Task<byte[]?> GetBinaryValueAsync(string clave);
        Task SetValueAsync(string clave, string valor);
        Task SetBinaryValueAsync(string clave, byte[] valor);
        Task SaveAllAsync(Dictionary<string, string> valores);
    }
}
