export interface Gasto {
  id: number;
  nroComprobante: string;
  importe: number;
  categoria: string;
  descripcion?: string;
  fecha: string;
}

export interface CreateGastoDto {
  nroComprobante: string;
  importe: number;
  categoria: string;
  descripcion?: string;
  fecha: string;
}

export interface UpdateGastoDto {
  nroComprobante: string;
  importe: number;
  categoria: string;
  descripcion?: string;
  fecha: string;
}
