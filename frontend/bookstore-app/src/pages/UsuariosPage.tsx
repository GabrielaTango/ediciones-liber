import { useState, useEffect } from 'react';
import type { FormEvent } from 'react';
import { Icon } from '../components/Icon';
import { authService } from '../services/authService';
import type { Usuario, CreateUsuarioDto, UpdateUsuarioDto } from '../types/usuario';
import Swal from 'sweetalert2';

const UsuariosPage = () => {
  const [usuarios, setUsuarios] = useState<Usuario[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [showPasswordModal, setShowPasswordModal] = useState(false);
  const [editingUsuario, setEditingUsuario] = useState<Usuario | null>(null);
  const [passwordUsuarioId, setPasswordUsuarioId] = useState<number | null>(null);
  const [newPassword, setNewPassword] = useState('');

  // Form fields
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [nombreCompleto, setNombreCompleto] = useState('');
  const [email, setEmail] = useState('');
  const [rol, setRol] = useState('usuario');
  const [activo, setActivo] = useState(true);

  const loadUsuarios = async () => {
    try {
      setLoading(true);
      const data = await authService.getAllUsuarios();
      setUsuarios(data);
    } catch (error) {
      console.error('Error al cargar usuarios:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadUsuarios();
  }, []);

  const resetForm = () => {
    setUsername('');
    setPassword('');
    setNombreCompleto('');
    setEmail('');
    setRol('usuario');
    setActivo(true);
    setEditingUsuario(null);
  };

  const openCreateModal = () => {
    resetForm();
    setShowModal(true);
  };

  const openEditModal = (usuario: Usuario) => {
    setEditingUsuario(usuario);
    setNombreCompleto(usuario.nombreCompleto);
    setEmail(usuario.email || '');
    setRol(usuario.rol);
    setActivo(usuario.activo);
    setShowModal(true);
  };

  const openPasswordModal = (id: number) => {
    setPasswordUsuarioId(id);
    setNewPassword('');
    setShowPasswordModal(true);
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    try {
      if (editingUsuario) {
        const updateData: UpdateUsuarioDto = { nombreCompleto, email: email || undefined, rol, activo };
        await authService.updateUsuario(editingUsuario.id, updateData);
        Swal.fire('Actualizado', 'Usuario actualizado correctamente', 'success');
      } else {
        const createData: CreateUsuarioDto = { username, password, nombreCompleto, email: email || undefined, rol };
        await authService.createUsuario(createData);
        Swal.fire('Creado', 'Usuario creado correctamente', 'success');
      }
      setShowModal(false);
      loadUsuarios();
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } } };
      const msg = err.response?.data?.message || 'Error al guardar el usuario';
      Swal.fire('Error', msg, 'error');
    }
  };

  const handleChangePassword = async (e: FormEvent) => {
    e.preventDefault();
    if (!passwordUsuarioId) return;
    try {
      await authService.changePassword(passwordUsuarioId, { newPassword });
      Swal.fire('Actualizado', 'Contraseña actualizada correctamente', 'success');
      setShowPasswordModal(false);
    } catch {
      Swal.fire('Error', 'Error al cambiar la contraseña', 'error');
    }
  };

  const handleDelete = async (id: number, name: string) => {
    const result = await Swal.fire({
      title: '¿Eliminar usuario?',
      text: `Se eliminará el usuario "${name}"`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Eliminar',
      cancelButtonText: 'Cancelar',
    });

    if (result.isConfirmed) {
      try {
        await authService.deleteUsuario(id);
        Swal.fire('Eliminado', 'Usuario eliminado correctamente', 'success');
        loadUsuarios();
      } catch {
        Swal.fire('Error', 'Error al eliminar el usuario', 'error');
      }
    }
  };

  return (
    <>
      {/* Page Header */}
      <div className="page-header">
        <h1>
          <div className="icon-circle">
            <Icon name="fa-solid fa-users-gear" />
          </div>
          Usuarios
        </h1>
        <button className="btn-gradient" onClick={openCreateModal}>
          <Icon name="fa-solid fa-plus" />
          <span>Nuevo Usuario</span>
        </button>
      </div>

      {/* Table */}
      <div className="card p-4">
        {loading ? (
          <div className="text-center py-5">
            <div className="spinner-gradient mx-auto" />
          </div>
        ) : (
          <div className="table-responsive">
            <table className="table custom-table">
              <thead>
                <tr>
                  <th>Usuario</th>
                  <th>Nombre Completo</th>
                  <th>Email</th>
                  <th>Rol</th>
                  <th>Estado</th>
                  <th>Último Acceso</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {usuarios.map((u) => (
                  <tr key={u.id}>
                    <td><strong>{u.username}</strong></td>
                    <td>{u.nombreCompleto}</td>
                    <td>{u.email || '-'}</td>
                    <td>
                      <span className={`badge ${u.rol === 'admin' ? 'badge-primary' : 'badge-info'}`}>
                        {u.rol === 'admin' ? 'Administrador' : 'Usuario'}
                      </span>
                    </td>
                    <td>
                      <span className={`badge ${u.activo ? 'badge-success' : 'badge-danger'}`}>
                        {u.activo ? 'Activo' : 'Inactivo'}
                      </span>
                    </td>
                    <td>
                      {u.ultimoAcceso
                        ? new Date(u.ultimoAcceso).toLocaleString('es-AR')
                        : 'Nunca'}
                    </td>
                    <td>
                      <div className="d-flex gap-2">
                        <button className="btn-icon" title="Editar" onClick={() => openEditModal(u)}>
                          <Icon name="fa-solid fa-pen" />
                        </button>
                        <button className="btn-icon" title="Cambiar contraseña" onClick={() => openPasswordModal(u.id)}>
                          <Icon name="fa-solid fa-key" />
                        </button>
                        <button className="btn-icon" title="Eliminar" onClick={() => handleDelete(u.id, u.username)}
                          style={{ color: 'var(--danger-color)' }}>
                          <Icon name="fa-solid fa-trash" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
                {usuarios.length === 0 && (
                  <tr>
                    <td colSpan={7} className="text-center py-4 text-muted">
                      No hay usuarios registrados
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Create/Edit Modal */}
      {showModal && (
        <div className="modal show d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">
                  {editingUsuario ? 'Editar Usuario' : 'Nuevo Usuario'}
                </h5>
                <button className="btn-close" onClick={() => setShowModal(false)} />
              </div>
              <form onSubmit={handleSubmit}>
                <div className="modal-body">
                  {!editingUsuario && (
                    <>
                      <div className="form-group">
                        <label>Usuario <span className="required">*</span></label>
                        <input
                          type="text"
                          className="form-control"
                          value={username}
                          onChange={(e) => setUsername(e.target.value)}
                          required
                          minLength={3}
                          maxLength={50}
                        />
                      </div>
                      <div className="form-group">
                        <label>Contraseña <span className="required">*</span></label>
                        <input
                          type="password"
                          className="form-control"
                          value={password}
                          onChange={(e) => setPassword(e.target.value)}
                          required
                          minLength={6}
                        />
                      </div>
                    </>
                  )}
                  <div className="form-group">
                    <label>Nombre Completo <span className="required">*</span></label>
                    <input
                      type="text"
                      className="form-control"
                      value={nombreCompleto}
                      onChange={(e) => setNombreCompleto(e.target.value)}
                      required
                    />
                  </div>
                  <div className="form-group">
                    <label>Email</label>
                    <input
                      type="email"
                      className="form-control"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                    />
                  </div>
                  <div className="form-group">
                    <label>Rol <span className="required">*</span></label>
                    <select
                      className="form-select"
                      value={rol}
                      onChange={(e) => setRol(e.target.value)}
                      required
                    >
                      <option value="usuario">Usuario</option>
                      <option value="admin">Administrador</option>
                    </select>
                  </div>
                  {editingUsuario && (
                    <div className="form-group">
                      <div className="form-check">
                        <input
                          type="checkbox"
                          className="form-check-input"
                          id="activo"
                          checked={activo}
                          onChange={(e) => setActivo(e.target.checked)}
                        />
                        <label className="form-check-label" htmlFor="activo">
                          Usuario activo
                        </label>
                      </div>
                    </div>
                  )}
                </div>
                <div className="modal-footer">
                  <button type="button" className="btn-secondary-action" onClick={() => setShowModal(false)}>
                    Cancelar
                  </button>
                  <button type="submit" className="btn-gradient">
                    {editingUsuario ? 'Guardar Cambios' : 'Crear Usuario'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Change Password Modal */}
      {showPasswordModal && (
        <div className="modal show d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Cambiar Contraseña</h5>
                <button className="btn-close" onClick={() => setShowPasswordModal(false)} />
              </div>
              <form onSubmit={handleChangePassword}>
                <div className="modal-body">
                  <div className="form-group">
                    <label>Nueva Contraseña <span className="required">*</span></label>
                    <input
                      type="password"
                      className="form-control"
                      value={newPassword}
                      onChange={(e) => setNewPassword(e.target.value)}
                      required
                      minLength={6}
                    />
                  </div>
                </div>
                <div className="modal-footer">
                  <button type="button" className="btn-secondary-action" onClick={() => setShowPasswordModal(false)}>
                    Cancelar
                  </button>
                  <button type="submit" className="btn-gradient">
                    Cambiar Contraseña
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </>
  );
};

export default UsuariosPage;
