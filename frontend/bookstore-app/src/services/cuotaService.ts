import api from './api';
import type { CuotaListado, UpdateImportePagadoDto } from '../types/cuota';

export interface CuotaFiltros {
  zonaId?: number;
  mes?: number;
  anio?: number;
}

export const cuotaService = {
  getAll: async (filtros?: CuotaFiltros): Promise<CuotaListado[]> => {
    const params = new URLSearchParams();
    if (filtros?.zonaId) params.append('zonaId', filtros.zonaId.toString());
    if (filtros?.mes) params.append('mes', filtros.mes.toString());
    if (filtros?.anio) params.append('anio', filtros.anio.toString());

    const queryString = params.toString();
    const response = await api.get<CuotaListado[]>(`/cuotas${queryString ? `?${queryString}` : ''}`);
    return response.data;
  },

  updateImportePagado: async (id: number, dto: UpdateImportePagadoDto): Promise<void> => {
    await api.put(`/cuotas/${id}/importe-pagado`, dto);
  },
};
