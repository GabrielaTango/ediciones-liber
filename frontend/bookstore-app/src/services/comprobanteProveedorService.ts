import api from './api';
import type { ComprobanteProveedor, ComprobanteProveedorDetail, CreateComprobanteProveedorDto, IvaCompra } from '../types/comprobanteProveedor';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5184/api';

export const comprobanteProveedorService = {
  getAll: async (): Promise<ComprobanteProveedor[]> => {
    const response = await api.get<ComprobanteProveedor[]>('/comprobantesproveedores');
    return response.data;
  },

  getById: async (id: number): Promise<ComprobanteProveedorDetail> => {
    const response = await api.get<ComprobanteProveedorDetail>(`/comprobantesproveedores/${id}`);
    return response.data;
  },

  create: async (data: CreateComprobanteProveedorDto): Promise<ComprobanteProveedor> => {
    const response = await api.post<ComprobanteProveedor>('/comprobantesproveedores', data);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/comprobantesproveedores/${id}`);
  },

  getIvaCompras: async (fechaDesde: string, fechaHasta: string): Promise<IvaCompra[]> => {
    const response = await api.get<IvaCompra[]>('/comprobantesproveedores/iva-compras', {
      params: { fechaDesde, fechaHasta }
    });
    return response.data;
  },

  openIvaComprasPdf: (fechaDesde: string, fechaHasta: string): void => {
    const pdfUrl = `${API_BASE_URL}/comprobantesproveedores/iva-compras-pdf?fechaDesde=${fechaDesde}&fechaHasta=${fechaHasta}`;
    window.open(pdfUrl, '_blank');
  },
};
