import api from './api';
import type { CuotaListado, CreatePagoCuotaDto, PagoCuotaDto, CreatePagoComprobanteDto } from '../types/cuota';

export interface CuotaFiltros {
  zonaId?: number;
  fechaCorte?: string;
}

export const cuotaService = {
  getAll: async (filtros?: CuotaFiltros): Promise<CuotaListado[]> => {
    const params = new URLSearchParams();
    if (filtros?.zonaId) params.append('zonaId', filtros.zonaId.toString());
    if (filtros?.fechaCorte) params.append('fechaCorte', filtros.fechaCorte);

    const queryString = params.toString();
    const response = await api.get<CuotaListado[]>(`/cuotas${queryString ? `?${queryString}` : ''}`);
    return response.data;
  },

  createPago: async (cuotaId: number, data: CreatePagoCuotaDto): Promise<PagoCuotaDto> => {
    const response = await api.post<PagoCuotaDto>(`/cuotas/${cuotaId}/pagos`, data);
    return response.data;
  },

  deletePago: async (pagoId: number): Promise<void> => {
    await api.delete(`/cuotas/pagos/${pagoId}`);
  },

  createPagoComprobante: async (comprobanteId: number, data: CreatePagoComprobanteDto): Promise<void> => {
    await api.post(`/cuotas/comprobante/${comprobanteId}/pago`, data);
  },
};
