import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { comprobanteProveedorService } from '../services/comprobanteProveedorService';
import { PageHeader } from '../components/PageHeader';
import { GradientCard } from '../components/GradientCard';
import { Icon } from '../components/Icon';
import type { ComprobanteProveedorDetail } from '../types/comprobanteProveedor';

const tipoLabels: Record<string, { label: string; className: string }> = {
  FC: { label: 'Factura', className: 'bg-primary' },
  NC: { label: 'Nota Crédito', className: 'bg-success' },
  ND: { label: 'Nota Débito', className: 'bg-danger' },
};

const estadoLabels: Record<string, { label: string; className: string }> = {
  PEN: { label: 'Pendiente', className: 'bg-warning text-dark' },
  PAG: { label: 'Pagado', className: 'bg-success' },
  PAR: { label: 'Parcial', className: 'bg-info' },
};

const ComprobanteProveedorViewPage = () => {
  const { id } = useParams<{ id: string }>();
  const [comprobante, setComprobante] = useState<ComprobanteProveedorDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (id) {
      loadComprobante(parseInt(id));
    }
  }, [id]);

  const loadComprobante = async (comprobanteId: number) => {
    try {
      setLoading(true);
      setError(null);
      const data = await comprobanteProveedorService.getById(comprobanteId);
      setComprobante(data);
    } catch (err) {
      setError('Error al cargar el comprobante de proveedor.');
      console.error('Error loading comprobante proveedor:', err);
    } finally {
      setLoading(false);
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

  if (loading) {
    return (
      <div className="text-center py-5">
        <div className="spinner-gradient" />
      </div>
    );
  }

  if (error || !comprobante) {
    return (
      <div>
        <PageHeader
          title="Ver Comprobante"
          icon="fa-solid fa-file-invoice"
          actions={
            <Link to="/comprobantes-proveedores" className="btn-secondary-action">
              <Icon name="fa-solid fa-arrow-left" />
              Volver
            </Link>
          }
        />
        <div className="alert alert-danger">{error || 'Comprobante no encontrado'}</div>
      </div>
    );
  }

  const tipo = tipoLabels[comprobante.tipoComprobante] || { label: comprobante.tipoComprobante, className: 'bg-secondary' };
  const totalPagado = comprobante.cuotas.reduce((sum, c) => sum + c.importePagado, 0);
  const saldoPendiente = comprobante.importeTotal - totalPagado;

  return (
    <div>
      <PageHeader
        title={`Comprobante ${comprobante.nroComprobante}`}
        icon="fa-solid fa-file-invoice"
        actions={
          <Link to="/comprobantes-proveedores" className="btn-secondary-action">
            <Icon name="fa-solid fa-arrow-left" />
            Volver
          </Link>
        }
      />

      <GradientCard title="Datos del Comprobante" icon="fa-solid fa-file-invoice-dollar">
        <div className="row">
          <div className="col-md-6 mb-3">
            <strong>Proveedor:</strong>
            <div>{comprobante.proveedorNombre}</div>
          </div>
          <div className="col-md-3 mb-3">
            <strong>Tipo:</strong>
            <div>
              <span className={`badge ${tipo.className}`}>{tipo.label}</span>
            </div>
          </div>
          <div className="col-md-3 mb-3">
            <strong>Fecha Emisión:</strong>
            <div>{formatDate(comprobante.fechaEmision)}</div>
          </div>
          <div className="col-md-4 mb-3">
            <strong>Nro. Comprobante:</strong>
            <div>{comprobante.nroComprobante}</div>
          </div>
          <div className="col-md-4 mb-3">
            <strong>Importe Total:</strong>
            <div className="fw-bold fs-5">{formatCurrency(comprobante.importeTotal)}</div>
          </div>
          <div className="col-md-2 mb-3">
            <strong>Cuotas:</strong>
            <div>{comprobante.cantidadCuotas}</div>
          </div>
          <div className="col-md-2 mb-3">
            <strong>1er Vencimiento:</strong>
            <div>{formatDate(comprobante.fechaPrimerVencimiento)}</div>
          </div>
        </div>
      </GradientCard>

      <GradientCard title="Detalle de Cuotas" icon="fa-solid fa-list-ol">
        <div className="table-responsive">
          <table className="custom-table">
            <thead>
              <tr>
                <th># Cuota</th>
                <th>Fecha Vencimiento</th>
                <th className="text-end">Importe</th>
                <th className="text-end">Pagado</th>
                <th className="text-end">Saldo</th>
                <th className="text-center">Estado</th>
              </tr>
            </thead>
            <tbody>
              {comprobante.cuotas.map((cuota) => {
                const est = estadoLabels[cuota.estado] || { label: cuota.estado, className: 'bg-secondary' };
                const saldoCuota = cuota.importe - cuota.importePagado;
                return (
                  <tr key={cuota.id}>
                    <td>{cuota.numeroCuota}</td>
                    <td>{formatDate(cuota.fechaVencimiento)}</td>
                    <td className="text-end">{formatCurrency(cuota.importe)}</td>
                    <td className="text-end">{formatCurrency(cuota.importePagado)}</td>
                    <td className="text-end">{formatCurrency(saldoCuota)}</td>
                    <td className="text-center">
                      <span className={`badge ${est.className}`}>{est.label}</span>
                    </td>
                  </tr>
                );
              })}
            </tbody>
            <tfoot>
              <tr className="fw-bold">
                <td colSpan={2}>Total</td>
                <td className="text-end">{formatCurrency(comprobante.importeTotal)}</td>
                <td className="text-end">{formatCurrency(totalPagado)}</td>
                <td className="text-end">{formatCurrency(saldoPendiente)}</td>
                <td></td>
              </tr>
            </tfoot>
          </table>
        </div>
      </GradientCard>
    </div>
  );
};

export default ComprobanteProveedorViewPage;
