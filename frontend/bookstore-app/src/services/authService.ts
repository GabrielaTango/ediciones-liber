import api from './api';
import type { LoginRequest, LoginResponse, Usuario, CreateUsuarioDto, UpdateUsuarioDto, ChangePasswordDto } from '../types/usuario';

export const authService = {
  login: async (data: LoginRequest): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>('/auth/login', data);
    return response.data;
  },

  getCurrentUser: async (): Promise<Usuario> => {
    const response = await api.get<Usuario>('/auth/me');
    return response.data;
  },

  // ABM Usuarios
  getAllUsuarios: async (): Promise<Usuario[]> => {
    const response = await api.get<Usuario[]>('/usuarios');
    return response.data;
  },

  getUsuarioById: async (id: number): Promise<Usuario> => {
    const response = await api.get<Usuario>(`/usuarios/${id}`);
    return response.data;
  },

  createUsuario: async (data: CreateUsuarioDto): Promise<Usuario> => {
    const response = await api.post<Usuario>('/usuarios', data);
    return response.data;
  },

  updateUsuario: async (id: number, data: UpdateUsuarioDto): Promise<Usuario> => {
    const response = await api.put<Usuario>(`/usuarios/${id}`, data);
    return response.data;
  },

  changePassword: async (id: number, data: ChangePasswordDto): Promise<void> => {
    await api.put(`/usuarios/${id}/password`, data);
  },

  deleteUsuario: async (id: number): Promise<void> => {
    await api.delete(`/usuarios/${id}`);
  },
};
