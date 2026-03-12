import api, { openAuthenticatedPdf } from './api';
import type { Comprobante, CreateComprobanteDto, UpdateComprobanteDto, ArticulosVendidosZonaReporte } from '../types/comprobante';
import type { IvaVenta } from '../types/dashboard';
import type { DeudoresReporte } from '../types/deudores';

export interface ComprobanteFilters {
  zonaId?: number;
  clienteId?: number;
  tipoComprobante?: string;
  fechaDesde?: string;
  fechaHasta?: string;
  vendedorId?: number;
  comprobante?: string;
}

export const comprobanteService = {
  getAll: async (filters?: ComprobanteFilters): Promise<Comprobante[]> => {
    const params = new URLSearchParams();
    if (filters?.zonaId) params.append('zonaId', filters.zonaId.toString());
    if (filters?.clienteId) params.append('clienteId', filters.clienteId.toString());
    if (filters?.tipoComprobante) params.append('tipoComprobante', filters.tipoComprobante);
    if (filters?.fechaDesde) params.append('fechaDesde', filters.fechaDesde);
    if (filters?.fechaHasta) params.append('fechaHasta', filters.fechaHasta);
    if (filters?.vendedorId) params.append('vendedorId', filters.vendedorId.toString());
    if (filters?.comprobante) params.append('comprobante', filters.comprobante);

    const queryString = params.toString();
    const url = queryString ? `/comprobantes?${queryString}` : '/comprobantes';
    const response = await api.get<Comprobante[]>(url);
    return response.data;
  },

  getById: async (id: number): Promise<Comprobante> => {
    const response = await api.get<Comprobante>(`/comprobantes/${id}`);
    return response.data;
  },

  create: async (data: CreateComprobanteDto): Promise<Comprobante> => {
    const response = await api.post<Comprobante>('/comprobantes', data);
    return response.data;
  },

  update: async (id: number, data: UpdateComprobanteDto): Promise<Comprobante> => {
    const response = await api.put<Comprobante>(`/comprobantes/${id}`, data);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/comprobantes/${id}`);
  },

  cancelar: async (id: number): Promise<Comprobante> => {
    const response = await api.post<Comprobante>(`/comprobantes/${id}/cancelar`);
    return response.data;
  },

  cancelarDeuda: async (id: number): Promise<void> => {
    await api.post(`/comprobantes/${id}/cancelar-deuda`);
  },

  openPdf: async (id: number): Promise<void> => {
    await openAuthenticatedPdf(`/comprobantes/${id}/pdf`);
  },

  openCuponesPdf: async (id: number): Promise<void> => {
    await openAuthenticatedPdf(`/comprobantes/${id}/cupones-pdf`);
  },

  openCompletoPdf: async (id: number): Promise<void> => {
    await openAuthenticatedPdf(`/comprobantes/${id}/completo-pdf`);
  },

  openBatchPdf: async (filters: ComprobanteFilters): Promise<void> => {
    const params = new URLSearchParams();
    if (filters.zonaId) params.append('zonaId', filters.zonaId.toString());
    if (filters.clienteId) params.append('clienteId', filters.clienteId.toString());
    if (filters.tipoComprobante) params.append('tipoComprobante', filters.tipoComprobante);
    if (filters.fechaDesde) params.append('fechaDesde', filters.fechaDesde);
    if (filters.fechaHasta) params.append('fechaHasta', filters.fechaHasta);
    if (filters.vendedorId) params.append('vendedorId', filters.vendedorId.toString());
    if (filters.comprobante) params.append('comprobante', filters.comprobante);

    await openAuthenticatedPdf(`/comprobantes/batch-pdf?${params.toString()}`);
  },

  openBatchCuponesPdf: async (filters: ComprobanteFilters): Promise<void> => {
    const params = new URLSearchParams();
    if (filters.zonaId) params.append('zonaId', filters.zonaId.toString());
    if (filters.clienteId) params.append('clienteId', filters.clienteId.toString());
    if (filters.tipoComprobante) params.append('tipoComprobante', filters.tipoComprobante);
    if (filters.fechaDesde) params.append('fechaDesde', filters.fechaDesde);
    if (filters.fechaHasta) params.append('fechaHasta', filters.fechaHasta);
    if (filters.vendedorId) params.append('vendedorId', filters.vendedorId.toString());
    if (filters.comprobante) params.append('comprobante', filters.comprobante);

    await openAuthenticatedPdf(`/comprobantes/batch-cupones-pdf?${params.toString()}`);
  },

  getUltimoGastoEnvio: async (): Promise<number> => {
    const response = await api.get<{ gastoEnvio: number }>('/comprobantes/ultimo-gasto-envio');
    return response.data.gastoEnvio;
  },

  getIvaVentas: async (fechaDesde: string, fechaHasta: string): Promise<IvaVenta[]> => {
    const response = await api.get<IvaVenta[]>(`/comprobantes/iva-ventas?fechaDesde=${fechaDesde}&fechaHasta=${fechaHasta}`);
    return response.data;
  },

  openIvaVentasPdf: async (fechaDesde: string, fechaHasta: string): Promise<void> => {
    await openAuthenticatedPdf(`/comprobantes/iva-ventas-pdf?fechaDesde=${fechaDesde}&fechaHasta=${fechaHasta}`);
  },

  getDeudores: async (mes: number, anio: number, zonaId?: number, vendedorId?: number): Promise<DeudoresReporte> => {
    const params = new URLSearchParams();
    params.append('mes', mes.toString());
    params.append('anio', anio.toString());
    if (zonaId) params.append('zonaId', zonaId.toString());
    if (vendedorId) params.append('vendedorId', vendedorId.toString());

    const response = await api.get<DeudoresReporte>(`/comprobantes/deudores?${params.toString()}`);
    return response.data;
  },

  openDeudoresPdf: async (mes: number, anio: number, zonaId?: number, vendedorId?: number): Promise<void> => {
    const params = new URLSearchParams();
    params.append('mes', mes.toString());
    params.append('anio', anio.toString());
    if (zonaId) params.append('zonaId', zonaId.toString());
    if (vendedorId) params.append('vendedorId', vendedorId.toString());
    await openAuthenticatedPdf(`/comprobantes/deudores-pdf?${params.toString()}`);
  },

  getArticulosVendidosZona: async (zonaId?: number): Promise<ArticulosVendidosZonaReporte> => {
    const params = new URLSearchParams();
    if (zonaId) params.append('zonaId', zonaId.toString());

    const queryString = params.toString();
    const url = queryString ? `/comprobantes/articulos-vendidos-zona?${queryString}` : '/comprobantes/articulos-vendidos-zona';
    const response = await api.get<ArticulosVendidosZonaReporte>(url);
    return response.data;
  },

  openArticulosVendidosZonaPdf: async (zonaId?: number): Promise<void> => {
    const params = new URLSearchParams();
    if (zonaId) params.append('zonaId', zonaId.toString());

    const queryString = params.toString();
    const url = queryString
      ? `/comprobantes/articulos-vendidos-zona-pdf?${queryString}`
      : `/comprobantes/articulos-vendidos-zona-pdf`;
    await openAuthenticatedPdf(url);
  },
};
