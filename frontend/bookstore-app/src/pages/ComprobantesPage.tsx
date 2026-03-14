import { useState, useEffect, useMemo } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import Select from 'react-select';
import { comprobanteService, type ComprobanteFilters } from '../services/comprobanteService';
import { referenceService } from '../services/referenceService';
import { clienteService } from '../services/clienteService';
import type { Comprobante } from '../types/comprobante';
import type { Zona, Vendedor } from '../types/references';
import type { Cliente } from '../types/cliente';
import { showSuccessAlert, showErrorAlert, showConfirmDialog } from '../utils/sweetalert';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { IconButton } from '../components/IconButton';

interface SelectOption {
  value: number;
  label: string;
}

const ComprobantesPage = () => {
  const [searchParams] = useSearchParams();
  const [comprobantes, setComprobantes] = useState<Comprobante[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Data para filtros
  const [zonas, setZonas] = useState<Zona[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [vendedores, setVendedores] = useState<Vendedor[]>([]);

  // Estados de filtros
  const [selectedZona, setSelectedZona] = useState<SelectOption | null>(null);
  const [selectedCliente, setSelectedCliente] = useState<SelectOption | null>(null);
  const [selectedVendedor, setSelectedVendedor] = useState<SelectOption | null>(null);
  const [tipoComprobante, setTipoComprobante] = useState<string>('');
  const [filtroComprobante, setFiltroComprobante] = useState<string>('');
  const [fechaDesde, setFechaDesde] = useState<string>(searchParams.get('fechaDesde') || '');
  const [fechaHasta, setFechaHasta] = useState<string>(searchParams.get('fechaHasta') || '');
  const [activeFilters, setActiveFilters] = useState<ComprobanteFilters | null>(null);

  useEffect(() => {
    const initialFilters: ComprobanteFilters = {};
    const paramFechaDesde = searchParams.get('fechaDesde');
    const paramFechaHasta = searchParams.get('fechaHasta');
    if (paramFechaDesde) initialFilters.fechaDesde = paramFechaDesde;
    if (paramFechaHasta) initialFilters.fechaHasta = paramFechaHasta;

    const hasInitialFilters = Object.keys(initialFilters).length > 0;
    if (hasInitialFilters) {
      setActiveFilters(initialFilters);
      loadComprobantes(initialFilters);
    } else {
      loadComprobantes();
    }
    loadFilterData();
  }, []);

  const loadFilterData = async () => {
    try {
      const [zonasData, clientesData, vendedoresData] = await Promise.all([
        referenceService.getZonas(),
        clienteService.getAll(),
        referenceService.getVendedores()
      ]);
      setZonas(zonasData);
      setClientes(clientesData);
      setVendedores(vendedoresData);
    } catch (err) {
      console.error('Error loading filter data:', err);
    }
  };

  const loadComprobantes = async (filters?: ComprobanteFilters) => {
    try {
      setLoading(true);
      setError(null);
      const data = await comprobanteService.getAll(filters);
      setComprobantes(data);
    } catch (err) {
      setError('Error al cargar los comprobantes. Verifique que la API esté ejecutándose.');
      console.error('Error loading comprobantes:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleFilter = () => {
    const filters: ComprobanteFilters = {};
    if (selectedZona) filters.zonaId = selectedZona.value;
    if (selectedCliente) filters.clienteId = selectedCliente.value;
    if (selectedVendedor) filters.vendedorId = selectedVendedor.value;
    if (tipoComprobante) filters.tipoComprobante = tipoComprobante;
    if (filtroComprobante) filters.comprobante = filtroComprobante;
    if (fechaDesde) filters.fechaDesde = fechaDesde;
    if (fechaHasta) filters.fechaHasta = fechaHasta;

    const hasFilters = Object.keys(filters).length > 0;
    setActiveFilters(hasFilters ? filters : null);
    loadComprobantes(hasFilters ? filters : undefined);
  };

  const handleClearFilters = () => {
    setSelectedZona(null);
    setSelectedCliente(null);
    setSelectedVendedor(null);
    setTipoComprobante('');
    setFiltroComprobante('');
    setFechaDesde('');
    setFechaHasta('');
    setActiveFilters(null);
    loadComprobantes();
  };

  const handlePrintAll = () => {
    if (activeFilters) {
      comprobanteService.openBatchPdf(activeFilters);
    }
  };

  const handlePrintAllCupones = () => {
    if (activeFilters) {
      comprobanteService.openBatchCuponesPdf(activeFilters);
    }
  };

  // Opciones para React Select
  const zonaOptions = useMemo<SelectOption[]>(() =>
    zonas.map(z => ({
      value: z.id,
      label: z.descripcion || `Zona ${z.id}`
    })), [zonas]);

  const clienteOptions = useMemo<SelectOption[]>(() =>
    clientes.map(c => ({
      value: c.id,
      label: `${c.codigo || ''} - ${c.nombre}`
    })), [clientes]);

  const vendedorOptions = useMemo<SelectOption[]>(() =>
    vendedores.map(v => ({
      value: v.id,
      label: v.descripcion || `Vendedor ${v.id}`
    })), [vendedores]);

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('es-AR');
  };

  const handleCancelarClick = async (comprobante: Comprobante) => {
    const result = await showConfirmDialog(
      'Cancelar Comprobante',
      `¿Está seguro de cancelar el comprobante ${comprobante.numeroComprobante}? Esto generará una Nota de Crédito con CAE.`
    );

    if (result.isConfirmed) {
      try {
        const notaCredito = await comprobanteService.cancelar(comprobante.id);
        await showSuccessAlert(
          'Nota de Crédito Generada',
          `Se ha generado la Nota de Crédito: ${notaCredito.numeroComprobante}`
        );
        // Recargar comprobantes para mostrar la NC
        loadComprobantes();
      } catch (err: any) {
        const mensaje = err.response?.data?.error || 'Error al cancelar el comprobante';
        await showErrorAlert('Error', mensaje);
        console.error('Error canceling comprobante:', err);
      }
    }
  };

  return (
    <div>
      <PageHeader
        title="Gestión de Comprobantes"
        icon="fa-solid fa-receipt"
        actions={
          <Link to="/comprobantes/nuevo">
            <GradientButton icon="fa-solid fa-plus">
              Nuevo Comprobante
            </GradientButton>
          </Link>
        }
      />

      {error && <div className="alert alert-danger">{error}</div>}

      {/* Sección de Filtros */}
      <div className="card mb-3">
        <div className="card-body">
          {/* Fila superior: Zona, Vendedor y Cliente */}
          <div className="row g-3 mb-3">
            <div className="col-md-3">
              <label className="form-label">Zona</label>
              <Select
                isClearable
                placeholder="Seleccionar zona..."
                options={zonaOptions}
                value={selectedZona}
                onChange={(option) => setSelectedZona(option)}
              />
            </div>
            <div className="col-md-3">
              <label className="form-label">Vendedor</label>
              <Select
                isClearable
                placeholder="Seleccionar vendedor..."
                options={vendedorOptions}
                value={selectedVendedor}
                onChange={(option) => setSelectedVendedor(option)}
              />
            </div>
            <div className="col-md-6">
              <label className="form-label">Cliente</label>
              <Select
                isClearable
                placeholder="Buscar cliente..."
                options={clienteOptions}
                value={selectedCliente}
                onChange={(option) => setSelectedCliente(option)}
              />
            </div>
          </div>
          {/* Fila inferior: Tipo, Fechas y Botones */}
          <div className="row g-3">
            <div className="col-md-2">
              <label className="form-label">Tipo</label>
              <select
                className="form-select"
                value={tipoComprobante}
                onChange={(e) => setTipoComprobante(e.target.value)}
              >
                <option value="">Todos</option>
                <option value="FC">Factura (FC)</option>
                <option value="NC">Nota de Crédito (NC)</option>
                <option value="PRE">Presupuesto (PRE)</option>
              </select>
            </div>
            <div className="col-md-2">
              <label className="form-label">Comprobante</label>
              <input
                type="text"
                className="form-control"
                placeholder="Nº comprobante"
                value={filtroComprobante}
                onChange={(e) => setFiltroComprobante(e.target.value)}
              />
            </div>
            <div className="col-md-2">
              <label className="form-label">Fecha Desde</label>
              <input
                type="date"
                className="form-control"
                value={fechaDesde}
                onChange={(e) => setFechaDesde(e.target.value)}
              />
            </div>
            <div className="col-md-2">
              <label className="form-label">Fecha Hasta</label>
              <input
                type="date"
                className="form-control"
                value={fechaHasta}
                onChange={(e) => setFechaHasta(e.target.value)}
              />
            </div>
            <div className="col-md-2 d-flex align-items-end gap-2">
              <button
                className="btn btn-primary"
                onClick={handleFilter}
                title="Filtrar"
              >
                <i className="fa-solid fa-filter"></i>
              </button>
              <button
                className="btn btn-secondary"
                onClick={handleClearFilters}
                title="Limpiar filtros"
              >
                <i className="fa-solid fa-times"></i>
              </button>
              <button
                className="btn btn-success"
                onClick={handlePrintAll}
                disabled={!activeFilters}
                title={activeFilters ? "Imprimir todos los comprobantes filtrados" : "Aplique filtros para imprimir en lote"}
              >
                <i className="fa-solid fa-print"></i>
              </button>
              <button
                className="btn btn-warning"
                onClick={handlePrintAllCupones}
                disabled={!activeFilters}
                title={activeFilters ? "Imprimir cupones de todos los comprobantes filtrados" : "Aplique filtros para imprimir cupones en lote"}
              >
                <i className="fa-solid fa-list"></i>
              </button>
            </div>
          </div>
        </div>
      </div>

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
                    <th>Número</th>
                    <th>Cliente</th>
                    <th>Vendedor</th>
                    <th className="text-end">Total</th>
                    <th className="text-center">Estado</th>
                    <th className="text-center">Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {comprobantes.map((comprobante) => (
                    <tr key={comprobante.id}>
                      <td>{formatDate(comprobante.fecha)}</td>
                      <td>
                        {comprobante.tipoComprobante || '-'}
                        {comprobante.estaCancelado && (
                          <span className="badge bg-secondary ms-1" title={`Cancelada por NC: ${comprobante.notaCreditoNumero}`}>
                            Cancelada
                          </span>
                        )}
                      </td>
                      <td>{comprobante.numeroComprobante || '-'}</td>
                      <td>{comprobante.clienteNombre || '-'}</td>
                      <td>{comprobante.vendedorNombre || '-'}</td>
                      <td className="text-end">${comprobante.total.toFixed(2)}</td>
                      <td className="text-center">
                        {comprobante.estado === 'PAG' && (
                          <span className="badge bg-success">Pagada</span>
                        )}
                        {comprobante.estado === 'CAN' && (
                          <span className="badge bg-danger">Cancelado</span>
                        )}
                        {(!comprobante.estado || comprobante.estado === 'PEN') && (
                          <span className="badge bg-warning text-dark">Pendiente</span>
                        )}
                      </td>
                      <td className="text-center">
                        <IconButton
                          icon="fa-solid fa-print"
                          title="Ver PDF"
                          variant="info"
                          onClick={() => comprobanteService.openPdf(comprobante.id)}
                        />
                        <IconButton
                          icon="fa-solid fa-list"
                          title="Cupones"
                          variant="secondary"
                          onClick={() => comprobanteService.openCuponesPdf(comprobante.id)}
                        />
                        <Link to={`/comprobantes/ver/${comprobante.id}`}>
                          <IconButton icon="fa-solid fa-eye" title="Ver" variant="primary" />
                        </Link>
                        {comprobante.tipoComprobante === 'FC' && !comprobante.estaCancelado && (
                          <IconButton
                            icon="fa-solid fa-ban"
                            title="Cancelar (Generar NC)"
                            variant="danger"
                            onClick={() => handleCancelarClick(comprobante)}
                          />
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ComprobantesPage;
