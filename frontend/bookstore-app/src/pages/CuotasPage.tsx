import { useState, useEffect, useMemo, useRef } from 'react';
import { cuotaService } from '../services/cuotaService';
import { referenceService } from '../services/referenceService';
import type { CuotaListado } from '../types/cuota';
import type { Zona } from '../types/references';
import { showSuccessAlert, showErrorAlert, showDeleteConfirmDialog } from '../utils/sweetalert';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { IconButton } from '../components/IconButton';
import { Icon } from '../components/Icon';

interface ComprobanteGroup {
  comprobanteId: number;
  numeroComprobante: string;
  fechaComprobante?: string;
  clienteNombre: string;
  zonaNombre?: string;
  cantidadCuotas: number;
  importeTotal: number;
  importePagado: number;
  saldo: number;
  cuotas: CuotaListado[];
}

const CuotasPage = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [cuotas, setCuotas] = useState<CuotaListado[]>([]);
  const [zonas, setZonas] = useState<Zona[]>([]);
  const [zonaId, setZonaId] = useState<number | ''>('');
  const [fechaCorte, setFechaCorte] = useState<string>(new Date().toISOString().split('T')[0]);
  const [mostrarPagadas, setMostrarPagadas] = useState(false);

  // Pago inline a nivel comprobante
  const [editingComprobanteId, setEditingComprobanteId] = useState<number | null>(null);
  const [editValue, setEditValue] = useState<string>('');
  const [fechaPago, setFechaPago] = useState<string>('');
  const [savingComprobanteId, setSavingComprobanteId] = useState<number | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const pendingNextRef = useRef<number | null>(null); // comprobanteId del que se acaba de pagar, para saltar al siguiente

  // Expandir filas
  const [expandedComprobantes, setExpandedComprobantes] = useState<Set<number>>(new Set());
  const [expandedCuotas, setExpandedCuotas] = useState<Set<number>>(new Set());

  useEffect(() => {
    loadZonas();
  }, []);

  useEffect(() => {
    loadCuotas();
  }, [zonaId, fechaCorte]);

  useEffect(() => {
    if (editingComprobanteId !== null && inputRef.current) {
      inputRef.current.focus();
      inputRef.current.select();
    }
  }, [editingComprobanteId]);

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
        fechaCorte: fechaCorte || undefined,
      });
      setCuotas(data);
    } catch (err) {
      setError('Error al cargar las cuotas');
      console.error('Error loading cuotas:', err);
    } finally {
      setLoading(false);
    }
  };

  const cuotasFiltradas = useMemo(() => {
    if (mostrarPagadas) return cuotas;
    return cuotas.filter(c => c.estado !== 'PAG' && c.importePagado < c.importe);
  }, [cuotas, mostrarPagadas]);

  const comprobanteGroups = useMemo((): ComprobanteGroup[] => {
    const map = new Map<number, ComprobanteGroup>();
    for (const cuota of cuotasFiltradas) {
      let group = map.get(cuota.comprobanteId);
      if (!group) {
        group = {
          comprobanteId: cuota.comprobanteId,
          numeroComprobante: cuota.numeroComprobante || '-',
          fechaComprobante: cuota.fechaComprobante,
          clienteNombre: cuota.clienteNombre || '',
          zonaNombre: cuota.zonaNombre,
          cantidadCuotas: 0,
          importeTotal: 0,
          importePagado: 0,
          saldo: 0,
          cuotas: [],
        };
        map.set(cuota.comprobanteId, group);
      }
      group.cantidadCuotas++;
      group.importeTotal += cuota.importe;
      group.importePagado += cuota.importePagado;
      group.saldo += cuota.importe - cuota.importePagado;
      group.cuotas.push(cuota);
    }
    return Array.from(map.values()).sort((a, b) => a.clienteNombre.localeCompare(b.clienteNombre));
  }, [cuotasFiltradas]);

  // Saltar al siguiente comprobante con saldo después de pagar
  useEffect(() => {
    if (pendingNextRef.current === null) return;
    const paidId = pendingNextRef.current;
    pendingNextRef.current = null;

    const idx = comprobanteGroups.findIndex(g => g.comprobanteId === paidId);
    for (let i = idx === -1 ? 0 : idx; i < comprobanteGroups.length; i++) {
      const next = comprobanteGroups[i];
      if (next.saldo > 0) {
        setEditingComprobanteId(next.comprobanteId);
        setEditValue(next.saldo.toFixed(2));
        return;
      }
    }
    setEditingComprobanteId(null);
    setEditValue('');
  }, [comprobanteGroups]);

  const handlePagoFocus = (group: ComprobanteGroup) => {
    setEditingComprobanteId(group.comprobanteId);
    setEditValue(group.saldo.toFixed(2));
  };

  const handlePagoBlur = () => {
    // No cerrar si hay un pago pendiente de saltar al siguiente
    if (pendingNextRef.current !== null) return;
    setEditingComprobanteId(null);
    setEditValue('');
  };

  const navigateComprobante = (direction: 'up' | 'down') => {
    if (editingComprobanteId === null) return;
    const idx = comprobanteGroups.findIndex(g => g.comprobanteId === editingComprobanteId);
    if (idx === -1) return;

    const step = direction === 'down' ? 1 : -1;
    for (let i = idx + step; i >= 0 && i < comprobanteGroups.length; i += step) {
      const next = comprobanteGroups[i];
      if (next.saldo > 0) {
        setEditingComprobanteId(next.comprobanteId);
        setEditValue(next.saldo.toFixed(2));
        return;
      }
    }
  };

  const handlePagoKeyDown = async (e: React.KeyboardEvent, group: ComprobanteGroup) => {
    if (e.key === 'ArrowDown') {
      e.preventDefault();
      navigateComprobante('down');
      return;
    }
    if (e.key === 'ArrowUp') {
      e.preventDefault();
      navigateComprobante('up');
      return;
    }
    if (e.key === 'Enter') {
      e.preventDefault();
      const valor = parseFloat(editValue) || 0;
      if (valor <= 0) {
        setEditingComprobanteId(null);
        setEditValue('');
        return;
      }

      try {
        setSavingComprobanteId(group.comprobanteId);
        setEditingComprobanteId(null);
        setEditValue('');
        pendingNextRef.current = group.comprobanteId;
        await cuotaService.createPagoComprobante(group.comprobanteId, {
          nroReferencia: 'PAGO',
          importe: valor,
          fecha: fechaPago || undefined,
        });
        await loadCuotas();
      } catch (err) {
        pendingNextRef.current = null;
        await showErrorAlert('Error', 'No se pudo registrar el pago');
        console.error('Error creating pago comprobante:', err);
      } finally {
        setSavingComprobanteId(null);
      }
    } else if (e.key === 'Escape') {
      setEditingComprobanteId(null);
      setEditValue('');
    }
  };

  const handleDeletePago = async (pagoId: number) => {
    const result = await showDeleteConfirmDialog('este pago');
    if (!result.isConfirmed) return;

    try {
      await cuotaService.deletePago(pagoId);
      await showSuccessAlert('Pago eliminado', 'El pago ha sido eliminado correctamente');
      await loadCuotas();
    } catch (err) {
      await showErrorAlert('Error', 'No se pudo eliminar el pago');
      console.error('Error deleting pago:', err);
    }
  };

  const toggleComprobante = (comprobanteId: number) => {
    setExpandedComprobantes((prev) => {
      const next = new Set(prev);
      if (next.has(comprobanteId)) {
        next.delete(comprobanteId);
      } else {
        next.add(comprobanteId);
      }
      return next;
    });
  };

  const toggleCuota = (cuotaId: number) => {
    setExpandedCuotas((prev) => {
      const next = new Set(prev);
      if (next.has(cuotaId)) {
        next.delete(cuotaId);
      } else {
        next.add(cuotaId);
      }
      return next;
    });
  };

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

  const getEstadoBadge = (estado?: string, importe?: number, pagado?: number) => {
    const imp = importe ?? 0;
    const pag = pagado ?? 0;
    if (estado === 'PAG' || pag >= imp) {
      return <span className="badge bg-success">Pagada</span>;
    }
    if (pag > 0 && pag < imp) {
      return <span className="badge bg-warning text-dark">Parcial</span>;
    }
    return <span className="badge bg-danger">Pendiente</span>;
  };

  const totales = {
    cantCuotas: cuotasFiltradas.length,
    importe: comprobanteGroups.reduce((s, g) => s + g.importeTotal, 0),
    pagado: comprobanteGroups.reduce((s, g) => s + g.importePagado, 0),
    saldo: comprobanteGroups.reduce((s, g) => s + g.saldo, 0),
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
              <label className="form-label">Fecha de Corte</label>
              <input
                type="date"
                className="form-control"
                value={fechaCorte}
                onChange={(e) => setFechaCorte(e.target.value)}
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

      {/* Fecha de pago */}
      <div className="card mb-4">
        <div className="card-body py-2">
          <div className="row align-items-center">
            <div className="col-auto">
              <label className="form-label mb-0 fw-bold">
                <i className="fa-solid fa-calendar-check me-2"></i>Fecha de Pago
              </label>
            </div>
            <div className="col-auto">
              <input
                type="date"
                className="form-control form-control-sm"
                style={{ width: '160px' }}
                value={fechaPago}
                onChange={(e) => setFechaPago(e.target.value)}
              />
            </div>
            <div className="col-auto text-muted small">
              {fechaPago ? `Los pagos se registrarán con fecha ${new Date(fechaPago + 'T00:00:00').toLocaleDateString('es-AR')}` : 'Sin fecha: se usará la fecha del día'}
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
                    <th style={{ width: '30px' }}></th>
                    <th style={{ minWidth: '120px' }}>Nro Comprobante</th>
                    <th style={{ minWidth: '100px' }}>Fecha Comp.</th>
                    <th style={{ minWidth: '200px' }}>Cliente</th>
                    <th style={{ minWidth: '100px' }}>Zona</th>
                    <th style={{ minWidth: '80px' }} className="text-center">Cuotas</th>
                    <th style={{ minWidth: '120px' }} className="text-end">Importe</th>
                    <th style={{ minWidth: '120px' }} className="text-end">Pagado</th>
                    <th style={{ minWidth: '120px' }} className="text-end">Saldo</th>
                    <th style={{ minWidth: '140px' }} className="text-end">Pagar</th>
                  </tr>
                </thead>
                <tbody>
                  {comprobanteGroups.length === 0 ? (
                    <tr>
                      <td colSpan={10} className="text-center py-4">
                        No hay cuotas para mostrar
                      </td>
                    </tr>
                  ) : (
                    comprobanteGroups.map((group) => {
                      const isExpanded = expandedComprobantes.has(group.comprobanteId);
                      const isEditing = editingComprobanteId === group.comprobanteId;
                      const isSaving = savingComprobanteId === group.comprobanteId;

                      return (
                        <>
                          <tr key={`comp-${group.comprobanteId}`}>
                            <td
                              style={{ cursor: 'pointer' }}
                              onClick={() => toggleComprobante(group.comprobanteId)}
                            >
                              <Icon name={isExpanded ? 'fa-solid fa-chevron-down' : 'fa-solid fa-chevron-right'} />
                            </td>
                            <td
                              className="fw-bold"
                              style={{ cursor: 'pointer' }}
                              onClick={() => toggleComprobante(group.comprobanteId)}
                            >
                              {group.numeroComprobante}
                            </td>
                            <td>{formatDate(group.fechaComprobante)}</td>
                            <td>{group.clienteNombre}</td>
                            <td>{group.zonaNombre || '-'}</td>
                            <td className="text-center">
                              <span className="badge bg-secondary">{group.cantidadCuotas}</span>
                            </td>
                            <td className="text-end">{formatCurrency(group.importeTotal)}</td>
                            <td className="text-end">{formatCurrency(group.importePagado)}</td>
                            <td className="text-end fw-bold">{formatCurrency(group.saldo)}</td>
                            <td className="text-end">
                              {group.saldo > 0 && (
                                isEditing ? (
                                  <input
                                    ref={inputRef}
                                    type="number"
                                    className="form-control form-control-sm text-end"
                                    style={{ width: '130px', marginLeft: 'auto' }}
                                    value={editValue}
                                    onChange={(e) => setEditValue(e.target.value)}
                                    onFocus={(e) => e.target.select()}
                                    onBlur={handlePagoBlur}
                                    onKeyDown={(e) => handlePagoKeyDown(e, group)}
                                    step="0.01"
                                    disabled={isSaving}
                                  />
                                ) : (
                                  <span
                                    onClick={(e) => { e.stopPropagation(); handlePagoFocus(group); }}
                                    style={{
                                      cursor: 'pointer',
                                      padding: '4px 8px',
                                      borderRadius: '4px',
                                      display: 'inline-block',
                                      minWidth: '80px',
                                      border: '1px dashed #198754',
                                      color: '#198754',
                                    }}
                                    title="Click para pagar"
                                  >
                                    {isSaving ? (
                                      <i className="fa-solid fa-spinner fa-spin" />
                                    ) : (
                                      <>
                                        <i className="fa-solid fa-dollar-sign me-1" />
                                        Pagar
                                      </>
                                    )}
                                  </span>
                                )
                              )}
                            </td>
                          </tr>

                          {isExpanded && (
                            <tr key={`comp-detail-${group.comprobanteId}`}>
                              <td colSpan={10} style={{ padding: 0, backgroundColor: 'rgba(0,0,0,0.02)' }}>
                                <div style={{ padding: '0.5rem 1rem 0.5rem 2.5rem' }}>
                                  <table className="table table-sm mb-0" style={{ fontSize: '0.82rem' }}>
                                    <thead>
                                      <tr>
                                        <th style={{ width: '30px' }}></th>
                                        <th>Cuota</th>
                                        <th>Vencimiento</th>
                                        <th className="text-end">Importe</th>
                                        <th className="text-end">Pagado</th>
                                        <th className="text-end">Saldo</th>
                                        <th className="text-center">Estado</th>
                                      </tr>
                                    </thead>
                                    <tbody>
                                      {group.cuotas.map((cuota) => {
                                        const saldo = cuota.importe - cuota.importePagado;
                                        const hasPagos = cuota.pagos && cuota.pagos.length > 0;
                                        const isCuotaExpanded = expandedCuotas.has(cuota.id);

                                        return (
                                          <>
                                            <tr key={`cuota-${cuota.id}`}>
                                              <td>
                                                {hasPagos && (
                                                  <span
                                                    style={{ cursor: 'pointer' }}
                                                    onClick={(e) => { e.stopPropagation(); toggleCuota(cuota.id); }}
                                                    title={isCuotaExpanded ? 'Colapsar pagos' : 'Ver pagos'}
                                                  >
                                                    <Icon name={isCuotaExpanded ? 'fa-solid fa-chevron-down' : 'fa-solid fa-chevron-right'} />
                                                  </span>
                                                )}
                                              </td>
                                              <td>
                                                {cuota.esCuotaCero ? (
                                                  <span className="badge" style={{ backgroundColor: 'rgba(108,117,125,0.2)', color: '#6c757d', fontSize: '0.7rem' }}>
                                                    C.Entrega
                                                  </span>
                                                ) : cuota.numeroCuota}
                                              </td>
                                              <td>{formatDate(cuota.fechaCuota)}</td>
                                              <td className="text-end">{formatCurrency(cuota.importe)}</td>
                                              <td className="text-end">{formatCurrency(cuota.importePagado)}</td>
                                              <td className="text-end">{formatCurrency(saldo)}</td>
                                              <td className="text-center">{getEstadoBadge(cuota.estado, cuota.importe, cuota.importePagado)}</td>
                                            </tr>
                                            {isCuotaExpanded && hasPagos && (
                                              <tr key={`pagos-${cuota.id}`}>
                                                <td colSpan={7} style={{ padding: 0, backgroundColor: 'rgba(0,0,0,0.03)' }}>
                                                  <div style={{ padding: '0.4rem 1rem 0.4rem 3rem' }}>
                                                    <table className="table table-sm mb-0" style={{ fontSize: '0.78rem' }}>
                                                      <thead>
                                                        <tr>
                                                          <th>Nro. Referencia</th>
                                                          <th>Fecha</th>
                                                          <th className="text-end">Importe</th>
                                                          <th style={{ width: '50px' }}></th>
                                                        </tr>
                                                      </thead>
                                                      <tbody>
                                                        {cuota.pagos.map((pago) => (
                                                          <tr key={pago.id}>
                                                            <td>{pago.nroReferencia}</td>
                                                            <td>{formatDate(pago.fecha)}</td>
                                                            <td className="text-end">{formatCurrency(pago.importe)}</td>
                                                            <td>
                                                              <IconButton
                                                                icon="fa-solid fa-trash"
                                                                title="Eliminar pago"
                                                                variant="danger"
                                                                onClick={() => handleDeletePago(pago.id)}
                                                              />
                                                            </td>
                                                          </tr>
                                                        ))}
                                                      </tbody>
                                                    </table>
                                                  </div>
                                                </td>
                                              </tr>
                                            )}
                                          </>
                                        );
                                      })}
                                    </tbody>
                                  </table>
                                </div>
                              </td>
                            </tr>
                          )}
                        </>
                      );
                    })
                  )}
                </tbody>
                {comprobanteGroups.length > 0 && (
                  <tfoot>
                    <tr style={{ fontWeight: 'bold', backgroundColor: 'rgba(0,0,0,0.05)' }}>
                      <td colSpan={5}>TOTALES ({comprobanteGroups.length} comprobantes)</td>
                      <td className="text-center">
                        <span className="badge bg-secondary">{totales.cantCuotas}</span>
                      </td>
                      <td className="text-end">{formatCurrency(totales.importe)}</td>
                      <td className="text-end">{formatCurrency(totales.pagado)}</td>
                      <td className="text-end">{formatCurrency(totales.saldo)}</td>
                      <td></td>
                    </tr>
                  </tfoot>
                )}
              </table>
            </div>

            <div className="mt-3 text-muted small">
              <i className="fa-solid fa-info-circle me-2"></i>
              Click en "Pagar" para ingresar el monto. Opcionalmente ingrese una fecha de pago (si se deja vacía se usa la fecha del día). Presione Enter para confirmar (se imputa de la cuota más vieja a la más nueva) o Escape para cancelar.
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default CuotasPage;
