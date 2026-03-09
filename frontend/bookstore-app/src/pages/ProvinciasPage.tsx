import { useState, useEffect, useMemo } from 'react';
import { referenceService } from '../services/referenceService';
import type { Provincia, CreateProvinciaDto, UpdateProvinciaDto } from '../types/references';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { IconButton } from '../components/IconButton';

const ProvinciasPage = () => {
  const [provincias, setProvincias] = useState<Provincia[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filtro, setFiltro] = useState('');

  const provinciasFiltradas = useMemo(() => {
    if (!filtro.trim()) return provincias;
    const busqueda = filtro.toLowerCase();
    return provincias.filter(p =>
      (p.descripcion?.toLowerCase() || '').includes(busqueda)
    );
  }, [provincias, filtro]);
  const [showModal, setShowModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [provinciaToDelete, setProvinciaToDelete] = useState<Provincia | null>(null);
  const [editingProvincia, setEditingProvincia] = useState<Provincia | null>(null);
  const [formData, setFormData] = useState<CreateProvinciaDto | UpdateProvinciaDto>({
    descripcion: '',
  });

  useEffect(() => {
    loadProvincias();
  }, []);

  const loadProvincias = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await referenceService.getProvincias();
      setProvincias(data);
    } catch (err) {
      setError('Error al cargar las provincias');
      console.error('Error loading provincias:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = () => {
    setEditingProvincia(null);
    setFormData({ descripcion: '' });
    setShowModal(true);
  };

  const handleEdit = (provincia: Provincia) => {
    setEditingProvincia(provincia);
    setFormData({
      descripcion: provincia.descripcion || '',
    });
    setShowModal(true);
  };

  const handleDeleteClick = (provincia: Provincia) => {
    setProvinciaToDelete(provincia);
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
      if (editingProvincia) {
        await referenceService.updateProvincia(editingProvincia.id, formData as UpdateProvinciaDto);
      } else {
        await referenceService.createProvincia(formData as CreateProvinciaDto);
      }
      setShowModal(false);
      await loadProvincias();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al guardar la provincia');
      console.error('Error saving provincia:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteConfirm = async () => {
    if (!provinciaToDelete) return;

    try {
      await referenceService.deleteProvincia(provinciaToDelete.id);
      setProvincias(provincias.filter((p) => p.id !== provinciaToDelete.id));
      setShowDeleteModal(false);
      setProvinciaToDelete(null);
    } catch (err) {
      setError('Error al eliminar la provincia');
      console.error('Error deleting provincia:', err);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  return (
    <div>
      <PageHeader
        title="Gestión de Provincias"
        icon="fa-solid fa-map"
        actions={
          <GradientButton icon="fa-solid fa-plus" onClick={handleCreate}>
            Nueva Provincia
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
                  {provinciasFiltradas.map((provincia) => (
                    <tr key={provincia.id}>
                      <td>{provincia.descripcion || '-'}</td>
                      <td>
                        <IconButton
                          icon="fa-solid fa-pen"
                          title="Editar"
                          variant="primary"
                          onClick={() => handleEdit(provincia)}
                        />
                        <IconButton
                          icon="fa-solid fa-trash"
                          title="Eliminar"
                          variant="danger"
                          onClick={() => handleDeleteClick(provincia)}
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
                  {editingProvincia ? 'Editar Provincia' : 'Nueva Provincia'}
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
                      placeholder="Nombre de la provincia"
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
                ¿Está seguro de que desea eliminar la provincia{' '}
                <strong>{provinciaToDelete?.descripcion}</strong>?
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

export default ProvinciasPage;
