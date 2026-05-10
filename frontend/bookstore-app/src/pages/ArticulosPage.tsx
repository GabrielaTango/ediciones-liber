import { useState, useEffect, useMemo } from 'react';
import { Link } from 'react-router-dom';
import { articuloService } from '../services/articuloService';
import type { Articulo } from '../types/articulo';
import { showSuccessAlert, showErrorAlert, showDeleteConfirmDialog } from '../utils/sweetalert';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { IconButton } from '../components/IconButton';

const ArticulosPage = () => {
  const [articulos, setArticulos] = useState<Articulo[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');

  const articulosFiltrados = useMemo(() => {
    const term = search.trim().toLowerCase();
    if (!term) return articulos;
    return articulos.filter((a) =>
      [a.descripcion, a.codBarras, a.tema, a.tomos]
        .filter((v) => v !== null && v !== undefined && v !== '')
        .some((v) => String(v).toLowerCase().includes(term))
    );
  }, [articulos, search]);

  useEffect(() => {
    loadArticulos();
  }, []);

  const loadArticulos = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await articuloService.getAll();
      setArticulos(data);
    } catch (err) {
      setError('Error al cargar los artículos. Verifique que la API esté ejecutándose.');
      console.error('Error loading articulos:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteClick = async (articulo: Articulo) => {
    const result = await showDeleteConfirmDialog(articulo.descripcion || 'este artículo');

    if (result.isConfirmed) {
      try {
        await articuloService.delete(articulo.id);
        setArticulos(articulos.filter((a) => a.id !== articulo.id));
        await showSuccessAlert('Artículo eliminado', `El artículo ${articulo.descripcion} ha sido eliminado correctamente`);
      } catch (err) {
        await showErrorAlert('Error', 'No se pudo eliminar el artículo');
        console.error('Error deleting articulo:', err);
      }
    }
  };

  return (
    <div>
      <PageHeader
        title="Gestión de Artículos"
        icon="fa-solid fa-box"
        actions={
          <Link to="/articulos/nuevo">
            <GradientButton icon="fa-solid fa-plus">
              Nuevo Artículo
            </GradientButton>
          </Link>
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
            <div className="row mb-3">
              <div className="col-md-6">
                <div className="input-group">
                  <i className="fa-solid fa-magnifying-glass"></i>
                  <input
                    type="text"
                    className="form-control"
                    placeholder="Buscar por descripción, código de barras, editorial..."
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    style={{ borderTopLeftRadius: 'var(--radius-md)', borderBottomLeftRadius: 'var(--radius-md)' }}
                  />
                  {search && (
                    <button
                      type="button"
                      className="btn btn-outline-secondary"
                      onClick={() => setSearch('')}
                      title="Limpiar"
                      style={{ position: 'absolute', right: '0.5rem', top: '50%', transform: 'translateY(-50%)', zIndex: 5, border: 'none', background: 'transparent' }}
                    >
                      <i className="fa-solid fa-xmark"></i>
                    </button>
                  )}
                </div>
              </div>
              <div className="col-md-6 d-flex align-items-center justify-content-md-end text-muted small">
                Mostrando {articulosFiltrados.length} de {articulos.length}
              </div>
            </div>
            <div className="table-responsive">
              <table className="custom-table">
                <thead>
                  <tr>
                    <th>Descripción</th>
                    <th>Código de Barras</th>
                    <th>Editorial</th>
                    <th>Tomos</th>
                    <th>Precio</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {articulosFiltrados.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="text-center py-4 text-muted">
                        No se encontraron artículos
                      </td>
                    </tr>
                  ) : (
                  articulosFiltrados.map((articulo) => (
                    <tr key={articulo.id}>
                      <td>{articulo.descripcion || '-'}</td>
                      <td>{articulo.codBarras || '-'}</td>
                      <td>{articulo.tema || '-'}</td>
                      <td>{articulo.tomos || '-'}</td>
                      <td>
                        {articulo.precio !== null && articulo.precio !== undefined
                          ? `$${articulo.precio.toFixed(2)}`
                          : '-'}
                      </td>
                      <td>
                        <Link to={`/articulos/editar/${articulo.id}`}>
                          <IconButton icon="fa-solid fa-pen" title="Editar" variant="primary" />
                        </Link>
                        <IconButton
                          icon="fa-solid fa-trash"
                          title="Eliminar"
                          variant="danger"
                          onClick={() => handleDeleteClick(articulo)}
                        />
                      </td>
                    </tr>
                  ))
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ArticulosPage;
