import { useState, useEffect, useMemo } from 'react';
import { Link } from 'react-router-dom';
import { clienteService } from '../services/clienteService';
import { referenceService } from '../services/referenceService';
import type { Cliente } from '../types/cliente';
import type { Zona } from '../types/references';
import { showSuccessAlert, showErrorAlert, showDeleteConfirmDialog } from '../utils/sweetalert';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { IconButton } from '../components/IconButton';

const ClientesPage = () => {
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [zonas, setZonas] = useState<Zona[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [zonaFilter, setZonaFilter] = useState<number | ''>('');

  const zonasMap = useMemo(() => new Map(zonas.map((z) => [z.id, z.descripcion || ''])), [zonas]);

  const clientesFiltrados = useMemo(() => {
    const term = search.trim().toLowerCase();
    return clientes.filter((c) => {
      if (zonaFilter !== '' && c.zona_Id !== zonaFilter) return false;
      if (!term) return true;
      return [
        c.codigo,
        c.nombre,
        c.nroDocumento,
        c.telefono,
        c.telefonoMovil,
        c.categoriaIva,
      ]
        .filter(Boolean)
        .some((v) => String(v).toLowerCase().includes(term));
    });
  }, [clientes, search, zonaFilter]);

  useEffect(() => {
    loadAll();
  }, []);

  const loadAll = async () => {
    try {
      setLoading(true);
      setError(null);
      const [clientesData, zonasData] = await Promise.all([
        clienteService.getAll(),
        referenceService.getZonas(),
      ]);
      setClientes(clientesData);
      setZonas(zonasData);
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
          <>
            <a href="/solicitud.pdf" download="solicitud.pdf">
              <GradientButton icon="fa-solid fa-file-pdf">
                Descargar Solicitud
              </GradientButton>
            </a>
            <Link to="/clientes/nuevo">
              <GradientButton icon="fa-solid fa-plus">
                Nuevo Cliente
              </GradientButton>
            </Link>
          </>
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
            <div className="row g-2 mb-3">
              <div className="col-md-7">
                <div className="input-group">
                  <i className="fa-solid fa-magnifying-glass"></i>
                  <input
                    type="text"
                    className="form-control"
                    placeholder="Buscar por código, nombre, documento, teléfono..."
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
              <div className="col-md-5">
                <select
                  className="form-select"
                  value={zonaFilter}
                  onChange={(e) => setZonaFilter(e.target.value ? parseInt(e.target.value, 10) : '')}
                >
                  <option value="">Todas las zonas</option>
                  {zonas.map((z) => (
                    <option key={z.id} value={z.id}>
                      {z.descripcion}
                    </option>
                  ))}
                </select>
              </div>
            </div>
            <div className="text-muted small mb-2">
              Mostrando {clientesFiltrados.length} de {clientes.length}
            </div>
            <div className="table-responsive">
              <table className="custom-table">
                <thead>
                  <tr>
                    <th>Código</th>
                    <th>Nombre</th>
                    <th>Documento</th>
                    <th>Teléfono</th>
                    <th>Zona</th>
                    <th>Categoría IVA</th>
                    <th>Descuento</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {clientesFiltrados.length === 0 ? (
                    <tr>
                      <td colSpan={8} className="text-center py-4 text-muted">
                        No se encontraron clientes
                      </td>
                    </tr>
                  ) : (
                  clientesFiltrados.map((cliente) => (
                    <tr key={cliente.id}>
                      <td>{cliente.codigo || '-'}</td>
                      <td>{cliente.nombre}</td>
                      <td>{cliente.nroDocumento || '-'}</td>
                      <td>{cliente.telefono || cliente.telefonoMovil || '-'}</td>
                      <td>{cliente.zona_Id ? zonasMap.get(cliente.zona_Id) || '-' : '-'}</td>
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

export default ClientesPage;
