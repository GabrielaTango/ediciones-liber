export interface Proveedor {
  id: number;
  codigo?: string;
  nombre: string;
  razonSocial?: string;
  cuit?: string;
  domicilio?: string;
  telefono?: string;
  mail?: string;
  fechaInhabilitacion?: string;
}

export interface CreateProveedorDto {
  nombre: string;
  razonSocial?: string;
  cuit?: string;
  domicilio?: string;
  telefono?: string;
  mail?: string;
}

export interface UpdateProveedorDto {
  nombre: string;
  razonSocial?: string;
  cuit?: string;
  domicilio?: string;
  telefono?: string;
  mail?: string;
  fechaInhabilitacion?: string;
}
