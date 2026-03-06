export interface PagoCuotaDto {
  id: number;
  nroReferencia: string;
  fecha: string;
  importe: number;
}

export interface CuotaListado {
  id: number;
  comprobanteId: number;
  numeroComprobante?: string;
  fechaComprobante?: string;
  clienteId: number;
  clienteNombre?: string;
  zonaId?: number;
  zonaNombre?: string;
  fechaCuota?: string;
  importe: number;
  importePagado: number;
  estado?: string;
  numeroCuota: number; // 0 = contraentrega, 1+ = cuotas regulares
  esCuotaCero: boolean; // Calculado en el backend: numeroCuota === 0
  pagos: PagoCuotaDto[];
}

export interface CreatePagoCuotaDto {
  nroReferencia: string;
  fecha: string;
  importe: number;
}

export interface CreatePagoComprobanteDto {
  nroReferencia: string;
  importe: number;
  fecha?: string;
}
