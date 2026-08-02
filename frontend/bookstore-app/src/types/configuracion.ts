export interface AfipConfigDto {
  cuit: string;
  wsaaUrl: string;
  wsfevUrl: string;
  puntoVenta: number;
  isProduction: boolean;
  tieneCrt: boolean;
  tieneKey: boolean;
}

export interface AfipConfigUpdateDto {
  cuit: string;
  wsaaUrl: string;
  wsfevUrl: string;
  puntoVenta: number;
  isProduction: boolean;
  crtBase64?: string;
  keyBase64?: string;
}

export type CertificadoEstado = 'vigente' | 'por_vencer' | 'vencido' | 'sin_certificado' | 'error';

export interface CertificadoEstadoDto {
  tieneCertificado: boolean;
  estado: CertificadoEstado;
  fechaEmision: string | null;
  fechaVencimiento: string | null;
  diasRestantes: number | null;
  subject: string | null;
  mensaje: string | null;
}

export interface UltimoComprobanteDto {
  puntoVenta: number;
  tipoComprobante: number;
  tipoComprobanteDescripcion: string;
  ultimoNumero: number;
}
