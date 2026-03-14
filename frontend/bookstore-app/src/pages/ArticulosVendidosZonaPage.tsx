import { useState, useEffect } from 'react';
import { comprobanteService } from '../services/comprobanteService';
import { referenceService } from '../services/referenceService';
import type { ArticulosVendidosZonaReporte } from '../types/comprobante';
import type { Zona } from '../types/references';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';

const ArticulosVendidosZonaPage = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [reporte, setReporte] = useState<ArticulosVendidosZonaReporte | null>(null);
  const [zonas, setZonas] = useState<Zona[]>([]);
  const [zonaId, setZonaId] = useState<number | ''>('');

  useEffect(() => {
    loadZonas();
  }, []);

  const loadZonas = async () => {
    try {
      const data = await referenceService.getZonas();
      setZonas(data);
    } catch (err) {
      console.error('Error loading zonas:', err);
    }
  };

  const loadReporte = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await comprobanteService.getArticulosVendidosZona(zonaId || undefined);
      setReporte(data);
    } catch (err) {
      setError('Error al cargar el reporte de clientes por zona');
      console.error('Error loading report:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleBuscar = () => {
    loadReporte();
  };

  const handlePdf = () => {
    comprobanteService.openArticulosVendidosZonaPdf(zonaId || undefined);
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('es-AR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  };

  return (
    <div>
      <PageHeader
        title="Clientes x Zona"
        icon="fa-solid fa-chart-bar"
      />

      {error && <div className="alert alert-danger">{error}</div>}

      {/* Filtros */}
      <div className="card mb-4">
        <div className="card-body">
          <div className="row align-items-end">
            <div className="col-md-4 mb-3 mb-md-0">
              <label className="form-label">Zona</label>
              <select
                className="form-select"
                value={zonaId}
                onChange={(e) => setZonaId(e.target.value ? parseInt(e.target.value, 10) : '')}
              >
                <option value="">Todas las zonas</option>
                {zonas.map((z) => (
                  <option key={z.id} value={z.id}>
                    {z.descripcion}
                  </option>
                ))}
              </select>
            </div>
            <div className="col-md-4">
              <div className="d-flex gap-2">
                <GradientButton
                  icon={loading ? 'fa-solid fa-spinner fa-spin' : 'fa-solid fa-search'}
                  onClick={handleBuscar}
                  disabled={loading}
                >
                  {loading ? 'Buscando...' : 'Buscar'}
                </GradientButton>
                {reporte && reporte.items.length > 0 && (
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

      {/* Información del período */}
      <div className="alert alert-info mb-4">
        <i className="fa-solid fa-info-circle me-2"></i>
        Este reporte muestra los clientes con al menos una venta en los últimos 3 años.
      </div>

      {/* Tabla de resultados */}
      {loading ? (
        <div className="text-center py-5">
          <div className="spinner-gradient" />
        </div>
      ) : reporte ? (
        <div className="card">
          <div className="card-header d-flex justify-content-between align-items-center">
            <span>
              <strong>Zona:</strong> {reporte.zonaNombre}
            </span>
            <span className="badge bg-primary">{reporte.items.length} registros</span>
          </div>
          <div className="card-body">
            <div className="table-responsive">
              <table className="custom-table" style={{ fontSize: '0.85rem' }}>
                <thead>
                  <tr>
                    <th style={{ minWidth: '40px' }}>V</th>
                    <th style={{ minWidth: '180px' }}>Razón Social</th>
                    <th style={{ minWidth: '150px' }}>Dirección</th>
                    <th style={{ minWidth: '150px' }}>Dir. Comercial</th>
                    <th style={{ minWidth: '200px' }}>Artículo</th>
                    <th style={{ minWidth: '90px' }}>Fecha</th>
                    <th style={{ minWidth: '120px' }}>Nro. Factura</th>
                  </tr>
                </thead>
                <tbody>
                  {reporte.items.length === 0 ? (
                    <tr>
                      <td colSpan={7} className="text-center py-4">
                        No se encontraron artículos vendidos en el período
                      </td>
                    </tr>
                  ) : (
                    reporte.items.map((item, index) => {
                      // Detectar cambio de cliente para alternar color
                      const prevItem = index > 0 ? reporte.items[index - 1] : null;
                      const isNewClient = !prevItem || item.razonSocial !== prevItem.razonSocial;
                      const clientIndex = reporte.items
                        .slice(0, index + 1)
                        .filter((it, idx) => idx === 0 || it.razonSocial !== reporte.items[idx - 1].razonSocial)
                        .length;
                      const altBackground = clientIndex % 2 === 0;

                      return (
                        <tr
                          key={`${item.codigoCliente}-${item.numeroFactura}-${index}`}
                          style={altBackground ? { backgroundColor: 'rgba(0,0,0,0.03)' } : {}}
                        >
                          <td className="text-center fw-bold">{item.vendedorInicial}</td>
                          <td>
                            {isNewClient && (
                              <strong>{item.razonSocial}</strong>
                            )}
                            {!isNewClient && item.razonSocial}
                          </td>
                          <td className="text-truncate" style={{ maxWidth: '150px' }} title={item.direccion}>
                            {item.direccion || '-'}
                          </td>
                          <td className="text-truncate" style={{ maxWidth: '150px' }} title={item.direccionComercial}>
                            {item.direccionComercial || '-'}
                          </td>
                          <td className="text-truncate" style={{ maxWidth: '200px' }} title={item.descripcionArticulo}>
                            {item.descripcionArticulo || '-'}
                          </td>
                          <td>{formatDate(item.fechaFactura)}</td>
                          <td>{item.numeroFactura || '-'}</td>
                        </tr>
                      );
                    })
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  );
};

export default ArticulosVendidosZonaPage;
