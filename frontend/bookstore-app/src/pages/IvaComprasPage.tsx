import { useState } from 'react';
import { comprobanteProveedorService } from '../services/comprobanteProveedorService';
import type { IvaCompra } from '../types/comprobanteProveedor';
import { showErrorAlert } from '../utils/sweetalert';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { Icon } from '../components/Icon';

const IvaComprasPage = () => {
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
  const [compras, setCompras] = useState<IvaCompra[]>([]);
  const [loading, setLoading] = useState(false);
  const [hasSearched, setHasSearched] = useState(false);

  const handleDownloadPdf = () => {
    if (!fechaDesde || !fechaHasta) {
      showErrorAlert('Error', 'Por favor seleccione ambas fechas');
      return;
    }

    if (fechaDesde > fechaHasta) {
      showErrorAlert('Error', 'La fecha desde no puede ser mayor que la fecha hasta');
      return;
    }

    comprobanteProveedorService.openIvaComprasPdf(fechaDesde, fechaHasta);
  };

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
      const data = await comprobanteProveedorService.getIvaCompras(fechaDesde, fechaHasta);
      setCompras(data);
    } catch (error) {
      console.error('Error loading IVA compras:', error);
      await showErrorAlert('Error', 'No se pudieron cargar los datos de IVA compras');
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
      currency: 'ARS'
    }).format(amount);
  };

  const calcularTotalDebitos = () => {
    return compras
      .filter(c => c.tipoComprobante !== 'NC')
      .reduce((sum, compra) => sum + compra.total, 0);
  };

  const calcularTotalCreditos = () => {
    return compras
      .filter(c => c.tipoComprobante === 'NC')
      .reduce((sum, compra) => sum + compra.total, 0);
  };

  return (
    <div>
      <PageHeader
        title="Libro de IVA - Compras"
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
              {compras.length > 0 && (
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

      {!loading && hasSearched && compras.length === 0 && (
        <div className="alert alert-info" role="alert">
          <Icon name="fa-solid fa-info-circle" className="me-2" />
          No se encontraron compras en el período seleccionado.
        </div>
      )}

      {!loading && compras.length > 0 && (
        <div className="card">
          <div className="card-body">
            <div className="table-responsive">
              <table className="custom-table">
                <thead>
                  <tr>
                    <th>Fecha</th>
                    <th>Tipo</th>
                    <th>Número</th>
                    <th>Proveedor</th>
                    <th>CUIT</th>
                    <th className="text-end">Débitos</th>
                    <th className="text-end">Créditos</th>
                  </tr>
                </thead>
                <tbody>
                  {compras.map((compra, index) => (
                    <tr key={index}>
                      <td>{formatDate(compra.fecha)}</td>
                      <td>
                        <span className={`badge ${
                          compra.tipoComprobante === 'FC' ? 'bg-primary' :
                          compra.tipoComprobante === 'NC' ? 'bg-success' :
                          compra.tipoComprobante === 'ND' ? 'bg-warning text-dark' : 'bg-secondary'
                        }`}>
                          {compra.tipoComprobante}
                        </span>
                      </td>
                      <td>{compra.numeroComprobante || '-'}</td>
                      <td>{compra.nombre || '-'}</td>
                      <td>{compra.cuit || '-'}</td>
                      <td className="text-end">
                        {compra.tipoComprobante !== 'NC' ? formatCurrency(compra.total) : formatCurrency(0)}
                      </td>
                      <td className="text-end">
                        {compra.tipoComprobante === 'NC' ? formatCurrency(compra.total) : formatCurrency(0)}
                      </td>
                    </tr>
                  ))}
                  <tr className="fw-bold">
                    <td colSpan={5} className="text-end">TOTALES:</td>
                    <td className="text-end">{formatCurrency(calcularTotalDebitos())}</td>
                    <td className="text-end">{formatCurrency(calcularTotalCreditos())}</td>
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

export default IvaComprasPage;
