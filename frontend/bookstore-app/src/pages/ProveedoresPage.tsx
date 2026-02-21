import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { proveedorService } from '../services/proveedorService';
import type { Proveedor } from '../types/proveedor';
import { showSuccessAlert, showErrorAlert, showDeleteConfirmDialog } from '../utils/sweetalert';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { IconButton } from '../components/IconButton';

const ProveedoresPage = () => {
  const [proveedores, setProveedores] = useState<Proveedor[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadProveedores();
  }, []);

  const loadProveedores = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await proveedorService.getAll();
      setProveedores(data);
    } catch (err) {
      setError('Error al cargar los proveedores. Verifique que la API esté ejecutándose.');
      console.error('Error loading proveedores:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteClick = async (proveedor: Proveedor) => {
    const result = await showDeleteConfirmDialog(proveedor.nombre);

    if (result.isConfirmed) {
      try {
        await proveedorService.delete(proveedor.id);
        setProveedores(proveedores.filter((p) => p.id !== proveedor.id));
        await showSuccessAlert('Proveedor eliminado', `El proveedor ${proveedor.nombre} ha sido eliminado correctamente`);
      } catch (err) {
        await showErrorAlert('Error', 'No se pudo eliminar el proveedor');
        console.error('Error deleting proveedor:', err);
      }
    }
  };

  return (
    <div>
      <PageHeader
        title="Gestión de Proveedores"
        icon="fa-solid fa-truck-field"
        actions={
          <Link to="/proveedores/nuevo">
            <GradientButton icon="fa-solid fa-plus">
              Nuevo Proveedor
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
                    <th>Código</th>
                    <th>Nombre</th>
                    <th>Razón Social</th>
                    <th>CUIT</th>
                    <th>Teléfono</th>
                    <th>Mail</th>
                    <th>Estado</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {proveedores.map((proveedor) => (
                    <tr key={proveedor.id}>
                      <td>{proveedor.codigo || '-'}</td>
                      <td>{proveedor.nombre}</td>
                      <td>{proveedor.razonSocial || '-'}</td>
                      <td>{proveedor.cuit || '-'}</td>
                      <td>{proveedor.telefono || '-'}</td>
                      <td>{proveedor.mail || '-'}</td>
                      <td>
                        {proveedor.fechaInhabilitacion ? (
                          <span className="badge bg-danger">Inhabilitado</span>
                        ) : (
                          <span className="badge bg-success">Activo</span>
                        )}
                      </td>
                      <td>
                        <Link to={`/proveedores/editar/${proveedor.id}`}>
                          <IconButton icon="fa-solid fa-pen" title="Editar" variant="primary" />
                        </Link>
                        <IconButton
                          icon="fa-solid fa-trash"
                          title="Eliminar"
                          variant="danger"
                          onClick={() => handleDeleteClick(proveedor)}
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

export default ProveedoresPage;
