import { useState, useEffect } from 'react';
import { comprobanteService } from '../services/comprobanteService';
import { referenceService } from '../services/referenceService';
import type { DeudoresReporte, DeudorItem } from '../types/deudores';
import type { Zona } from '../types/references';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';

const DeudoresPage = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [reporte, setReporte] = useState<DeudoresReporte | null>(null);
  const [zonas, setZonas] = useState<Zona[]>([]);

  // Inicializar con el mes actual
  const now = new Date();
  const [mes, setMes] = useState(now.getMonth() + 1);
  const [anio, setAnio] = useState(now.getFullYear());
  const [zonaId, setZonaId] = useState<number | ''>('');

  useEffect(() => {
    loadZonas();
    loadDeudores();
  }, []);

  const loadZonas = async () => {
    try {
      const data = await referenceService.getZonas();
      setZonas(data);
    } catch (err) {
      console.error('Error loading zonas:', err);
    }
  };

  const loadDeudores = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await comprobanteService.getDeudores(mes, anio, zonaId || undefined);
      setReporte(data);
    } catch (err) {
      setError('Error al cargar los datos de deudores');
      console.error('Error loading deudores:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleBuscar = () => {
    loadDeudores();
  };

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('es-AR', {
      style: 'currency',
      currency: 'ARS',
      minimumFractionDigits: 2,
    }).format(value);
  };

  const getCuotaValue = (deudor: DeudorItem, periodo: string) => {
    const cuota = deudor.cuotas.find(c => c.periodo === periodo);
    if (!cuota) return null;

    // Mostrar el importe pagado (puede ser 0, parcial o total)
    return cuota.importePagado;
  };

  const getCuotaStyle = (deudor: DeudorItem, periodo: string) => {
    const cuota = deudor.cuotas.find(c => c.periodo === periodo);
    if (!cuota) return {};

    // Pagada completamente
    if (cuota.estado === 'PAG' || cuota.importePagado >= cuota.importe) {
      return { backgroundColor: 'rgba(25, 135, 84, 0.1)', color: '#198754' };
    }
    // Parcialmente pagada
    if (cuota.importePagado > 0 && cuota.importePagado < cuota.importe) {
      return { backgroundColor: 'rgba(255, 193, 7, 0.2)', color: '#856404' };
    }
    // Pendiente
    return { backgroundColor: 'rgba(220, 53, 69, 0.1)', color: '#dc3545' };
  };

  const meses = [
    { value: 1, label: 'Enero' },
    { value: 2, label: 'Febrero' },
    { value: 3, label: 'Marzo' },
    { value: 4, label: 'Abril' },
    { value: 5, label: 'Mayo' },
    { value: 6, label: 'Junio' },
    { value: 7, label: 'Julio' },
    { value: 8, label: 'Agosto' },
    { value: 9, label: 'Septiembre' },
    { value: 10, label: 'Octubre' },
    { value: 11, label: 'Noviembre' },
    { value: 12, label: 'Diciembre' },
  ];

  const anios = Array.from({ length: 10 }, (_, i) => now.getFullYear() - 5 + i);

  return (
    <div>
      <PageHeader
        title="Listado de Deudores"
        icon="fa-solid fa-file-invoice-dollar"
      />

      {error && <div className="alert alert-danger">{error}</div>}

      {/* Filtros */}
      <div className="card mb-4">
        <div className="card-body">
          <div className="row align-items-end">
            <div className="col-md-3 mb-3 mb-md-0">
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
            <div className="col-md-2 mb-3 mb-md-0">
              <label className="form-label">Mes</label>
              <select
                className="form-select"
                value={mes}
                onChange={(e) => setMes(parseInt(e.target.value, 10))}
              >
                {meses.map((m) => (
                  <option key={m.value} value={m.value}>
                    {m.label}
                  </option>
                ))}
              </select>
            </div>
            <div className="col-md-2 mb-3 mb-md-0">
              <label className="form-label">Año</label>
              <select
                className="form-select"
                value={anio}
                onChange={(e) => setAnio(parseInt(e.target.value, 10))}
              >
                {anios.map((a) => (
                  <option key={a} value={a}>
                    {a}
                  </option>
                ))}
              </select>
            </div>
            <div className="col-md-3">
              <GradientButton
                icon={loading ? 'fa-solid fa-spinner fa-spin' : 'fa-solid fa-search'}
                onClick={handleBuscar}
                disabled={loading}
              >
                {loading ? 'Buscando...' : 'Buscar'}
              </GradientButton>
            </div>
          </div>
        </div>
      </div>

      {/* Tabla de resultados */}
      {loading ? (
        <div className="text-center py-5">
          <div className="spinner-gradient" />
        </div>
      ) : reporte ? (
        <div className="card">
          <div className="card-body">
            <div className="table-responsive">
              <table className="custom-table" style={{ fontSize: '0.85rem' }}>
                <thead>
                  <tr>
                    <th style={{ minWidth: '120px' }}>Nº Comprobante</th>
                    <th style={{ minWidth: '180px' }}>Razón Social</th>
                    <th style={{ minWidth: '80px' }}>Vendedor</th>
                    <th style={{ minWidth: '60px' }} className="text-center">Cuotas</th>
                    <th style={{ minWidth: '100px' }} className="text-end">Total</th>
                    <th style={{ minWidth: '100px' }} className="text-end">Saldo</th>
                    <th style={{ minWidth: '90px' }} className="text-end">Anticipo</th>
                    {reporte.periodosCuotas.map((periodo) => (
                      <th key={periodo} style={{ minWidth: '90px' }} className="text-end">
                        {periodo}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {reporte.deudores.length === 0 ? (
                    <tr>
                      <td colSpan={8 + reporte.periodosCuotas.length} className="text-center py-4">
                        No hay comprobantes para el período seleccionado
                      </td>
                    </tr>
                  ) : (
                    reporte.deudores.map((deudor) => (
                      <tr key={deudor.comprobanteId}>
                        <td>{deudor.numeroComprobante || '-'}</td>
                        <td>{deudor.razonSocial}</td>
                        <td className="text-center">{deudor.codigoVendedor || '-'}</td>
                        <td className="text-center">{deudor.cantidadCuotas}</td>
                        <td className="text-end">{formatCurrency(deudor.totalComprobante)}</td>
                        <td className="text-end" style={{
                          fontWeight: 'bold',
                          color: deudor.saldo > 0 ? '#dc3545' : '#198754'
                        }}>
                          {formatCurrency(deudor.saldo)}
                        </td>
                        <td className="text-end">{formatCurrency(deudor.anticipo)}</td>
                        {reporte.periodosCuotas.map((periodo) => {
                          const value = getCuotaValue(deudor, periodo);
                          const style = getCuotaStyle(deudor, periodo);

                          return (
                            <td
                              key={`${deudor.comprobanteId}-${periodo}`}
                              className="text-end"
                              style={style}
                            >
                              {value !== null ? formatCurrency(value) : '-'}
                            </td>
                          );
                        })}
                      </tr>
                    ))
                  )}
                </tbody>
                {reporte.deudores.length > 0 && (
                  <tfoot>
                    <tr style={{ fontWeight: 'bold', backgroundColor: 'rgba(0,0,0,0.05)' }}>
                      <td colSpan={4}>TOTALES</td>
                      <td className="text-end">
                        {formatCurrency(reporte.deudores.reduce((sum, d) => sum + d.totalComprobante, 0))}
                      </td>
                      <td className="text-end" style={{ color: '#dc3545' }}>
                        {formatCurrency(reporte.deudores.reduce((sum, d) => sum + d.saldo, 0))}
                      </td>
                      <td className="text-end">
                        {formatCurrency(reporte.deudores.reduce((sum, d) => sum + d.anticipo, 0))}
                      </td>
                      {reporte.periodosCuotas.map((periodo) => {
                        const total = reporte.deudores.reduce((sum, deudor) => {
                          const value = getCuotaValue(deudor, periodo);
                          return sum + (value || 0);
                        }, 0);

                        return (
                          <td key={`total-${periodo}`} className="text-end">
                            {formatCurrency(total)}
                          </td>
                        );
                      })}
                    </tr>
                  </tfoot>
                )}
              </table>
            </div>

            {/* Leyenda */}
            <div className="mt-3 d-flex gap-4">
              <div className="d-flex align-items-center">
                <span
                  className="me-2"
                  style={{
                    display: 'inline-block',
                    width: '16px',
                    height: '16px',
                    backgroundColor: 'rgba(25, 135, 84, 0.3)',
                    borderRadius: '3px'
                  }}
                ></span>
                <small>Cuota pagada</small>
              </div>
              <div className="d-flex align-items-center">
                <span
                  className="me-2"
                  style={{
                    display: 'inline-block',
                    width: '16px',
                    height: '16px',
                    backgroundColor: 'rgba(255, 193, 7, 0.4)',
                    borderRadius: '3px'
                  }}
                ></span>
                <small>Cuota parcial</small>
              </div>
              <div className="d-flex align-items-center">
                <span
                  className="me-2"
                  style={{
                    display: 'inline-block',
                    width: '16px',
                    height: '16px',
                    backgroundColor: 'rgba(220, 53, 69, 0.3)',
                    borderRadius: '3px'
                  }}
                ></span>
                <small>Cuota pendiente</small>
              </div>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  );
};

export default DeudoresPage;
