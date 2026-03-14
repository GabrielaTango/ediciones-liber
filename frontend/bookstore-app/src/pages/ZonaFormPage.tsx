import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { referenceService } from '../services/referenceService';
import type { Zona, SubZona, CreateSubZonaDto, UpdateSubZonaDto, Provincia } from '../types/references';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { GradientCard } from '../components/GradientCard';
import { IconButton } from '../components/IconButton';
import { Icon } from '../components/Icon';

const ZonaFormPage = () => {
  const { id } = useParams<{ id: string }>();
  const zonaId = parseInt(id || '0');

  const [zona, setZona] = useState<Zona | null>(null);
  const [subzonas, setSubzonas] = useState<SubZona[]>([]);
  const [provincias, setProvincias] = useState<Provincia[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // SubZona modal
  const [showModal, setShowModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [szToDelete, setSzToDelete] = useState<SubZona | null>(null);
  const [editingSz, setEditingSz] = useState<SubZona | null>(null);
  const [szForm, setSzForm] = useState<CreateSubZonaDto | UpdateSubZonaDto>({
    descripcion: '',
    zonaId: zonaId,
    provinciaId: 0,
    codigoPostal: '',
    localidad: '',
  });

  useEffect(() => {
    loadData();
  }, [zonaId]);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const [zonaData, subzonasData, provinciasData] = await Promise.all([
        referenceService.getZonaById(zonaId),
        referenceService.getSubZonasByZona(zonaId),
        referenceService.getProvincias(),
      ]);
      setZona(zonaData);
      setSubzonas(subzonasData);
      setProvincias(provinciasData);
    } catch (err) {
      setError('Error al cargar los datos de la zona');
      console.error('Error loading zona data:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadSubzonas = async () => {
    try {
      const data = await referenceService.getSubZonasByZona(zonaId);
      setSubzonas(data);
    } catch (err) {
      console.error('Error loading subzonas:', err);
    }
  };

  const handleCreate = () => {
    setEditingSz(null);
    setSzForm({ descripcion: '', zonaId: zonaId, provinciaId: 0, codigoPostal: '', localidad: '' });
    setShowModal(true);
  };

  const handleEdit = (sz: SubZona) => {
    setEditingSz(sz);
    setSzForm({
      descripcion: sz.descripcion || '',
      zonaId: zonaId,
      provinciaId: sz.provinciaId || 0,
      codigoPostal: sz.codigoPostal || '',
      localidad: sz.localidad || '',
    });
    setShowModal(true);
  };

  const handleDeleteClick = (sz: SubZona) => {
    setSzToDelete(sz);
    setShowDeleteModal(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    if (!szForm.descripcion.trim()) { setError('La descripción es obligatoria'); return; }
    if (!szForm.provinciaId || szForm.provinciaId === 0) { setError('La provincia es obligatoria'); return; }
    if (!szForm.codigoPostal.trim()) { setError('El código postal es obligatorio'); return; }
    if (!szForm.localidad.trim()) { setError('La localidad es obligatoria'); return; }

    try {
      if (editingSz) {
        await referenceService.updateSubZona(editingSz.id, szForm as UpdateSubZonaDto);
      } else {
        await referenceService.createSubZona(szForm as CreateSubZonaDto);
      }
      setShowModal(false);
      await loadSubzonas();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al guardar la subzona');
    }
  };

  const handleDeleteConfirm = async () => {
    if (!szToDelete) return;
    try {
      await referenceService.deleteSubZona(szToDelete.id);
      setSubzonas(subzonas.filter((s) => s.id !== szToDelete.id));
      setShowDeleteModal(false);
      setSzToDelete(null);
    } catch (err) {
      setError('Error al eliminar la subzona');
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    if (name === 'provinciaId') {
      setSzForm((prev) => ({ ...prev, [name]: parseInt(value) || 0 }));
    } else {
      setSzForm((prev) => ({ ...prev, [name]: value }));
    }
  };

  if (loading) {
    return (
      <div className="text-center py-5">
        <div className="spinner-gradient" />
      </div>
    );
  }

  return (
    <div>
      <PageHeader
        title={`Zona: ${zona?.descripcion || ''}`}
        icon="fa-solid fa-location-dot"
        actions={
          <Link to="/zonas" className="btn-secondary-action">
            <Icon name="fa-solid fa-arrow-left" />
            Volver
          </Link>
        }
      />

      {error && <div className="alert alert-danger">{error}</div>}

      <GradientCard
        title="SubZonas"
        icon="fa-solid fa-map-location-dot"
      >
        <div className="d-flex justify-content-end mb-3">
          <GradientButton icon="fa-solid fa-plus" onClick={handleCreate}>
            Nueva SubZona
          </GradientButton>
        </div>

        {subzonas.length === 0 ? (
          <div className="alert alert-info mb-0">
            No hay subzonas para esta zona. Haga clic en "Nueva SubZona" para agregar una.
          </div>
        ) : (
          <div className="table-responsive">
            <table className="custom-table">
              <thead>
                <tr>
                  <th>Nombre</th>
                  <th>Provincia</th>
                  <th>Código Postal</th>
                  <th>Localidad</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {subzonas.map((sz) => (
                  <tr key={sz.id}>
                    <td>{sz.descripcion || '-'}</td>
                    <td>{sz.provinciaDescripcion || '-'}</td>
                    <td>{sz.codigoPostal || '-'}</td>
                    <td>{sz.localidad || '-'}</td>
                    <td>
                      <IconButton icon="fa-solid fa-pen" title="Editar" variant="primary" onClick={() => handleEdit(sz)} />
                      <IconButton icon="fa-solid fa-trash" title="Eliminar" variant="danger" onClick={() => handleDeleteClick(sz)} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </GradientCard>

      {/* Modal SubZona crear/editar */}
      {showModal && (
        <div className="modal show d-block" tabIndex={-1} style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">{editingSz ? 'Editar SubZona' : 'Nueva SubZona'}</h5>
                <button type="button" className="btn-close" onClick={() => setShowModal(false)} aria-label="Close"></button>
              </div>
              <form onSubmit={handleSubmit}>
                <div className="modal-body">
                  <div className="mb-3">
                    <label className="form-label">Nombre <span className="text-danger">*</span></label>
                    <input type="text" className="form-control" name="descripcion" value={szForm.descripcion} onChange={handleChange} placeholder="Nombre de la subzona" maxLength={100} required />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Provincia <span className="text-danger">*</span></label>
                    <select className="form-select" name="provinciaId" value={szForm.provinciaId} onChange={handleChange} required>
                      <option value={0}>Seleccione una provincia</option>
                      {provincias.map((p) => (
                        <option key={p.id} value={p.id}>{p.descripcion}</option>
                      ))}
                    </select>
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Código Postal <span className="text-danger">*</span></label>
                    <input type="text" className="form-control" name="codigoPostal" value={szForm.codigoPostal} onChange={handleChange} placeholder="Código Postal" maxLength={10} required />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Localidad <span className="text-danger">*</span></label>
                    <input type="text" className="form-control" name="localidad" value={szForm.localidad} onChange={handleChange} placeholder="Localidad" maxLength={100} required />
                  </div>
                </div>
                <div className="modal-footer">
                  <button type="button" className="btn btn-secondary" onClick={() => setShowModal(false)}>Cancelar</button>
                  <button type="submit" className="btn btn-primary">Guardar</button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Modal SubZona eliminar */}
      {showDeleteModal && (
        <div className="modal show d-block" tabIndex={-1} style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Confirmar Eliminación</h5>
                <button type="button" className="btn-close" onClick={() => setShowDeleteModal(false)} aria-label="Close"></button>
              </div>
              <div className="modal-body">
                ¿Está seguro de que desea eliminar la subzona <strong>{szToDelete?.descripcion}</strong>?
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowDeleteModal(false)}>Cancelar</button>
                <button type="button" className="btn btn-danger" onClick={handleDeleteConfirm}>Eliminar</button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ZonaFormPage;
