import api from './api';
import type { AfipConfigDto, AfipConfigUpdateDto, CertificadoEstadoDto, UltimoComprobanteDto } from '../types/configuracion';

export const configuracionService = {
  getAfipConfig: async (): Promise<AfipConfigDto> => {
    const response = await api.get<AfipConfigDto>('/configuracion/afip');
    return response.data;
  },

  updateAfipConfig: async (data: AfipConfigUpdateDto): Promise<void> => {
    await api.put('/configuracion/afip', data);
  },

  getCertificadoEstado: async (): Promise<CertificadoEstadoDto> => {
    const response = await api.get<CertificadoEstadoDto>('/configuracion/afip/certificado');
    return response.data;
  },

  getUltimoComprobante: async (puntoVenta?: number): Promise<UltimoComprobanteDto[]> => {
    const params = puntoVenta ? `?puntoVenta=${puntoVenta}` : '';
    const response = await api.get<UltimoComprobanteDto[]>(`/configuracion/afip/ultimo-comprobante${params}`);
    return response.data;
  },

  getBackupPath: async (): Promise<string> => {
    const response = await api.get<{ ruta: string }>('/configuracion/backup-path');
    return response.data.ruta;
  },

  setBackupPath: async (ruta: string): Promise<void> => {
    await api.put('/configuracion/backup-path', { ruta });
  },

  listarArchivosBackup: async (): Promise<string[]> => {
    const response = await api.get<string[]>('/configuracion/backup/archivos');
    return response.data;
  },

  realizarBackup: async (): Promise<{ message: string; archivo: string }> => {
    const response = await api.post<{ message: string; archivo: string }>('/configuracion/backup');
    return response.data;
  },

  restaurarBackup: async (archivo: string): Promise<void> => {
    await api.post('/configuracion/restore', { archivo });
  },

  vaciarDatos: async (): Promise<void> => {
    await api.post('/configuracion/vaciar-datos');
  },
};
