import { useState, useEffect } from 'react';
import { cuotaProveedorService } from '../services/cuotaProveedorService';
import { proveedorService } from '../services/proveedorService';
import type { CuotaProveedorListado, CreatePagoCuotaProveedorDto } from '../types/cuotaProveedor';
import type { Proveedor } from '../types/proveedor';
import { showSuccessAlert, showErrorAlert, showDeleteConfirmDialog } from '../utils/sweetalert';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { IconButton } from '../components/IconButton';
import { Icon } from '../components/Icon';

const tipoLabels: Record<string, { label: string; className: string }> = {
  FC: { label: 'FC', className: 'bg-primary' },
  NC: { label: 'NC', className: 'bg-success' },
  ND: { label: 'ND', className: 'bg-danger' },
};

const CuotasProveedoresPage = () => {
  const [cuotas, setCuotas] = useState<CuotaProveedorListado[]>([]);
  const [proveedores, setProveedores] = useState<Proveedor[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Filtros
  const [proveedorId, setProveedorId] = useState<number | ''>('');
  const [fechaDesde, setFechaDesde] = useState('');
  const [fechaHasta, setFechaHasta] = useState('');
  const [estadoFiltro, setEstadoFiltro] = useState('');

  // Modal pago
  const [showModal, setShowModal] = useState(false);
  const [cuotaSeleccionada, setCuotaSeleccionada] = useState<CuotaProveedorListado | null>(null);
  const [pagoForm, setPagoForm] = useState<CreatePagoCuotaProveedorDto>({
    nroReferencia: '',
    fecha: new Date().toISOString().split('T')[0],
    importe: 0,
  });
  const [savingPago, setSavingPago] = useState(false);

  // Expandir filas para ver pagos
  const [expandedRows, setExpandedRows] = useState<Set<number>>(new Set());

  useEffect(() => {
    loadProveedores();
    loadCuotas();
  }, []);

  const loadProveedores = async () => {
    try {
      const data = await proveedorService.getAll();
      setProveedores(data);
    } catch (err) {
      console.error('Error loading proveedores:', err);
    }
  };

  const loadCuotas = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await cuotaProveedorService.getAll({
        proveedorId: proveedorId || undefined,
        fechaDesde: fechaDesde || undefined,
        fechaHasta: fechaHasta || undefined,
        estado: estadoFiltro || undefined,
      });
      setCuotas(data);
    } catch (err) {
      setError('Error al cargar las cuotas de proveedores.');
      console.error('Error loading cuotas proveedores:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleBuscar = () => {
    loadCuotas();
  };

  const handleOpenPago = (cuota: CuotaProveedorListado) => {
    setCuotaSeleccionada(cuota);
    const saldo = cuota.importe - cuota.importePagado;
    setPagoForm({
      nroReferencia: '',
      fecha: new Date().toISOString().split('T')[0],
      importe: Math.round(saldo * 100) / 100,
    });
    setShowModal(true);
  };

  const handleClosePago = () => {
    setShowModal(false);
    setCuotaSeleccionada(null);
  };

  const handleSavePago = async () => {
    if (!cuotaSeleccionada) return;

    if (!pagoForm.nroReferencia.trim()) {
      await showErrorAlert('Error', 'El Nro. de Referencia es obligatorio');
      return;
    }
    if (pagoForm.importe <= 0) {
      await showErrorAlert('Error', 'El importe debe ser mayor a 0');
      return;
    }

    try {
      setSavingPago(true);
      await cuotaProveedorService.createPago(cuotaSeleccionada.id, pagoForm);
      await showSuccessAlert('Pago registrado', 'El pago ha sido registrado correctamente');
      handleClosePago();
      await loadCuotas();
    } catch (err) {
      await showErrorAlert('Error', 'No se pudo registrar el pago');
      console.error('Error creating pago:', err);
    } finally {
      setSavingPago(false);
    }
  };

  const handleDeletePago = async (pagoId: number) => {
    const result = await showDeleteConfirmDialog('este pago');
    if (!result.isConfirmed) return;

    try {
      await cuotaProveedorService.deletePago(pagoId);
      await showSuccessAlert('Pago eliminado', 'El pago ha sido eliminado correctamente');
      await loadCuotas();
    } catch (err) {
      await showErrorAlert('Error', 'No se pudo eliminar el pago');
      console.error('Error deleting pago:', err);
    }
  };

  const toggleExpand = (cuotaId: number) => {
    setExpandedRows((prev) => {
      const next = new Set(prev);
      if (next.has(cuotaId)) {
        next.delete(cuotaId);
      } else {
        next.add(cuotaId);
      }
      return next;
    });
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

  const getEstadoBadge = (estado: string) => {
    switch (estado) {
      case 'PAG':
        return <span className="badge bg-success">Pagada</span>;
      case 'PAR':
        return <span className="badge bg-warning text-dark">Parcial</span>;
      default:
        return <span className="badge bg-danger">Pendiente</span>;
    }
  };

  const totales = {
    importe: cuotas.reduce((s, c) => s + c.importe, 0),
    pagado: cuotas.reduce((s, c) => s + c.importePagado, 0),
  };

  return (
    <div>
      <PageHeader
        title="Cuotas de Proveedores"
        icon="fa-solid fa-money-check-dollar"
      />

      {error && <div className="alert alert-danger">{error}</div>}

      {/* Filtros */}
      <div className="card mb-4">
        <div className="card-body">
          <div className="row align-items-end">
            <div className="col-md-3 mb-3 mb-md-0">
              <label className="form-label">Proveedor</label>
              <select
                className="form-select"
                value={proveedorId}
                onChange={(e) => setProveedorId(e.target.value ? parseInt(e.target.value) : '')}
              >
                <option value="">Todos</option>
                {proveedores.map((p) => (
                  <option key={p.id} value={p.id}>{p.nombre}</option>
                ))}
              </select>
            </div>
            <div className="col-md-2 mb-3 mb-md-0">
              <label className="form-label">Desde</label>
              <input
                type="date"
                className="form-control"
                value={fechaDesde}
                onChange={(e) => setFechaDesde(e.target.value)}
              />
            </div>
            <div className="col-md-2 mb-3 mb-md-0">
              <label className="form-label">Hasta</label>
              <input
                type="date"
                className="form-control"
                value={fechaHasta}
                onChange={(e) => setFechaHasta(e.target.value)}
              />
            </div>
            <div className="col-md-2 mb-3 mb-md-0">
              <label className="form-label">Estado</label>
              <select
                className="form-select"
                value={estadoFiltro}
                onChange={(e) => setEstadoFiltro(e.target.value)}
              >
                <option value="">Todos</option>
                <option value="PEN">Pendiente</option>
                <option value="PAR">Parcial</option>
                <option value="PAG">Pagada</option>
              </select>
            </div>
            <div className="col-md-2">
              <GradientButton
                icon={loading ? 'fa-solid fa-spinner fa-spin' : 'fa-solid fa-search'}
                onClick={handleBuscar}
                disabled={loading}
              >
                Buscar
              </GradientButton>
            </div>
          </div>
        </div>
      </div>

      {/* Tabla */}
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
                    <th>Vencimiento</th>
                    <th>Tipo</th>
                    <th>Nro. Comprobante</th>
                    <th>Proveedor</th>
                    <th>Cuota</th>
                    <th className="text-end">Importe</th>
                    <th className="text-end">Pagado</th>
                    <th className="text-end">Saldo</th>
                    <th className="text-center">Estado</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {cuotas.length === 0 ? (
                    <tr>
                      <td colSpan={11} className="text-center py-4 text-muted">
                        No hay cuotas para mostrar
                      </td>
                    </tr>
                  ) : (
                    cuotas.map((cuota) => {
                      const tipo = tipoLabels[cuota.tipoComprobante] || { label: cuota.tipoComprobante, className: 'bg-secondary' };
                      const saldo = cuota.importe - cuota.importePagado;
                      const isExpanded = expandedRows.has(cuota.id);
                      const hasPagos = cuota.pagos && cuota.pagos.length > 0;

                      return (
                        <>
                          <tr key={cuota.id}>
                            <td>
                              {hasPagos && (
                                <span
                                  style={{ cursor: 'pointer' }}
                                  onClick={() => toggleExpand(cuota.id)}
                                  title={isExpanded ? 'Colapsar pagos' : 'Ver pagos'}
                                >
                                  <Icon name={isExpanded ? 'fa-solid fa-chevron-down' : 'fa-solid fa-chevron-right'} />
                                </span>
                              )}
                            </td>
                            <td>{formatDate(cuota.fechaVencimiento)}</td>
                            <td>
                              <span className={`badge ${tipo.className}`}>{tipo.label}</span>
                            </td>
                            <td>{cuota.nroComprobante}</td>
                            <td>{cuota.proveedorNombre}</td>
                            <td className="text-center">{cuota.numeroCuota}</td>
                            <td className="text-end">{formatCurrency(cuota.importe)}</td>
                            <td className="text-end">{formatCurrency(cuota.importePagado)}</td>
                            <td className="text-end">{formatCurrency(saldo)}</td>
                            <td className="text-center">{getEstadoBadge(cuota.estado)}</td>
                            <td>
                              {cuota.estado !== 'PAG' && (
                                <IconButton
                                  icon="fa-solid fa-dollar-sign"
                                  title="Registrar pago"
                                  variant="success"
                                  onClick={() => handleOpenPago(cuota)}
                                />
                              )}
                            </td>
                          </tr>
                          {isExpanded && hasPagos && (
                            <tr key={`pagos-${cuota.id}`}>
                              <td colSpan={11} style={{ padding: 0, backgroundColor: 'rgba(0,0,0,0.02)' }}>
                                <div style={{ padding: '0.5rem 1rem 0.5rem 3rem' }}>
                                  <table className="table table-sm mb-0" style={{ fontSize: '0.8rem' }}>
                                    <thead>
                                      <tr>
                                        <th>Nro. Referencia</th>
                                        <th>Fecha</th>
                                        <th className="text-end">Importe</th>
                                        <th style={{ width: '60px' }}></th>
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
                    })
                  )}
                </tbody>
                {cuotas.length > 0 && (
                  <tfoot>
                    <tr style={{ fontWeight: 'bold', backgroundColor: 'rgba(0,0,0,0.05)' }}>
                      <td colSpan={6}>TOTALES ({cuotas.length} cuotas)</td>
                      <td className="text-end">{formatCurrency(totales.importe)}</td>
                      <td className="text-end">{formatCurrency(totales.pagado)}</td>
                      <td className="text-end">{formatCurrency(totales.importe - totales.pagado)}</td>
                      <td colSpan={2}></td>
                    </tr>
                  </tfoot>
                )}
              </table>
            </div>
          </div>
        </div>
      )}

      {/* Modal Pago */}
      {showModal && cuotaSeleccionada && (
        <div className="modal d-block" tabIndex={-1} style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">
                  <Icon name="fa-solid fa-dollar-sign" /> Registrar Pago
                </h5>
                <button type="button" className="btn-close" onClick={handleClosePago}></button>
              </div>
              <div className="modal-body">
                <div className="mb-3 p-3" style={{ backgroundColor: 'rgba(0,0,0,0.03)', borderRadius: '8px' }}>
                  <div className="row">
                    <div className="col-6">
                      <small className="text-muted">Proveedor</small>
                      <div className="fw-bold">{cuotaSeleccionada.proveedorNombre}</div>
                    </div>
                    <div className="col-6">
                      <small className="text-muted">Comprobante</small>
                      <div className="fw-bold">{cuotaSeleccionada.nroComprobante}</div>
                    </div>
                    <div className="col-4 mt-2">
                      <small className="text-muted">Cuota</small>
                      <div>{cuotaSeleccionada.numeroCuota}</div>
                    </div>
                    <div className="col-4 mt-2">
                      <small className="text-muted">Importe Cuota</small>
                      <div>{formatCurrency(cuotaSeleccionada.importe)}</div>
                    </div>
                    <div className="col-4 mt-2">
                      <small className="text-muted">Saldo</small>
                      <div className="fw-bold text-danger">
                        {formatCurrency(cuotaSeleccionada.importe - cuotaSeleccionada.importePagado)}
                      </div>
                    </div>
                  </div>
                </div>

                <div className="mb-3">
                  <label className="form-label">Nro. Referencia <span className="text-danger">*</span></label>
                  <input
                    type="text"
                    className="form-control"
                    value={pagoForm.nroReferencia}
                    onChange={(e) => setPagoForm({ ...pagoForm, nroReferencia: e.target.value })}
                    placeholder="Ej: TRF-001, CHQ-123"
                    autoFocus
                  />
                </div>
                <div className="mb-3">
                  <label className="form-label">Fecha <span className="text-danger">*</span></label>
                  <input
                    type="date"
                    className="form-control"
                    value={pagoForm.fecha}
                    onChange={(e) => setPagoForm({ ...pagoForm, fecha: e.target.value })}
                  />
                </div>
                <div className="mb-3">
                  <label className="form-label">Importe <span className="text-danger">*</span></label>
                  <div className="input-group">
                    <span className="input-group-text">$</span>
                    <input
                      type="number"
                      className="form-control"
                      value={pagoForm.importe}
                      onChange={(e) => setPagoForm({ ...pagoForm, importe: parseFloat(e.target.value) || 0 })}
                      onFocus={(e) => e.target.select()}
                      min="0.01"
                      step="0.01"
                    />
                  </div>
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={handleClosePago}>
                  Cancelar
                </button>
                <button
                  type="button"
                  className="btn btn-success"
                  onClick={handleSavePago}
                  disabled={savingPago}
                >
                  {savingPago ? (
                    <>
                      <span className="spinner-border spinner-border-sm me-2" />
                      Guardando...
                    </>
                  ) : (
                    <>
                      <Icon name="fa-solid fa-check" /> Registrar Pago
                    </>
                  )}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default CuotasProveedoresPage;
