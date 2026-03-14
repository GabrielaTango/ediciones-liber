export interface Cliente {
  id: number;
  codigo?: string;
  nombre: string;
  zona_Id?: number;
  subZona_Id?: number;
  vendedor_Id?: number;
  domicilioComercial?: string;
  domicilioParticular?: string;
  provincia_Id?: number;
  codigoPostal?: string;
  fechaAlta: string;
  fechaInha?: string;
  soloContado: boolean;
  telefono?: string;
  telefonoMovil?: string;
  contacto?: string;
  tipoDocumento?: string;
  nroDocumento?: string;
  nroIIBB?: string;
  categoriaIva?: string;
  condicionPago?: string;
  descuento: number;
  observaciones?: string;
  tipoDocArca?: string;
}

export interface CreateClienteDto {
  codigo?: string;
  nombre: string;
  zona_Id?: number;
  subZona_Id?: number;
  vendedor_Id?: number;
  domicilioComercial?: string;
  domicilioParticular?: string;
  provincia_Id?: number;
  codigoPostal?: string;
  soloContado: boolean;
  telefono?: string;
  telefonoMovil?: string;
  contacto?: string;
  tipoDocumento?: string;
  nroDocumento?: string;
  nroIIBB?: string;
  categoriaIva?: string;
  condicionPago?: string;
  descuento: number;
  observaciones?: string;
  tipoDocArca?: string;
}

export interface UpdateClienteDto {
  codigo?: string;
  nombre: string;
  zona_Id?: number;
  subZona_Id?: number;
  vendedor_Id?: number;
  domicilioComercial?: string;
  domicilioParticular?: string;
  provincia_Id?: number;
  codigoPostal?: string;
  fechaInha?: string;
  soloContado: boolean;
  telefono?: string;
  telefonoMovil?: string;
  contacto?: string;
  tipoDocumento?: string;
  nroDocumento?: string;
  nroIIBB?: string;
  categoriaIva?: string;
  condicionPago?: string;
  descuento: number;
  observaciones?: string;
  tipoDocArca?: string;
}
