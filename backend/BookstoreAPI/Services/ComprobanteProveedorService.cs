using BookstoreAPI.DTOs;
using BookstoreAPI.Models;
using BookstoreAPI.Repositories;

namespace BookstoreAPI.Services
{
    public class ComprobanteProveedorService : IComprobanteProveedorService
    {
        private readonly IComprobanteProveedorRepository _repository;

        public ComprobanteProveedorService(IComprobanteProveedorRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ComprobanteProveedorListDto>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<ComprobanteProveedorListDto?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<ComprobanteProveedorDetailDto?> GetDetailByIdAsync(int id)
        {
            return await _repository.GetDetailByIdAsync(id);
        }

        public async Task<ComprobanteProveedorListDto> CreateAsync(CreateComprobanteProveedorDto dto)
        {
            var comprobante = new ComprobanteProveedor
            {
                Proveedor_Id = dto.Proveedor_Id,
                TipoComprobante = dto.TipoComprobante,
                FechaEmision = dto.FechaEmision,
                NroComprobante = dto.NroComprobante,
                ImporteTotal = dto.ImporteTotal,
                CantidadCuotas = dto.CantidadCuotas,
                FechaPrimerVencimiento = dto.FechaPrimerVencimiento
            };

            var cuotas = GenerarCuotas(dto.ImporteTotal, dto.CantidadCuotas, dto.FechaPrimerVencimiento);

            var created = await _repository.CreateAsync(comprobante, cuotas);

            return await _repository.GetByIdAsync(created.Id)
                ?? throw new Exception("Error al recuperar el comprobante creado");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        private List<CuotaProveedor> GenerarCuotas(decimal importeTotal, int cantidadCuotas, DateTime fechaPrimerVencimiento)
        {
            var cuotas = new List<CuotaProveedor>();
            var valorBase = Math.Round(importeTotal / cantidadCuotas, 2);
            var ultimaCuota = importeTotal - (valorBase * (cantidadCuotas - 1));

            for (int i = 1; i <= cantidadCuotas; i++)
            {
                var fechaVencimiento = AddMonthsSafe(fechaPrimerVencimiento, i - 1);

                cuotas.Add(new CuotaProveedor
                {
                    NumeroCuota = i,
                    FechaVencimiento = fechaVencimiento,
                    Importe = (i == cantidadCuotas) ? ultimaCuota : valorBase,
                    ImportePagado = 0,
                    Estado = "PEN"
                });
            }

            return cuotas;
        }

        private DateTime AddMonthsSafe(DateTime date, int months)
        {
            if (months == 0) return date;

            var targetMonth = date.Month + months;
            var targetYear = date.Year;

            while (targetMonth > 12)
            {
                targetMonth -= 12;
                targetYear++;
            }

            var daysInTargetMonth = DateTime.DaysInMonth(targetYear, targetMonth);
            var targetDay = Math.Min(date.Day, daysInTargetMonth);

            return new DateTime(targetYear, targetMonth, targetDay);
        }
    }
}
