// Zona
export interface Zona {
  id: number;
  descripcion?: string;
}

export interface CreateZonaDto {
  descripcion: string;
}

export interface UpdateZonaDto {
  descripcion: string;
}

// SubZona
export interface SubZona {
  id: number;
  descripcion?: string;
  zonaId?: number;
  provinciaId: number;
  codigoPostal?: string;
  localidad?: string;
  provinciaDescripcion?: string;
}

export interface CreateSubZonaDto {
  descripcion: string;
  zonaId?: number;
  provinciaId: number;
  codigoPostal: string;
  localidad: string;
}

export interface UpdateSubZonaDto {
  descripcion: string;
  zonaId?: number;
  provinciaId: number;
  codigoPostal: string;
  localidad: string;
}

// Provincia
export interface Provincia {
  id: number;
  descripcion?: string;
}

export interface CreateProvinciaDto {
  descripcion: string;
}

export interface UpdateProvinciaDto {
  descripcion: string;
}

// Vendedor
export interface Vendedor {
  id: number;
  descripcion?: string;
}

export interface CreateVendedorDto {
  descripcion: string;
}

export interface UpdateVendedorDto {
  descripcion: string;
}

// Transporte
export interface Transporte {
  id: number;
  nombre: string;
  direccion?: string;
  localidad?: string;
  provinciaId?: number;
  provinciaDescripcion?: string;
  cuit?: string;
}

export interface CreateTransporteDto {
  nombre: string;
  direccion?: string;
  localidad?: string;
  provinciaId?: number;
  cuit?: string;
}

export interface UpdateTransporteDto {
  nombre: string;
  direccion?: string;
  localidad?: string;
  provinciaId?: number;
  cuit?: string;
}
