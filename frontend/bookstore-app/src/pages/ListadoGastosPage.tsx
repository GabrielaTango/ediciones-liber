import { useState } from 'react';
import { gastoService } from '../services/gastoService';
import type { Gasto } from '../types/gasto';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';

const ListadoGastosPage = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [gastos, setGastos] = useState<Gasto[] | null>(null);

  const hoy = new Date();
  const primerDiaMes = new Date(hoy.getFullYear(), hoy.getMonth(), 1);
  const [fechaDesde, setFechaDesde] = useState(primerDiaMes.toISOString().split('T')[0]);
  const [fechaHasta, setFechaHasta] = useState(hoy.toISOString().split('T')[0]);

  const loadListado = async () => {
    if (!fechaDesde || !fechaHasta) {
      setError('Debe seleccionar ambas fechas');
      return;
    }

    try {
      setLoading(true);
      setError(null);
      const data = await gastoService.getListado(fechaDesde, fechaHasta);
      setGastos(data);
    } catch (err) {
      setError('Error al cargar el listado de gastos');
      console.error('Error loading report:', err);
    } finally {
      setLoading(false);
    }
  };

  const handlePdf = () => {
    gastoService.openListadoPdf(fechaDesde, fechaHasta);
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('es-AR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  };

  const total = gastos?.reduce((sum, g) => sum + g.importe, 0) ?? 0;

  return (
    <div>
      <PageHeader
        title="Listado de Gastos"
        icon="fa-solid fa-receipt"
      />

      {error && <div className="alert alert-danger">{error}</div>}

      {/* Filtros */}
      <div className="card mb-4">
        <div className="card-body">
          <div className="row align-items-end">
            <div className="col-md-3 mb-3 mb-md-0">
              <label className="form-label">Desde</label>
              <input
                type="date"
                className="form-control"
                value={fechaDesde}
                onChange={(e) => setFechaDesde(e.target.value)}
              />
            </div>
            <div className="col-md-3 mb-3 mb-md-0">
              <label className="form-label">Hasta</label>
              <input
                type="date"
                className="form-control"
                value={fechaHasta}
                onChange={(e) => setFechaHasta(e.target.value)}
              />
            </div>
            <div className="col-md-4">
              <div className="d-flex gap-2">
                <GradientButton
                  icon={loading ? 'fa-solid fa-spinner fa-spin' : 'fa-solid fa-search'}
                  onClick={loadListado}
                  disabled={loading}
                >
                  {loading ? 'Buscando...' : 'Buscar'}
                </GradientButton>
                {gastos && gastos.length > 0 && (
                  <button
                    className="btn btn-outline-danger"
                    onClick={handlePdf}
                  >
                    <i className="fa-solid fa-file-pdf me-2"></i>
                    Generar PDF
                  </button>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Tabla de resultados */}
      {loading ? (
        <div className="text-center py-5">
          <div className="spinner-gradient" />
        </div>
      ) : gastos ? (
        <div className="card">
          <div className="card-header d-flex justify-content-between align-items-center">
            <span>
              <strong>Período:</strong> {formatDate(fechaDesde)} - {formatDate(fechaHasta)}
            </span>
            <span className="badge bg-primary">{gastos.length} registros</span>
          </div>
          <div className="card-body">
            <div className="table-responsive">
              <table className="custom-table" style={{ fontSize: '0.85rem' }}>
                <thead>
                  <tr>
                    <th style={{ minWidth: '90px' }}>Fecha</th>
                    <th style={{ minWidth: '120px' }}>Nro. Comprobante</th>
                    <th style={{ minWidth: '120px' }}>Categoría</th>
                    <th style={{ minWidth: '200px' }}>Descripción</th>
                    <th style={{ minWidth: '100px' }} className="text-end">Importe</th>
                  </tr>
                </thead>
                <tbody>
                  {gastos.length === 0 ? (
                    <tr>
                      <td colSpan={5} className="text-center py-4">
                        No se encontraron gastos en el período seleccionado
                      </td>
                    </tr>
                  ) : (
                    gastos.map((gasto, index) => (
                      <tr
                        key={gasto.id}
                        style={index % 2 === 1 ? { backgroundColor: 'rgba(0,0,0,0.03)' } : {}}
                      >
                        <td>{formatDate(gasto.fecha)}</td>
                        <td>{gasto.nroComprobante}</td>
                        <td>{gasto.categoria}</td>
                        <td className="text-truncate" style={{ maxWidth: '300px' }} title={gasto.descripcion}>
                          {gasto.descripcion || '-'}
                        </td>
                        <td className="text-end">${gasto.importe.toLocaleString('es-AR', { minimumFractionDigits: 2 })}</td>
                      </tr>
                    ))
                  )}
                </tbody>
                {gastos.length > 0 && (
                  <tfoot>
                    <tr style={{ borderTop: '2px solid #333' }}>
                      <td colSpan={4} className="fw-bold">Total</td>
                      <td className="text-end fw-bold">${total.toLocaleString('es-AR', { minimumFractionDigits: 2 })}</td>
                    </tr>
                  </tfoot>
                )}
              </table>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  );
};

export default ListadoGastosPage;
