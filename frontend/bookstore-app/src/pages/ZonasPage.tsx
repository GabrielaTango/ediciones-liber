import { useState, useEffect, useMemo } from 'react';
import { referenceService } from '../services/referenceService';
import type { Zona, CreateZonaDto, UpdateZonaDto } from '../types/references';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { IconButton } from '../components/IconButton';

const ZonasPage = () => {
  const [zonas, setZonas] = useState<Zona[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filtro, setFiltro] = useState('');

  const zonasFiltradas = useMemo(() => {
    if (!filtro.trim()) return zonas;
    const busqueda = filtro.toLowerCase();
    return zonas.filter(z =>
      (z.descripcion?.toLowerCase() || '').includes(busqueda)
    );
  }, [zonas, filtro]);
  const [showModal, setShowModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [zonaToDelete, setZonaToDelete] = useState<Zona | null>(null);
  const [editingZona, setEditingZona] = useState<Zona | null>(null);
  const [formData, setFormData] = useState<CreateZonaDto | UpdateZonaDto>({
    descripcion: '',
  });

  useEffect(() => {
    loadZonas();
  }, []);

  const loadZonas = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await referenceService.getZonas();
      setZonas(data);
    } catch (err) {
      setError('Error al cargar las zonas');
      console.error('Error loading zonas:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = () => {
    setEditingZona(null);
    setFormData({ descripcion: '' });
    setShowModal(true);
  };

  const handleEdit = (zona: Zona) => {
    setEditingZona(zona);
    setFormData({
      descripcion: zona.descripcion || '',
    });
    setShowModal(true);
  };

  const handleDeleteClick = (zona: Zona) => {
    setZonaToDelete(zona);
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
      if (editingZona) {
        await referenceService.updateZona(editingZona.id, formData as UpdateZonaDto);
      } else {
        await referenceService.createZona(formData as CreateZonaDto);
      }
      setShowModal(false);
      await loadZonas();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al guardar la zona');
      console.error('Error saving zona:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteConfirm = async () => {
    if (!zonaToDelete) return;

    try {
      await referenceService.deleteZona(zonaToDelete.id);
      setZonas(zonas.filter((z) => z.id !== zonaToDelete.id));
      setShowDeleteModal(false);
      setZonaToDelete(null);
    } catch (err) {
      setError('Error al eliminar la zona');
      console.error('Error deleting zona:', err);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  return (
    <div>
      <PageHeader
        title="Gestión de Zonas"
        icon="fa-solid fa-location-dot"
        actions={
          <GradientButton icon="fa-solid fa-plus" onClick={handleCreate}>
            Nueva Zona
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
                  {zonasFiltradas.map((zona) => (
                    <tr key={zona.id}>
                      <td>{zona.descripcion || '-'}</td>
                      <td>
                        <IconButton
                          icon="fa-solid fa-pen"
                          title="Editar"
                          variant="primary"
                          onClick={() => handleEdit(zona)}
                        />
                        <IconButton
                          icon="fa-solid fa-trash"
                          title="Eliminar"
                          variant="danger"
                          onClick={() => handleDeleteClick(zona)}
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
                  {editingZona ? 'Editar Zona' : 'Nueva Zona'}
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
                      placeholder="Nombre de la zona"
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
                ¿Está seguro de que desea eliminar la zona{' '}
                <strong>{zonaToDelete?.descripcion}</strong>?
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

export default ZonasPage;
