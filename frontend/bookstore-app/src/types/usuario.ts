export interface Usuario {
  id: number;
  username: string;
  nombreCompleto: string;
  email?: string;
  rol: string;
  activo: boolean;
  fechaCreacion: string;
  ultimoAcceso?: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  usuario: Usuario;
}

export interface CreateUsuarioDto {
  username: string;
  password: string;
  nombreCompleto: string;
  email?: string;
  rol: string;
}

export interface UpdateUsuarioDto {
  nombreCompleto: string;
  email?: string;
  rol: string;
  activo: boolean;
}

export interface ChangePasswordDto {
  newPassword: string;
}
