import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { comprobanteProveedorService } from '../services/comprobanteProveedorService';
import type { ComprobanteProveedor } from '../types/comprobanteProveedor';
import { showSuccessAlert, showErrorAlert, showDeleteConfirmDialog } from '../utils/sweetalert';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { IconButton } from '../components/IconButton';

const tipoLabels: Record<string, { label: string; className: string }> = {
  FC: { label: 'Factura', className: 'bg-primary' },
  NC: { label: 'Nota Crédito', className: 'bg-success' },
  ND: { label: 'Nota Débito', className: 'bg-danger' },
};

const ComprobantesProveedoresPage = () => {
  const [comprobantes, setComprobantes] = useState<ComprobanteProveedor[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadComprobantes();
  }, []);

  const loadComprobantes = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await comprobanteProveedorService.getAll();
      setComprobantes(data);
    } catch (err) {
      setError('Error al cargar los comprobantes de proveedores. Verifique que la API esté ejecutándose.');
      console.error('Error loading comprobantes proveedores:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteClick = async (comp: ComprobanteProveedor) => {
    const result = await showDeleteConfirmDialog(`el comprobante ${comp.nroComprobante}`);

    if (result.isConfirmed) {
      try {
        await comprobanteProveedorService.delete(comp.id);
        setComprobantes(comprobantes.filter((c) => c.id !== comp.id));
        await showSuccessAlert('Comprobante eliminado', `El comprobante ${comp.nroComprobante} ha sido eliminado correctamente`);
      } catch (err) {
        await showErrorAlert('Error', 'No se pudo eliminar el comprobante');
        console.error('Error deleting comprobante proveedor:', err);
      }
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('es-AR');
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('es-AR', {
      style: 'currency',
      currency: 'ARS',
    }).format(amount);
  };

  return (
    <div>
      <PageHeader
        title="Comprobantes de Proveedores"
        icon="fa-solid fa-file-invoice"
        actions={
          <Link to="/comprobantes-proveedores/nuevo">
            <GradientButton icon="fa-solid fa-plus">
              Nuevo Comprobante
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
                    <th>Fecha</th>
                    <th>Tipo</th>
                    <th>Nro. Comprobante</th>
                    <th>Proveedor</th>
                    <th>Importe</th>
                    <th>Cuotas</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {comprobantes.length === 0 ? (
                    <tr>
                      <td colSpan={7} className="text-center text-muted py-4">
                        No hay comprobantes de proveedores registrados
                      </td>
                    </tr>
                  ) : (
                    comprobantes.map((comp) => {
                      const tipo = tipoLabels[comp.tipoComprobante] || { label: comp.tipoComprobante, className: 'bg-secondary' };
                      return (
                        <tr key={comp.id}>
                          <td>{formatDate(comp.fechaEmision)}</td>
                          <td>
                            <span className={`badge ${tipo.className}`}>{tipo.label}</span>
                          </td>
                          <td>{comp.nroComprobante}</td>
                          <td>{comp.proveedorNombre}</td>
                          <td className="text-end fw-bold">{formatCurrency(comp.importeTotal)}</td>
                          <td className="text-center">{comp.cantidadCuotas}</td>
                          <td>
                            <Link to={`/comprobantes-proveedores/ver/${comp.id}`}>
                              <IconButton icon="fa-solid fa-eye" title="Ver" variant="primary" />
                            </Link>
                            <IconButton
                              icon="fa-solid fa-trash"
                              title="Eliminar"
                              variant="danger"
                              onClick={() => handleDeleteClick(comp)}
                            />
                          </td>
                        </tr>
                      );
                    })
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

export default ComprobantesProveedoresPage;
