import api, { openAuthenticatedPdf } from './api';
import type { Remito, CreateRemitoDto, UpdateRemitoDto } from '../types/remito';

export const remitoService = {
  getAll: async (): Promise<Remito[]> => {
    const response = await api.get<Remito[]>('/remitos');
    return response.data;
  },

  getById: async (id: number): Promise<Remito> => {
    const response = await api.get<Remito>(`/remitos/${id}`);
    return response.data;
  },

  create: async (data: CreateRemitoDto): Promise<Remito> => {
    const response = await api.post<Remito>('/remitos', data);
    return response.data;
  },

  update: async (id: number, data: UpdateRemitoDto): Promise<Remito> => {
    const response = await api.put<Remito>(`/remitos/${id}`, data);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/remitos/${id}`);
  },

  openPdf: async (id: number): Promise<void> => {
    await openAuthenticatedPdf(`/remitos/${id}/pdf`);
  },

  openEtiquetasPdf: async (id: number): Promise<void> => {
    await openAuthenticatedPdf(`/remitos/${id}/etiquetas-pdf`);
  },

  openCompletoPdf: async (id: number): Promise<void> => {
    await openAuthenticatedPdf(`/remitos/${id}/completo-pdf`);
  },
};
