import { useState, useEffect } from 'react';
import { referenceService } from '../services/referenceService';
import type { Transporte, Provincia, CreateTransporteDto, UpdateTransporteDto } from '../types/references';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { IconButton } from '../components/IconButton';

const TransportesPage = () => {
  const [transportes, setTransportes] = useState<Transporte[]>([]);
  const [provincias, setProvincias] = useState<Provincia[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showModal, setShowModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [transporteToDelete, setTransporteToDelete] = useState<Transporte | null>(null);
  const [editingTransporte, setEditingTransporte] = useState<Transporte | null>(null);
  const [formData, setFormData] = useState<CreateTransporteDto | UpdateTransporteDto>({
    nombre: '',
    direccion: '',
    localidad: '',
    provinciaId: undefined,
    cuit: '',
  });

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const [transportesData, provinciasData] = await Promise.all([
        referenceService.getTransportes(),
        referenceService.getProvincias(),
      ]);
      setTransportes(transportesData);
      setProvincias(provinciasData);
    } catch (err) {
      setError('Error al cargar los datos');
      console.error('Error loading data:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = () => {
    setEditingTransporte(null);
    setFormData({
      nombre: '',
      direccion: '',
      localidad: '',
      provinciaId: undefined,
      cuit: '',
    });
    setShowModal(true);
  };

  const handleEdit = (transporte: Transporte) => {
    setEditingTransporte(transporte);
    setFormData({
      nombre: transporte.nombre || '',
      direccion: transporte.direccion || '',
      localidad: transporte.localidad || '',
      provinciaId: transporte.provinciaId || undefined,
      cuit: transporte.cuit || '',
    });
    setShowModal(true);
  };

  const handleDeleteClick = (transporte: Transporte) => {
    setTransporteToDelete(transporte);
    setShowDeleteModal(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!formData.nombre.trim()) {
      setError('El nombre es obligatorio');
      return;
    }

    try {
      setLoading(true);
      if (editingTransporte) {
        await referenceService.updateTransporte(editingTransporte.id, formData as UpdateTransporteDto);
      } else {
        await referenceService.createTransporte(formData as CreateTransporteDto);
      }
      setShowModal(false);
      await loadData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al guardar el transporte');
      console.error('Error saving transporte:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteConfirm = async () => {
    if (!transporteToDelete) return;

    try {
      await referenceService.deleteTransporte(transporteToDelete.id);
      setTransportes(transportes.filter((t) => t.id !== transporteToDelete.id));
      setShowDeleteModal(false);
      setTransporteToDelete(null);
    } catch (err) {
      setError('Error al eliminar el transporte');
      console.error('Error deleting transporte:', err);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    if (name === 'provinciaId') {
      setFormData((prev) => ({ ...prev, [name]: value ? parseInt(value, 10) : undefined }));
    } else {
      setFormData((prev) => ({ ...prev, [name]: value }));
    }
  };

  return (
    <div>
      <PageHeader
        title="Gestión de Transportes"
        icon="fa-solid fa-truck"
        actions={
          <GradientButton icon="fa-solid fa-plus" onClick={handleCreate}>
            Nuevo Transporte
          </GradientButton>
        }
      />

      {error && <div className="alert alert-danger">{error}</div>}

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
                    <th>Dirección</th>
                    <th>Localidad</th>
                    <th>Provincia</th>
                    <th>CUIT</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {transportes.map((transporte) => (
                    <tr key={transporte.id}>
                      <td>{transporte.nombre}</td>
                      <td>{transporte.direccion || '-'}</td>
                      <td>{transporte.localidad || '-'}</td>
                      <td>{transporte.provinciaDescripcion || '-'}</td>
                      <td>{transporte.cuit || '-'}</td>
                      <td>
                        <IconButton
                          icon="fa-solid fa-pen"
                          title="Editar"
                          variant="primary"
                          onClick={() => handleEdit(transporte)}
                        />
                        <IconButton
                          icon="fa-solid fa-trash"
                          title="Eliminar"
                          variant="danger"
                          onClick={() => handleDeleteClick(transporte)}
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
          <div className="modal-dialog modal-lg">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">
                  {editingTransporte ? 'Editar Transporte' : 'Nuevo Transporte'}
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
                  <div className="row">
                    <div className="col-md-12 mb-3">
                      <label className="form-label">
                        Nombre <span className="text-danger">*</span>
                      </label>
                      <input
                        type="text"
                        className="form-control"
                        name="nombre"
                        value={formData.nombre}
                        onChange={handleChange}
                        placeholder="Nombre del transporte"
                        maxLength={100}
                        required
                      />
                    </div>
                  </div>
                  <div className="row">
                    <div className="col-md-12 mb-3">
                      <label className="form-label">Dirección</label>
                      <input
                        type="text"
                        className="form-control"
                        name="direccion"
                        value={formData.direccion || ''}
                        onChange={handleChange}
                        placeholder="Dirección"
                        maxLength={200}
                      />
                    </div>
                  </div>
                  <div className="row">
                    <div className="col-md-6 mb-3">
                      <label className="form-label">Localidad</label>
                      <input
                        type="text"
                        className="form-control"
                        name="localidad"
                        value={formData.localidad || ''}
                        onChange={handleChange}
                        placeholder="Localidad"
                        maxLength={100}
                      />
                    </div>
                    <div className="col-md-6 mb-3">
                      <label className="form-label">Provincia</label>
                      <select
                        className="form-select"
                        name="provinciaId"
                        value={formData.provinciaId || ''}
                        onChange={handleChange}
                      >
                        <option value="">Seleccione una provincia</option>
                        {provincias.map((provincia) => (
                          <option key={provincia.id} value={provincia.id}>
                            {provincia.descripcion}
                          </option>
                        ))}
                      </select>
                    </div>
                  </div>
                  <div className="row">
                    <div className="col-md-6 mb-3">
                      <label className="form-label">CUIT</label>
                      <input
                        type="text"
                        className="form-control"
                        name="cuit"
                        value={formData.cuit || ''}
                        onChange={handleChange}
                        placeholder="XX-XXXXXXXX-X"
                        maxLength={13}
                      />
                    </div>
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
                ¿Está seguro de que desea eliminar el transporte{' '}
                <strong>{transporteToDelete?.nombre}</strong>?
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

export default TransportesPage;
