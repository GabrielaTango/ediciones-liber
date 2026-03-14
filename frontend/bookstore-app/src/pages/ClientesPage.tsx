import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { clienteService } from '../services/clienteService';
import type { Cliente } from '../types/cliente';
import { showSuccessAlert, showErrorAlert, showDeleteConfirmDialog } from '../utils/sweetalert';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { IconButton } from '../components/IconButton';

const ClientesPage = () => {
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadClientes();
  }, []);

  const loadClientes = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await clienteService.getAll();
      setClientes(data);
    } catch (err) {
      setError('Error al cargar los clientes. Verifique que la API esté ejecutándose.');
      console.error('Error loading clientes:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteClick = async (cliente: Cliente) => {
    const result = await showDeleteConfirmDialog(cliente.nombre);

    if (result.isConfirmed) {
      try {
        await clienteService.delete(cliente.id);
        setClientes(clientes.filter((c) => c.id !== cliente.id));
        await showSuccessAlert('Cliente eliminado', `El cliente ${cliente.nombre} ha sido eliminado correctamente`);
      } catch (err) {
        await showErrorAlert('Error', 'No se pudo eliminar el cliente');
        console.error('Error deleting cliente:', err);
      }
    }
  };

  return (
    <div>
      <PageHeader
        title="Gestión de Clientes"
        icon="fa-solid fa-users"
        actions={
          <Link to="/clientes/nuevo">
            <GradientButton icon="fa-solid fa-plus">
              Nuevo Cliente
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
                    <th>Documento</th>
                    <th>Teléfono</th>
                    <th>Categoría IVA</th>
                    <th>Descuento</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {clientes.map((cliente) => (
                    <tr key={cliente.id}>
                      <td>{cliente.codigo || '-'}</td>
                      <td>{cliente.nombre}</td>
                      <td>{cliente.nroDocumento || '-'}</td>
                      <td>{cliente.telefono || cliente.telefonoMovil || '-'}</td>
                      <td>{cliente.categoriaIva || '-'}</td>
                      <td>{cliente.descuento}%</td>
                      <td>
                        <Link to={`/clientes/editar/${cliente.id}`}>
                          <IconButton icon="fa-solid fa-pen" title="Editar" variant="primary" />
                        </Link>
                        <IconButton
                          icon="fa-solid fa-trash"
                          title="Eliminar"
                          variant="danger"
                          onClick={() => handleDeleteClick(cliente)}
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

export default ClientesPage;
