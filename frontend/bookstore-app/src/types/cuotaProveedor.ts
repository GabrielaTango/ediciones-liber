export interface PagoCuotaProveedorDto {
  id: number;
  nroReferencia: string;
  fecha: string;
  importe: number;
}

export interface CuotaProveedorListado {
  id: number;
  comprobanteProveedorId: number;
  nroComprobante: string;
  tipoComprobante: string;
  fechaEmision: string;
  proveedorId: number;
  proveedorNombre: string;
  numeroCuota: number;
  fechaVencimiento: string;
  importe: number;
  importePagado: number;
  estado: string;
  pagos: PagoCuotaProveedorDto[];
}

export interface CuotaProveedorFiltros {
  proveedorId?: number;
  fechaDesde?: string;
  fechaHasta?: string;
  estado?: string;
}

export interface CreatePagoCuotaProveedorDto {
  nroReferencia: string;
  fecha: string;
  importe: number;
}
