import api from './api';
import type { Proveedor, CreateProveedorDto, UpdateProveedorDto } from '../types/proveedor';

export const proveedorService = {
  getAll: async (): Promise<Proveedor[]> => {
    const response = await api.get<Proveedor[]>('/proveedores');
    return response.data;
  },

  getById: async (id: number): Promise<Proveedor> => {
    const response = await api.get<Proveedor>(`/proveedores/${id}`);
    return response.data;
  },

  create: async (data: CreateProveedorDto): Promise<Proveedor> => {
    const response = await api.post<Proveedor>('/proveedores', data);
    return response.data;
  },

  update: async (id: number, data: UpdateProveedorDto): Promise<Proveedor> => {
    const response = await api.put<Proveedor>(`/proveedores/${id}`, data);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/proveedores/${id}`);
  },
};
