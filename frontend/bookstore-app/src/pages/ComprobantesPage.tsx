import { useState, useEffect, useMemo } from 'react';
import { Link } from 'react-router-dom';
import Select from 'react-select';
import { comprobanteService, type ComprobanteFilters } from '../services/comprobanteService';
import { referenceService } from '../services/referenceService';
import { clienteService } from '../services/clienteService';
import type { Comprobante } from '../types/comprobante';
import type { Zona } from '../types/references';
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
  const [comprobantes, setComprobantes] = useState<Comprobante[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Data para filtros
  const [zonas, setZonas] = useState<Zona[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);

  // Estados de filtros
  const [selectedZona, setSelectedZona] = useState<SelectOption | null>(null);
  const [selectedCliente, setSelectedCliente] = useState<SelectOption | null>(null);
  const [tipoComprobante, setTipoComprobante] = useState<string>('');
  const [fechaDesde, setFechaDesde] = useState<string>('');
  const [fechaHasta, setFechaHasta] = useState<string>('');

  useEffect(() => {
    loadComprobantes();
    loadFilterData();
  }, []);

  const loadFilterData = async () => {
    try {
      const [zonasData, clientesData] = await Promise.all([
        referenceService.getZonas(),
        clienteService.getAll()
      ]);
      setZonas(zonasData);
      setClientes(clientesData);
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
    if (tipoComprobante) filters.tipoComprobante = tipoComprobante;
    if (fechaDesde) filters.fechaDesde = fechaDesde;
    if (fechaHasta) filters.fechaHasta = fechaHasta;
    loadComprobantes(filters);
  };

  const handleClearFilters = () => {
    setSelectedZona(null);
    setSelectedCliente(null);
    setTipoComprobante('');
    setFechaDesde('');
    setFechaHasta('');
    loadComprobantes();
  };

  // Opciones para React Select
  const zonaOptions = useMemo<SelectOption[]>(() =>
    zonas.map(z => ({
      value: z.id,
      label: z.descripcion || z.codigo || `Zona ${z.id}`
    })), [zonas]);

  const clienteOptions = useMemo<SelectOption[]>(() =>
    clientes.map(c => ({
      value: c.id,
      label: `${c.codigo || ''} - ${c.nombre}`
    })), [clientes]);

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
          {/* Fila superior: Zona y Cliente */}
          <div className="row g-3 mb-3">
            <div className="col-md-4">
              <label className="form-label">Zona</label>
              <Select
                isClearable
                placeholder="Seleccionar zona..."
                options={zonaOptions}
                value={selectedZona}
                onChange={(option) => setSelectedZona(option)}
              />
            </div>
            <div className="col-md-8">
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
                        <IconButton
                          icon="fa-solid fa-print"
                          title="Imprimir Completo (Factura x3 + Cupones)"
                          variant="success"
                          onClick={() => comprobanteService.openCompletoPdf(comprobante.id)}
                        />
                        <IconButton
                          icon="fa-solid fa-file-pdf"
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
