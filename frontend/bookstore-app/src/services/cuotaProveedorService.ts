import api from './api';
import type { CuotaProveedorListado, CuotaProveedorFiltros, CreatePagoCuotaProveedorDto, PagoCuotaProveedorDto } from '../types/cuotaProveedor';

export const cuotaProveedorService = {
  getAll: async (filtros?: CuotaProveedorFiltros): Promise<CuotaProveedorListado[]> => {
    const params: Record<string, string> = {};
    if (filtros?.proveedorId) params.proveedorId = filtros.proveedorId.toString();
    if (filtros?.fechaDesde) params.fechaDesde = filtros.fechaDesde;
    if (filtros?.fechaHasta) params.fechaHasta = filtros.fechaHasta;
    if (filtros?.estado) params.estado = filtros.estado;

    const response = await api.get<CuotaProveedorListado[]>('/cuotasproveedores', { params });
    return response.data;
  },

  createPago: async (cuotaId: number, data: CreatePagoCuotaProveedorDto): Promise<PagoCuotaProveedorDto> => {
    const response = await api.post<PagoCuotaProveedorDto>(`/cuotasproveedores/${cuotaId}/pagos`, data);
    return response.data;
  },

  deletePago: async (pagoId: number): Promise<void> => {
    await api.delete(`/cuotasproveedores/pagos/${pagoId}`);
  },
};
