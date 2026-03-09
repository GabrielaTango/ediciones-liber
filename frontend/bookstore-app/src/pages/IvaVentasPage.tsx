import { useState } from 'react';
import { comprobanteService } from '../services/comprobanteService';
import type { IvaVenta } from '../types/dashboard';
import { showErrorAlert } from '../utils/sweetalert';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { Icon } from '../components/Icon';

const IvaVentasPage = () => {
  // Por defecto el mes actual
  const getFirstDayOfMonth = (): string => {
    const now = new Date();
    return new Date(now.getFullYear(), now.getMonth(), 1).toISOString().split('T')[0];
  };

  const getLastDayOfMonth = (): string => {
    const now = new Date();
    return new Date(now.getFullYear(), now.getMonth() + 1, 0).toISOString().split('T')[0];
  };

  const [fechaDesde, setFechaDesde] = useState<string>(getFirstDayOfMonth());
  const [fechaHasta, setFechaHasta] = useState<string>(getLastDayOfMonth());
  const [ventas, setVentas] = useState<IvaVenta[]>([]);
  const [loading, setLoading] = useState(false);
  const [hasSearched, setHasSearched] = useState(false);

  const handleSearch = async () => {
    if (!fechaDesde || !fechaHasta) {
      await showErrorAlert('Error', 'Por favor seleccione ambas fechas');
      return;
    }

    if (fechaDesde > fechaHasta) {
      await showErrorAlert('Error', 'La fecha desde no puede ser mayor que la fecha hasta');
      return;
    }

    try {
      setLoading(true);
      setHasSearched(true);
      const data = await comprobanteService.getIvaVentas(fechaDesde, fechaHasta);
      setVentas(data);
    } catch (error) {
      console.error('Error loading IVA ventas:', error);
      await showErrorAlert('Error', 'No se pudieron cargar los datos de IVA ventas');
    } finally {
      setLoading(false);
    }
  };

  const handleDownloadPdf = () => {
    if (!fechaDesde || !fechaHasta) {
      showErrorAlert('Error', 'Por favor seleccione ambas fechas');
      return;
    }

    if (fechaDesde > fechaHasta) {
      showErrorAlert('Error', 'La fecha desde no puede ser mayor que la fecha hasta');
      return;
    }

    comprobanteService.openIvaVentasPdf(fechaDesde, fechaHasta);
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('es-AR');
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('es-AR', {
      style: 'currency',
      currency: 'ARS'
    }).format(amount);
  };

  const calcularTotal = () => {
    return ventas.reduce((sum, venta) => sum + venta.total, 0);
  };

  return (
    <div>
      <PageHeader
        title="Libro de IVA - Ventas"
        icon="fa-solid fa-file-lines"
      />

      {/* Filtros */}
      <div className="card mb-4">
        <div className="card-body">
          <div className="row g-3">
            <div className="col-md-4">
              <label htmlFor="fechaDesde" className="form-label">
                Fecha Desde
              </label>
              <input
                type="date"
                className="form-control"
                id="fechaDesde"
                value={fechaDesde}
                onChange={(e) => setFechaDesde(e.target.value)}
              />
            </div>
            <div className="col-md-4">
              <label htmlFor="fechaHasta" className="form-label">
                Fecha Hasta
              </label>
              <input
                type="date"
                className="form-control"
                id="fechaHasta"
                value={fechaHasta}
                onChange={(e) => setFechaHasta(e.target.value)}
              />
            </div>
            <div className="col-md-4 d-flex align-items-end gap-2">
              <GradientButton
                icon="fa-solid fa-search"
                onClick={handleSearch}
                disabled={loading}
              >
                {loading ? 'Buscando...' : 'Buscar'}
              </GradientButton>
              {ventas.length > 0 && (
                <button
                  className="btn btn-outline-danger"
                  onClick={handleDownloadPdf}
                >
                  <i className="fa-solid fa-file-pdf me-2"></i>
                  Generar PDF
                </button>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Resultados */}
      {loading && (
        <div className="text-center py-5">
          <div className="spinner-gradient" />
        </div>
      )}

      {!loading && hasSearched && ventas.length === 0 && (
        <div className="alert alert-info" role="alert">
          <Icon name="fa-solid fa-info-circle" className="me-2" />
          No se encontraron ventas en el período seleccionado.
        </div>
      )}

      {!loading && ventas.length > 0 && (
        <div className="card">
          <div className="card-body">
            <div className="table-responsive">
              <table className="custom-table">
                <thead>
                  <tr>
                    <th>Fecha</th>
                    <th>Número</th>
                    <th>Cliente</th>
                    <th>CUIT/DNI</th>
                    <th className="text-end">Débitos</th>
                    <th className="text-end">Créditos</th>
                  </tr>
                </thead>
                <tbody>
                  {ventas.map((venta, index) => (
                    <tr key={index}>
                      <td>{formatDate(venta.fecha)}</td>
                      <td>{venta.numeroComprobante || '-'}</td>
                      <td>{venta.nombre || '-'}</td>
                      <td>{venta.nroDocumento || '-'}</td>
                      <td className="text-end">{formatCurrency(venta.total)}</td>
                      <td className="text-end">{formatCurrency(0)}</td>
                    </tr>
                  ))}
                  <tr className="fw-bold">
                    <td colSpan={4} className="text-end">TOTALES:</td>
                    <td className="text-end">{formatCurrency(calcularTotal())}</td>
                    <td className="text-end">{formatCurrency(0)}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default IvaVentasPage;
