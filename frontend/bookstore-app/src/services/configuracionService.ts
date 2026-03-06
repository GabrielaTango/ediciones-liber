import api from './api';
import type { AfipConfigDto, AfipConfigUpdateDto, UltimoComprobanteDto } from '../types/configuracion';

export const configuracionService = {
  getAfipConfig: async (): Promise<AfipConfigDto> => {
    const response = await api.get<AfipConfigDto>('/configuracion/afip');
    return response.data;
  },

  updateAfipConfig: async (data: AfipConfigUpdateDto): Promise<void> => {
    await api.put('/configuracion/afip', data);
  },

  getUltimoComprobante: async (puntoVenta?: number): Promise<UltimoComprobanteDto[]> => {
    const params = puntoVenta ? `?puntoVenta=${puntoVenta}` : '';
    const response = await api.get<UltimoComprobanteDto[]>(`/configuracion/afip/ultimo-comprobante${params}`);
    return response.data;
  },
};
