import { useState, useEffect, useMemo } from 'react';
import { referenceService } from '../services/referenceService';
import type { Vendedor, CreateVendedorDto, UpdateVendedorDto } from '../types/references';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { IconButton } from '../components/IconButton';

const VendedoresPage = () => {
  const [vendedores, setVendedores] = useState<Vendedor[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filtro, setFiltro] = useState('');

  const vendedoresFiltrados = useMemo(() => {
    if (!filtro.trim()) return vendedores;
    const busqueda = filtro.toLowerCase();
    return vendedores.filter(v =>
      (v.descripcion?.toLowerCase() || '').includes(busqueda)
    );
  }, [vendedores, filtro]);
  const [showModal, setShowModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [vendedorToDelete, setVendedorToDelete] = useState<Vendedor | null>(null);
  const [editingVendedor, setEditingVendedor] = useState<Vendedor | null>(null);
  const [formData, setFormData] = useState<CreateVendedorDto | UpdateVendedorDto>({
    descripcion: '',
  });

  useEffect(() => {
    loadVendedores();
  }, []);

  const loadVendedores = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await referenceService.getVendedores();
      setVendedores(data);
    } catch (err) {
      setError('Error al cargar los vendedores');
      console.error('Error loading vendedores:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = () => {
    setEditingVendedor(null);
    setFormData({ descripcion: '' });
    setShowModal(true);
  };

  const handleEdit = (vendedor: Vendedor) => {
    setEditingVendedor(vendedor);
    setFormData({
      descripcion: vendedor.descripcion || '',
    });
    setShowModal(true);
  };

  const handleDeleteClick = (vendedor: Vendedor) => {
    setVendedorToDelete(vendedor);
    setShowDeleteModal(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!formData.descripcion.trim()) {
      setError('La descripción es obligatoria');
      return;
    }

    try {
      setLoading(true);
      if (editingVendedor) {
        await referenceService.updateVendedor(editingVendedor.id, formData as UpdateVendedorDto);
      } else {
        await referenceService.createVendedor(formData as CreateVendedorDto);
      }
      setShowModal(false);
      await loadVendedores();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al guardar el vendedor');
      console.error('Error saving vendedor:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteConfirm = async () => {
    if (!vendedorToDelete) return;

    try {
      await referenceService.deleteVendedor(vendedorToDelete.id);
      setVendedores(vendedores.filter((v) => v.id !== vendedorToDelete.id));
      setShowDeleteModal(false);
      setVendedorToDelete(null);
    } catch (err) {
      setError('Error al eliminar el vendedor');
      console.error('Error deleting vendedor:', err);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  return (
    <div>
      <PageHeader
        title="Gestión de Vendedores"
        icon="fa-solid fa-user-tie"
        actions={
          <GradientButton icon="fa-solid fa-plus" onClick={handleCreate}>
            Nuevo Vendedor
          </GradientButton>
        }
      />

      {error && <div className="alert alert-danger">{error}</div>}

      {/* Filtro */}
      <div className="card mb-3">
        <div className="card-body py-2">
          <div className="row">
            <div className="col-md-4">
              <input
                type="text"
                className="form-control"
                placeholder="Buscar por nombre..."
                value={filtro}
                onChange={(e) => setFiltro(e.target.value)}
              />
            </div>
          </div>
        </div>
      </div>

      {loading ? (
        <div className="text-center py-5">
          <div className="spinner-gradient" />
        </div>
      ) : (
        <div className="card">
          <div className="card-body">
            <div className="table-responsive">
              <table className="custom-table">
                <thead>
                  <tr>
                    <th>Nombre</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {vendedoresFiltrados.map((vendedor) => (
                    <tr key={vendedor.id}>
                      <td>{vendedor.descripcion || '-'}</td>
                      <td>
                        <IconButton
                          icon="fa-solid fa-pen"
                          title="Editar"
                          variant="primary"
                          onClick={() => handleEdit(vendedor)}
                        />
                        <IconButton
                          icon="fa-solid fa-trash"
                          title="Eliminar"
                          variant="danger"
                          onClick={() => handleDeleteClick(vendedor)}
                        />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* Modal para crear/editar */}
      {showModal && (
        <div className="modal show d-block" tabIndex={-1} style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">
                  {editingVendedor ? 'Editar Vendedor' : 'Nuevo Vendedor'}
                </h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setShowModal(false)}
                  aria-label="Close"
                ></button>
              </div>
              <form onSubmit={handleSubmit}>
                <div className="modal-body">
                  <div className="mb-3">
                    <label className="form-label">
                      Nombre <span className="text-danger">*</span>
                    </label>
                    <input
                      type="text"
                      className="form-control"
                      name="descripcion"
                      value={formData.descripcion}
                      onChange={handleChange}
                      placeholder="Nombre del vendedor"
                      maxLength={100}
                      required
                    />
                  </div>
                </div>
                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => setShowModal(false)}
                  >
                    Cancelar
                  </button>
                  <button type="submit" className="btn btn-primary" disabled={loading}>
                    {loading ? 'Guardando...' : 'Guardar'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Modal para eliminar */}
      {showDeleteModal && (
        <div className="modal show d-block" tabIndex={-1} style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Confirmar Eliminación</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setShowDeleteModal(false)}
                  aria-label="Close"
                ></button>
              </div>
              <div className="modal-body">
                ¿Está seguro de que desea eliminar el vendedor{' '}
                <strong>{vendedorToDelete?.descripcion}</strong>?
              </div>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setShowDeleteModal(false)}
                >
                  Cancelar
                </button>
                <button
                  type="button"
                  className="btn btn-danger"
                  onClick={handleDeleteConfirm}
                >
                  Eliminar
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default VendedoresPage;
