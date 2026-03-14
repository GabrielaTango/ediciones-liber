import { useState, useEffect } from 'react';
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
                  {articulos.map((articulo) => (
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
                  ))}
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
