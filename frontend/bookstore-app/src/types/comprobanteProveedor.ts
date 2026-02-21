export interface ComprobanteProveedor {
  id: number;
  proveedor_Id: number;
  proveedorNombre: string;
  tipoComprobante: string;
  fechaEmision: string;
  nroComprobante: string;
  importeTotal: number;
  cantidadCuotas: number;
  fechaPrimerVencimiento: string;
}

export interface CreateComprobanteProveedorDto {
  proveedor_Id: number;
  tipoComprobante: string;
  fechaEmision: string;
  nroComprobante: string;
  importeTotal: number;
  cantidadCuotas: number;
  fechaPrimerVencimiento: string;
}

export interface CuotaPreview {
  numeroCuota: number;
  fechaVencimiento: string;
  importe: number;
}

export interface CuotaProveedorDto {
  id: number;
  numeroCuota: number;
  fechaVencimiento: string;
  importe: number;
  importePagado: number;
  estado: string;
}

export interface ComprobanteProveedorDetail {
  id: number;
  proveedor_Id: number;
  proveedorNombre: string;
  tipoComprobante: string;
  fechaEmision: string;
  nroComprobante: string;
  importeTotal: number;
  cantidadCuotas: number;
  fechaPrimerVencimiento: string;
  cuotas: CuotaProveedorDto[];
}

export interface IvaCompra {
  fecha: string;
  tipoComprobante: string;
  numeroComprobante: string;
  nombre: string;
  cuit: string;
  total: number;
}
