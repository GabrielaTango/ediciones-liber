import api, { openAuthenticatedPdf } from './api';
import type { ComprobanteProveedor, ComprobanteProveedorDetail, CreateComprobanteProveedorDto, IvaCompra } from '../types/comprobanteProveedor';

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

  openIvaComprasPdf: async (fechaDesde: string, fechaHasta: string): Promise<void> => {
    await openAuthenticatedPdf(`/comprobantesproveedores/iva-compras-pdf?fechaDesde=${fechaDesde}&fechaHasta=${fechaHasta}`);
  },
};
