using BookstoreAPI.Models.Afip;
using BookstoreAPI.Repositories;

namespace BookstoreAPI.Services.Afip
{
    public interface IAfipConfigProvider
    {
        Task<AfipConfig> GetConfigAsync();
    }

    public class AfipConfigProvider : IAfipConfigProvider
    {
        private readonly IConfiguracionRepository _repo;

        public AfipConfigProvider(IConfiguracionRepository repo)
        {
            _repo = repo;
        }

        public async Task<AfipConfig> GetConfigAsync()
        {
            var valores = await _repo.GetAllAsync();
            var crtBytes = await _repo.GetBinaryValueAsync("Afip_Crt");
            var keyBytes = await _repo.GetBinaryValueAsync("Afip_Key");

            return new AfipConfig
            {
                CUIT = valores.GetValueOrDefault("Afip_CUIT", ""),
                WsaaUrl = valores.GetValueOrDefault("Afip_WsaaUrl", ""),
                WsfevUrl = valores.GetValueOrDefault("Afip_WsfevUrl", ""),
                PuntoVenta = int.TryParse(valores.GetValueOrDefault("Afip_PuntoVenta", "0"), out var pv) ? pv : 0,
                IsProduction = valores.GetValueOrDefault("Afip_IsProduction", "false").Equals("true", StringComparison.OrdinalIgnoreCase),
                CrtBytes = crtBytes,
                KeyBytes = keyBytes
            };
        }
    }
}
