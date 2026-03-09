using BookstoreAPI.DTOs;
using BookstoreAPI.Models;
using BookstoreAPI.Repositories;

namespace BookstoreAPI.Services
{
    public class GastoService : IGastoService
    {
        private readonly IGastoRepository _gastoRepository;

        public GastoService(IGastoRepository gastoRepository)
        {
            _gastoRepository = gastoRepository;
        }

        public async Task<IEnumerable<Gasto>> GetAllGastosAsync()
        {
            return await _gastoRepository.GetAllAsync();
        }

        public async Task<Gasto?> GetGastoByIdAsync(int id)
        {
            return await _gastoRepository.GetByIdAsync(id);
        }

        public async Task<Gasto> CreateGastoAsync(CreateGastoDto createDto)
        {
            var gasto = new Gasto
            {
                NroComprobante = createDto.NroComprobante,
                Importe = createDto.Importe,
                Categoria = createDto.Categoria,
                Descripcion = createDto.Descripcion,
                Fecha = createDto.Fecha
            };

            var id = await _gastoRepository.CreateAsync(gasto);
            gasto.Id = id;
            return gasto;
        }

        public async Task<Gasto?> UpdateGastoAsync(int id, UpdateGastoDto updateDto)
        {
            var exists = await _gastoRepository.ExistsAsync(id);
            if (!exists)
            {
                return null;
            }

            var gasto = new Gasto
            {
                Id = id,
                NroComprobante = updateDto.NroComprobante,
                Importe = updateDto.Importe,
                Categoria = updateDto.Categoria,
                Descripcion = updateDto.Descripcion,
                Fecha = updateDto.Fecha
            };

            await _gastoRepository.UpdateAsync(id, gasto);
            return gasto;
        }

        public async Task<bool> DeleteGastoAsync(int id)
        {
            var exists = await _gastoRepository.ExistsAsync(id);
            if (!exists)
            {
                return false;
            }

            return await _gastoRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<string>> GetCategoriasAsync()
        {
            return await _gastoRepository.GetCategoriasAsync();
        }

        public async Task<IEnumerable<Gasto>> GetGastosByFechaRangoAsync(DateTime fechaDesde, DateTime fechaHasta)
        {
            return await _gastoRepository.GetByFechaRangoAsync(fechaDesde, fechaHasta);
        }
    }
}
