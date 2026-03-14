export interface Articulo {
  id: number;
  descripcion?: string;
  codBarras?: string;
  observaciones?: string;
  tomos?: number;
  tema?: string;
  precio?: number;
}

export interface CreateArticuloDto {
  descripcion: string;
  codBarras?: string;
  observaciones?: string;
  tomos?: number;
  tema?: string;
  precio?: number;
}

export interface UpdateArticuloDto {
  descripcion: string;
  codBarras?: string;
  observaciones?: string;
  tomos?: number;
  tema?: string;
  precio?: number;
}
