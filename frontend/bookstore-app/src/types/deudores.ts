export interface CuotaDeudor {
  periodo: string; // formato MM/YYYY o "Otras"
  saldo: number;   // sumatoria de saldos de cuotas en ese período
  importePagado: number; // sumatoria de importes pagados en ese período
}

export interface DeudorItem {
  comprobanteId: number;
  numeroComprobante: string;
  razonSocial: string;
  codigoVendedor?: string;
  cantidadCuotas: number;
  totalComprobante: number;
  saldo: number;
  anticipo: number;
  contraEntrega: number;
  contraEntregaPagado: number;
  cuotas: CuotaDeudor[];
}

export interface DeudoresReporte {
  mes: number;
  anio: number;
  periodosCuotas: string[]; // Lista de períodos únicos para columnas
  deudores: DeudorItem[];
}
