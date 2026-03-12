import api, { openAuthenticatedPdf } from './api';
import type { Gasto, CreateGastoDto, UpdateGastoDto } from '../types/gasto';

export const gastoService = {
  getAll: async (): Promise<Gasto[]> => {
    const response = await api.get<Gasto[]>('/gastos');
    return response.data;
  },

  getById: async (id: number): Promise<Gasto> => {
    const response = await api.get<Gasto>(`/gastos/${id}`);
    return response.data;
  },

  getCategorias: async (): Promise<string[]> => {
    const response = await api.get<string[]>('/gastos/categorias');
    return response.data;
  },

  create: async (data: CreateGastoDto): Promise<Gasto> => {
    const response = await api.post<Gasto>('/gastos', data);
    return response.data;
  },

  update: async (id: number, data: UpdateGastoDto): Promise<Gasto> => {
    const response = await api.put<Gasto>(`/gastos/${id}`, data);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/gastos/${id}`);
  },

  getListado: async (fechaDesde: string, fechaHasta: string): Promise<Gasto[]> => {
    const response = await api.get<Gasto[]>(`/gastos/listado?fechaDesde=${fechaDesde}&fechaHasta=${fechaHasta}`);
    return response.data;
  },

  openListadoPdf: async (fechaDesde: string, fechaHasta: string): Promise<void> => {
    await openAuthenticatedPdf(`/gastos/listado-pdf?fechaDesde=${fechaDesde}&fechaHasta=${fechaHasta}`);
  },
};
