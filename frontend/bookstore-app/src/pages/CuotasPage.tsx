import { useState, useEffect, useRef, useMemo } from 'react';
import { cuotaService } from '../services/cuotaService';
import { referenceService } from '../services/referenceService';
import type { CuotaListado } from '../types/cuota';
import type { Zona } from '../types/references';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';

const MESES = [
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

const CuotasPage = () => {
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [cuotas, setCuotas] = useState<CuotaListado[]>([]);
  const [zonas, setZonas] = useState<Zona[]>([]);
  const [zonaId, setZonaId] = useState<number | ''>('');
  const [mes, setMes] = useState<number | ''>(new Date().getMonth() + 1);
  const [anio, setAnio] = useState<number | ''>(new Date().getFullYear());
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editValue, setEditValue] = useState<string>('');
  const [mostrarPagadas, setMostrarPagadas] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  const [anioInput, setAnioInput] = useState<string>(anio.toString());

  useEffect(() => {
    loadZonas();
  }, []);

  useEffect(() => {
    loadCuotas();
  }, [zonaId, mes, anio]);

  useEffect(() => {
    if (editingId !== null && inputRef.current) {
      inputRef.current.focus();
      inputRef.current.select();
    }
  }, [editingId]);

  const loadZonas = async () => {
    try {
      const data = await referenceService.getZonas();
      setZonas(data);
    } catch (err) {
      console.error('Error loading zonas:', err);
    }
  };

  const loadCuotas = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await cuotaService.getAll({
        zonaId: zonaId || undefined,
        mes: mes || undefined,
        anio: anio || undefined
      });
      setCuotas(data);
    } catch (err) {
      setError('Error al cargar las cuotas');
      console.error('Error loading cuotas:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleFocus = (cuota: CuotaListado) => {
    setEditingId(cuota.id);
    // Auto-completar con el importe si importePagado es 0
    if (cuota.importePagado === 0) {
      setEditValue(cuota.importe.toString());
    } else {
      setEditValue(cuota.importePagado.toString());
    }
  };

  const handleBlur = () => {
    setEditingId(null);
    setEditValue('');
  };

  const handleKeyDown = async (e: React.KeyboardEvent, cuota: CuotaListado) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      const newValue = parseFloat(editValue) || 0;

      if (newValue !== cuota.importePagado) {
        try {
          setSaving(cuota.id);
          await cuotaService.updateImportePagado(cuota.id, {
            importePagado: newValue
          });

          // Actualizar el estado local
          setCuotas(prev => prev.map(c =>
            c.id === cuota.id
              ? {
                  ...c,
                  importePagado: newValue,
                  estado: newValue >= c.importe ? 'PAG' : (newValue > 0 ? 'PAR' : 'PEN')
                }
              : c
          ));
        } catch (err) {
          setError('Error al guardar el importe pagado');
          console.error('Error updating importe pagado:', err);
        } finally {
          setSaving(null);
        }
      }

      setEditingId(null);
      setEditValue('');

      // Mover al siguiente input
      const currentIndex = cuotasFiltradas.findIndex(c => c.id === cuota.id);
      if (currentIndex < cuotasFiltradas.length - 1) {
        const nextCuota = cuotasFiltradas[currentIndex + 1];
        setTimeout(() => handleFocus(nextCuota), 50);
      }
    } else if (e.key === 'Escape') {
      setEditingId(null);
      setEditValue('');
    }
  };

  const cuotasFiltradas = useMemo(() => {
    if (mostrarPagadas) return cuotas;
    return cuotas.filter(c => c.estado !== 'PAG' && c.importePagado < c.importe);
  }, [cuotas, mostrarPagadas]);

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('es-AR', {
      style: 'currency',
      currency: 'ARS',
      minimumFractionDigits: 2,
    }).format(value);
  };

  const formatDate = (dateStr?: string) => {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString('es-AR');
  };

  const getEstadoStyle = (cuota: CuotaListado) => {
    if (cuota.estado === 'PAG' || cuota.importePagado >= cuota.importe) {
      return { backgroundColor: 'rgba(25, 135, 84, 0.1)', color: '#198754' };
    }
    if (cuota.importePagado > 0 && cuota.importePagado < cuota.importe) {
      return { backgroundColor: 'rgba(255, 193, 7, 0.2)', color: '#856404' };
    }
    return { backgroundColor: 'rgba(220, 53, 69, 0.1)', color: '#dc3545' };
  };

  const getEstadoLabel = (cuota: CuotaListado) => {
    if (cuota.estado === 'PAG' || cuota.importePagado >= cuota.importe) {
      return 'Pagada';
    }
    if (cuota.importePagado > 0 && cuota.importePagado < cuota.importe) {
      return 'Parcial';
    }
    return 'Pendiente';
  };

  return (
    <div>
      <PageHeader
        title="Listado de Cuotas"
        icon="fa-solid fa-money-check-dollar"
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
                onChange={(e) => setMes(e.target.value ? parseInt(e.target.value, 10) : '')}
              >
                <option value="">Todos</option>
                {MESES.map((m) => (
                  <option key={m.value} value={m.value}>
                    {m.label}
                  </option>
                ))}
              </select>
            </div>
            <div className="col-md-2 mb-3 mb-md-0">
              <label className="form-label">Año</label>
              <input
                type="number"
                className="form-control"
                placeholder="Ej: 2026"
                value={anioInput}
                onChange={(e) => setAnioInput(e.target.value)}
                onBlur={() => {
                  const parsed = parseInt(anioInput, 10);
                  setAnio(parsed && parsed >= 2000 ? parsed : '');
                }}
                onKeyDown={(e) => {
                  if (e.key === 'Enter') {
                    const parsed = parseInt(anioInput, 10);
                    setAnio(parsed && parsed >= 2000 ? parsed : '');
                  }
                }}
              />
            </div>
            <div className="col-md-2">
              <GradientButton
                icon={loading ? 'fa-solid fa-spinner fa-spin' : 'fa-solid fa-sync'}
                onClick={loadCuotas}
                disabled={loading}
              >
                {loading ? 'Cargando...' : 'Actualizar'}
              </GradientButton>
            </div>
            <div className="col-md-3 d-flex align-items-end">
              <div className="form-check">
                <input
                  className="form-check-input"
                  type="checkbox"
                  id="mostrarPagadas"
                  checked={mostrarPagadas}
                  onChange={(e) => setMostrarPagadas(e.target.checked)}
                />
                <label className="form-check-label" htmlFor="mostrarPagadas">
                  Mostrar pagadas
                </label>
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
      ) : (
        <div className="card">
          <div className="card-body">
            <div className="table-responsive">
              <table className="custom-table" style={{ fontSize: '0.85rem' }}>
                <thead>
                  <tr>
                    <th style={{ minWidth: '100px' }}>Fecha Cuota</th>
                    <th style={{ minWidth: '120px' }}>Nº Comprobante</th>
                    <th style={{ minWidth: '100px' }}>Fecha Comp.</th>
                    <th style={{ minWidth: '200px' }}>Cliente</th>
                    <th style={{ minWidth: '100px' }}>Zona</th>
                    <th style={{ minWidth: '100px' }} className="text-end">Importe</th>
                    <th style={{ minWidth: '120px' }} className="text-end">Importe Pagado</th>
                    <th style={{ minWidth: '80px' }} className="text-center">Estado</th>
                  </tr>
                </thead>
                <tbody>
                  {cuotasFiltradas.length === 0 ? (
                    <tr>
                      <td colSpan={8} className="text-center py-4">
                        No hay cuotas para mostrar
                      </td>
                    </tr>
                  ) : (
                    cuotasFiltradas.map((cuota) => (
                      <tr key={cuota.id}>
                        <td>
                          {formatDate(cuota.fechaCuota)}
                          {cuota.esCuotaCero && (
                            <span
                              className="badge ms-2"
                              style={{
                                backgroundColor: 'rgba(108, 117, 125, 0.2)',
                                color: '#6c757d',
                                fontSize: '0.7rem'
                              }}
                            >
                              C.Entrega
                            </span>
                          )}
                        </td>
                        <td>{cuota.numeroComprobante || '-'}</td>
                        <td>{formatDate(cuota.fechaComprobante)}</td>
                        <td>{cuota.clienteNombre}</td>
                        <td>{cuota.zonaNombre || '-'}</td>
                        <td className="text-end">{formatCurrency(cuota.importe)}</td>
                        <td className="text-end">
                          {editingId === cuota.id ? (
                            <input
                              ref={inputRef}
                              type="number"
                              className="form-control form-control-sm text-end"
                              style={{ width: '120px', marginLeft: 'auto' }}
                              value={editValue}
                              onChange={(e) => setEditValue(e.target.value)}
                              onFocus={(e) => e.target.select()}
                              onBlur={handleBlur}
                              onKeyDown={(e) => handleKeyDown(e, cuota)}
                              step="0.01"
                              disabled={saving === cuota.id}
                            />
                          ) : (
                            <span
                              onClick={() => handleFocus(cuota)}
                              style={{
                                cursor: 'pointer',
                                padding: '4px 8px',
                                borderRadius: '4px',
                                display: 'inline-block',
                                minWidth: '80px',
                                border: '1px dashed #ccc'
                              }}
                              title="Click para editar"
                            >
                              {saving === cuota.id ? (
                                <i className="fa-solid fa-spinner fa-spin" />
                              ) : (
                                formatCurrency(cuota.importePagado)
                              )}
                            </span>
                          )}
                        </td>
                        <td className="text-center">
                          <span
                            className="badge"
                            style={{
                              ...getEstadoStyle(cuota),
                              padding: '6px 12px',
                              fontWeight: 500
                            }}
                          >
                            {getEstadoLabel(cuota)}
                          </span>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
                {cuotasFiltradas.length > 0 && (
                  <tfoot>
                    <tr style={{ fontWeight: 'bold', backgroundColor: 'rgba(0,0,0,0.05)' }}>
                      <td colSpan={5}>TOTALES ({cuotasFiltradas.length} cuotas)</td>
                      <td className="text-end">
                        {formatCurrency(cuotasFiltradas.reduce((sum, c) => sum + c.importe, 0))}
                      </td>
                      <td className="text-end">
                        {formatCurrency(cuotasFiltradas.reduce((sum, c) => sum + c.importePagado, 0))}
                      </td>
                      <td></td>
                    </tr>
                  </tfoot>
                )}
              </table>
            </div>

            {/* Instrucciones */}
            <div className="mt-3 text-muted small">
              <i className="fa-solid fa-info-circle me-2"></i>
              Click en el importe pagado para editar. Presione Enter para guardar o Escape para cancelar.
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default CuotasPage;
