export interface Comprobante {
  id: number;
  cliente_Id: number;
  clienteNombre?: string;
  fecha: string;
  tipoComprobante?: string;
  numeroComprobante?: string;
  total: number;
  cae?: string;
  vto?: string;
  bonificacion?: number;
  porcentajeBonif?: number;
  anticipo?: number;
  contraEntrega?: number;
  cuotas?: number;
  valorCuota?: number;
  vendedor_Id?: number;
  vendedorNombre?: string;
  gastosEnvio?: number;
  esElectronica: boolean;
  esPresupuesto: boolean;

  // Estado del comprobante: PEN, PAG, CAN
  estado?: string;

  // Relación con comprobante asociado
  comprobanteAsociado_Id?: number;
  comprobanteAsociadoNumero?: string;  // Para NC: número de la factura

  // Para Facturas: indica si fue cancelada por una NC
  estaCancelado: boolean;
  notaCreditoNumero?: string;  // Número de la NC que canceló esta factura

  detalles: ComprobanteDetalle[];
}

export interface ComprobanteDetalle {
  id?: number;
  articulo_Id: number;
  articuloCodigo?: string;
  articuloDescripcion?: string;
  cantidad: number;
  precio_Unitario: number;
  subtotal: number;
}

export interface CreateComprobanteDto {
  cliente_Id: number;
  fecha: string;
  tipoComprobante?: string;
  numeroComprobante?: string;
  total: number;
  cae?: string;
  vto?: string;
  bonificacion?: number;
  porcentajeBonif?: number;
  anticipo?: number;
  contraEntrega?: number;
  cuotas?: number;
  valorCuota?: number;
  vendedor_Id?: number;
  gastosEnvio?: number;
  esElectronica?: boolean;
  esPresupuesto?: boolean;
  detalles: ComprobanteDetalleDto[];
}

export interface UpdateComprobanteDto {
  cliente_Id: number;
  fecha: string;
  tipoComprobante?: string;
  numeroComprobante?: string;
  total: number;
  cae?: string;
  vto?: string;
  bonificacion?: number;
  porcentajeBonif?: number;
  anticipo?: number;
  contraEntrega?: number;
  cuotas?: number;
  valorCuota?: number;
  vendedor_Id?: number;
  gastosEnvio?: number;
  esElectronica?: boolean;
  esPresupuesto?: boolean;
  detalles: ComprobanteDetalleDto[];
}

export interface ComprobanteDetalleDto {
  articulo_Id: number;
  cantidad: number;
  precio_Unitario: number;
  subtotal: number;
}

export interface ArticuloVendidoZonaItem {
  vendedorInicial: string;
  codigoCliente: string;
  razonSocial: string;
  direccion: string;
  direccionComercial: string;
  descripcionArticulo: string;
  fechaFactura: string;
  numeroFactura: string;
}

export interface ArticulosVendidosZonaReporte {
  zonaId?: number;
  zonaNombre: string;
  items: ArticuloVendidoZonaItem[];
}
