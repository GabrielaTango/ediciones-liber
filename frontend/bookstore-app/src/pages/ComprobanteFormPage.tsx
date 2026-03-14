import { useState, useEffect, useMemo, useRef } from 'react';
import { useNavigate, useParams, useLocation, Link } from 'react-router-dom';
import Swal from 'sweetalert2';
import Select from 'react-select';
import { comprobanteService } from '../services/comprobanteService';
import { showSuccessAlert, showErrorAlert, showConfirmDialog } from '../utils/sweetalert';
import { clienteService } from '../services/clienteService';
import { articuloService } from '../services/articuloService';
import { referenceService } from '../services/referenceService';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { GradientCard } from '../components/GradientCard';
import { FormGroup } from '../components/FormGroup';
import { Icon } from '../components/Icon';
import type { CreateComprobanteDto, UpdateComprobanteDto, ComprobanteDetalleDto } from '../types/comprobante';
import type { Cliente } from '../types/cliente';
import type { Articulo } from '../types/articulo';
import type { Vendedor, Zona } from '../types/references';
import IconButton from '../components/IconButton';

interface SelectOption {
  value: number;
  label: string;
}

interface ArticuloOption extends SelectOption {
  precio: number;
}

interface ItemTemp extends ComprobanteDetalleDto {
  tempId: number;
  articuloDescripcion?: string;
  articuloCodigo?: string;
}

const ComprobanteFormPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const isViewMode = location.pathname.includes('/ver/');
  const isEditMode = !!id && !isViewMode;

  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [articulos, setArticulos] = useState<Articulo[]>([]);
  const [vendedores, setVendedores] = useState<Vendedor[]>([]);
  const [zonas, setZonas] = useState<Zona[]>([]);

  const [selectedClienteId, setSelectedClienteId] = useState<number | null>(null);
  const [selectedCliente, setSelectedCliente] = useState<Cliente | null>(null);
  const [selectedVendedorId, setSelectedVendedorId] = useState<number | null>(null);

  const [items, setItems] = useState<ItemTemp[]>([]);
  const [nextTempId, setNextTempId] = useState(1);

  // Formulario de item
  const [showItemModal, setShowItemModal] = useState(false);
  const [editingItem, setEditingItem] = useState<ItemTemp | null>(null);
  const [itemForm, setItemForm] = useState({
    articulo_Id: 0,
    cantidad: 1,
    precio_Unitario: 0,
  });

  // Refs para navegación con Enter en el modal de items
  const articuloSelectRef = useRef<any>(null);
  const cantidadRef = useRef<HTMLInputElement>(null);
  const precioRef = useRef<HTMLInputElement>(null);
  const agregarBtnRef = useRef<HTMLButtonElement>(null);

  // Cálculo de cuotas (se define ANTES de cargar items)
  const [anticipo, setAnticipo] = useState<number>(0);
  const [contraEntrega, setContraEntrega] = useState<number>(0);
  const [cantidadCuotas, setCantidadCuotas] = useState<number>(1);
  const [valorCuota, setValorCuota] = useState<number>(0);
  const [gastosEnvio, setGastosEnvio] = useState<number>(0);

  // Tipo de comprobante
  const [esElectronica, setEsElectronica] = useState<boolean>(true);
  const [esPresupuesto, setEsPresupuesto] = useState<boolean>(false);

  // Datos del comprobante cargado (para modo visualización)
  const [tipoComprobanteCargado, setTipoComprobanteCargado] = useState<string>('');
  const [numeroComprobanteCargado, setNumeroComprobanteCargado] = useState<string>('');
  const [caeCargado, setCaeCargado] = useState<string>('');
  const [fechaCargada, setFechaCargada] = useState<string>('');
  const [estadoComprobante, setEstadoComprobante] = useState<string>('PEN');
  const [estaCancelado, setEstaCancelado] = useState<boolean>(false);
  const [notaCreditoNumero, setNotaCreditoNumero] = useState<string>('');
  const [comprobanteAsociadoNumero, setComprobanteAsociadoNumero] = useState<string>('');

  const [loading, setLoading] = useState(false);

  // Helper para obtener el nombre del tipo de comprobante
  const getTipoComprobanteNombre = (tipo: string): string => {
    switch (tipo) {
      case 'FC': return 'Factura';
      case 'NC': return 'Nota de Crédito';
      case 'PRE': return 'Presupuesto';
      default: return tipo || 'Comprobante';
    }
  };

  // Helper para obtener el color del badge según el tipo
  const getTipoComprobanteBadgeClass = (tipo: string): string => {
    switch (tipo) {
      case 'FC': return 'bg-success';
      case 'NC': return 'bg-danger';
      case 'PRE': return 'bg-warning text-dark';
      default: return 'bg-secondary';
    }
  };

  // Helper para obtener badge de estado
  const getEstadoBadge = (estado: string) => {
    switch (estado) {
      case 'PAG': return <span className="badge bg-success fs-6">Pagada</span>;
      case 'CAN': return <span className="badge bg-danger fs-6">Deuda Cancelada</span>;
      default: return <span className="badge bg-warning text-dark fs-6">Pendiente</span>;
    }
  };

  const handleCancelarDeuda = async () => {
    if (!id) return;

    const result = await showConfirmDialog(
      'Cancelar Deuda',
      `¿Está seguro de cancelar la deuda del comprobante ${numeroComprobanteCargado}? Esto eliminará las cuotas pendientes.`
    );

    if (result.isConfirmed) {
      try {
        await comprobanteService.cancelarDeuda(parseInt(id));
        await showSuccessAlert('Deuda Cancelada', 'Las cuotas pendientes fueron eliminadas y el comprobante fue marcado como cancelado.');
        loadComprobante(parseInt(id));
      } catch (err: any) {
        const mensaje = err.response?.data?.message || 'Error al cancelar la deuda';
        await showErrorAlert('Error', mensaje);
      }
    }
  };

  // Opciones para React Select
  const clienteOptions = useMemo<SelectOption[]>(() =>
    clientes.map(c => {
      const zona = zonas.find(z => z.id === c.zona_Id);
      return {
        value: c.id,
        label: `${c.nombre} - ${zona?.descripcion || 'Sin zona'}`
      };
    }), [clientes, zonas]);

  const articuloOptions = useMemo<ArticuloOption[]>(() =>
    articulos.map(a => ({
      value: a.id,
      label: `${a.descripcion} - ${a.tema || 'Sin editorial'}`,
      precio: a.precio || 0
    })), [articulos]);

  // Valores seleccionados para React Select
  const selectedClienteOption = useMemo(() =>
    clienteOptions.find(o => o.value === selectedClienteId) || null
  , [clienteOptions, selectedClienteId]);

  const selectedArticuloOption = useMemo(() =>
    articuloOptions.find(o => o.value === itemForm.articulo_Id) || null
  , [articuloOptions, itemForm.articulo_Id]);

  useEffect(() => {
    loadInitialData();
  }, []);

  useEffect(() => {
    if ((isEditMode || isViewMode) && id) {
      loadComprobante(parseInt(id));
    }
  }, [id, isEditMode, isViewMode]);

  useEffect(() => {
    if (selectedClienteId) {
      loadCliente(selectedClienteId);
    }
  }, [selectedClienteId]);

  // El total se calcula automáticamente
  const calcularTotalComprobante = (): number => {
    return anticipo + contraEntrega + (cantidadCuotas * valorCuota);
  };

  const loadInitialData = async () => {
    try {
      setLoading(true);
      const [clientesData, articulosData, vendedoresData, zonasData] = await Promise.all([
        clienteService.getAll(),
        articuloService.getAll(),
        referenceService.getVendedores(),
        referenceService.getZonas(),
      ]);
      setClientes(clientesData);
      setArticulos(articulosData);
      setVendedores(vendedoresData);
      setZonas(zonasData);

      // Cargar último gasto de envío como default solo para comprobantes nuevos
      if (!id) {
        try {
          const ultimoGasto = await comprobanteService.getUltimoGastoEnvio();
          if (ultimoGasto > 0) {
            setGastosEnvio(ultimoGasto);
          }
        } catch {
          // Si falla, se queda en 0
        }
      }
    } catch (err) {
      console.error('Error loading initial data:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadComprobante = async (comprobanteId: number) => {
    try {
      setLoading(true);
      const comprobante = await comprobanteService.getById(comprobanteId);
      setSelectedClienteId(comprobante.cliente_Id);
      setSelectedVendedorId(comprobante.vendedor_Id || null);
      setAnticipo(comprobante.anticipo || 0);
      setContraEntrega(comprobante.contraEntrega || 0);
      setCantidadCuotas(comprobante.cuotas || 1);
      setValorCuota(comprobante.valorCuota || 0);
      setGastosEnvio(comprobante.gastosEnvio || 0);
      setEsElectronica(comprobante.esElectronica ?? true);
      setEsPresupuesto(comprobante.esPresupuesto ?? false);

      // Guardar datos para visualización
      setTipoComprobanteCargado(comprobante.tipoComprobante || '');
      setNumeroComprobanteCargado(comprobante.numeroComprobante || '');
      setCaeCargado(comprobante.cae || '');
      setFechaCargada(comprobante.fecha ? new Date(comprobante.fecha).toLocaleDateString('es-AR') : '');
      setEstadoComprobante(comprobante.estado || 'PEN');
      setEstaCancelado(comprobante.estaCancelado || false);
      setNotaCreditoNumero(comprobante.notaCreditoNumero || '');
      setComprobanteAsociadoNumero(comprobante.comprobanteAsociadoNumero || '');

      const itemsTemp: ItemTemp[] = comprobante.detalles.map((d, index) => ({
        tempId: index + 1,
        articulo_Id: d.articulo_Id,
        articuloCodigo: d.articuloCodigo,
        articuloDescripcion: d.articuloDescripcion,
        cantidad: d.cantidad,
        precio_Unitario: d.precio_Unitario,
        subtotal: d.subtotal,
      }));
      setItems(itemsTemp);
      setNextTempId(itemsTemp.length + 1);
    } catch (err) {
      console.error('Error loading comprobante:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadCliente = async (clienteId: number) => {
    try {
      const cliente = await clienteService.getById(clienteId);
      setSelectedCliente(cliente);
      // Establecer vendedor del cliente como predeterminado
      if (cliente.vendedor_Id && !selectedVendedorId) {
        setSelectedVendedorId(cliente.vendedor_Id);
      }
    } catch (err) {
      console.error('Error loading cliente:', err);
    }
  };

  const handleClienteChange = (option: SelectOption | null) => {
    setSelectedClienteId(option?.value || null);
  };

  const handleVendedorChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const vendedorId = parseInt(e.target.value);
    setSelectedVendedorId(vendedorId || null);
  };

  const handleAddItem = () => {
    setEditingItem(null);
    setItemForm({
      articulo_Id: 0,
      cantidad: 1,
      precio_Unitario: 0,
    });
    setShowItemModal(true);
  };

  const handleEditItem = (item: ItemTemp) => {
    setEditingItem(item);
    setItemForm({
      articulo_Id: item.articulo_Id,
      cantidad: item.cantidad,
      precio_Unitario: item.precio_Unitario,
    });
    setShowItemModal(true);
  };

  const handleDeleteItem = (tempId: number) => {
    setItems(items.filter(item => item.tempId !== tempId));
  };

  const handleArticuloSelect = (option: ArticuloOption | null) => {
    setItemForm({
      ...itemForm,
      articulo_Id: option?.value || 0,
      precio_Unitario: option?.precio || 0,
    });
  };

  const handleItemFormChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setItemForm({
      ...itemForm,
      [name]: parseFloat(value) || 0,
    });
  };

  const handleSaveItem = () => {
    if (itemForm.articulo_Id === 0) {
      mostrarError('Debe seleccionar un artículo');
      return;
    }

    if (itemForm.cantidad <= 0) {
      mostrarError('La cantidad debe ser mayor a 0');
      return;
    }

    const articulo = articulos.find(a => a.id === itemForm.articulo_Id);
    const subtotal = itemForm.cantidad * itemForm.precio_Unitario;

    const newItem: ItemTemp = {
      tempId: editingItem ? editingItem.tempId : nextTempId,
      articulo_Id: itemForm.articulo_Id,
      articuloCodigo: undefined,
      articuloDescripcion: articulo?.descripcion,
      cantidad: itemForm.cantidad,
      precio_Unitario: itemForm.precio_Unitario,
      subtotal: subtotal,
    };

    if (editingItem) {
      setItems(items.map(item => item.tempId === editingItem.tempId ? newItem : item));
    } else {
      setItems([...items, newItem]);
      setNextTempId(nextTempId + 1);
    }

    setShowItemModal(false);
  };

  const calcularTotalItems = (): number => {
    return items.reduce((sum, item) => sum + item.subtotal, 0);
  };

  const validarTotales = (): boolean => {
    const totalItems = calcularTotalItems();
    const totalComprobante = calcularTotalComprobante();
    return Math.abs(totalItems - totalComprobante) < 0.01; // Tolerancia de centavos
  };

  const mostrarError = (mensaje: string) => {
    Swal.fire({
      icon: 'error',
      title: 'Error',
      text: mensaje,
      confirmButtonColor: '#dc3545',
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!selectedClienteId) {
      mostrarError('Debe seleccionar un cliente');
      return;
    }

    if (!esElectronica && !esPresupuesto) {
      mostrarError('Si selecciona modalidad Manual, debe marcar el tilde "Es Presupuesto"');
      return;
    }

    const totalComprobante = calcularTotalComprobante();

    if (totalComprobante <= 0) {
      mostrarError('El total del comprobante debe ser mayor a 0');
      return;
    }

    if (items.length === 0) {
      mostrarError('Debe agregar al menos un item');
      return;
    }

    if (!validarTotales()) {
      const totalItems = calcularTotalItems();
      const diferencia = totalComprobante - totalItems;
      mostrarError(`El total de items ($${totalItems.toFixed(2)}) no coincide con el total calculado ($${totalComprobante.toFixed(2)}). Diferencia: $${diferencia.toFixed(2)}`);
      return;
    }

    const detalles: ComprobanteDetalleDto[] = items.map(item => ({
      articulo_Id: item.articulo_Id,
      cantidad: item.cantidad,
      precio_Unitario: item.precio_Unitario,
      subtotal: item.subtotal,
    }));

    const dto: CreateComprobanteDto | UpdateComprobanteDto = {
      cliente_Id: selectedClienteId,
      fecha: new Date().toISOString(),
      tipoComprobante: 'FC',
      total: totalComprobante,
      vendedor_Id: selectedVendedorId || undefined,
      anticipo: anticipo,
      contraEntrega: contraEntrega,
      cuotas: cantidadCuotas,
      valorCuota: valorCuota,
      gastosEnvio: gastosEnvio > 0 ? gastosEnvio : undefined,
      esElectronica: esElectronica,
      esPresupuesto: esPresupuesto,
      detalles: detalles,
    };

    try {
      setLoading(true);
      if (isEditMode && id) {
        await comprobanteService.update(parseInt(id), dto as UpdateComprobanteDto);
        navigate('/comprobantes');
      } else {
        const nuevoComprobante = await comprobanteService.create(dto as CreateComprobanteDto);
        await Swal.fire({
          icon: 'success',
          title: 'Comprobante Generado',
          html: `<p>Se ha generado el comprobante:</p><h3 style="color: #198754; margin: 10px 0;">${nuevoComprobante.numeroComprobante}</h3>`,
          confirmButtonText: 'Aceptar',
          confirmButtonColor: '#198754',
        });
        navigate('/comprobantes');
      }
    } catch (err: any) {
      const mensaje = err.response?.data?.message ||
        err.response?.data?.error ||
        `Error al ${isEditMode ? 'actualizar' : 'crear'} el comprobante`;
      mostrarError(mensaje);
      console.error('Error saving comprobante:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <PageHeader
        title={isViewMode
          ? `${getTipoComprobanteNombre(tipoComprobanteCargado)}`
          : isEditMode ? 'Editar Comprobante' : 'Nuevo Comprobante'}
        icon="fa-solid fa-receipt"
        actions={
          <div className="d-flex gap-2">
            {isViewMode && estadoComprobante === 'PEN' && tipoComprobanteCargado !== 'NC' && (
              <button
                type="button"
                className="btn btn-danger"
                onClick={handleCancelarDeuda}
              >
                <i className="fa-solid fa-ban me-1"></i>
                Cancelar Deuda
              </button>
            )}
            <Link to="/comprobantes" className="btn-secondary-action">
              <Icon name="fa-solid fa-arrow-left" />
              Volver
            </Link>
          </div>
        }
      />

      {/* Información del comprobante en modo visualización */}
      {isViewMode && tipoComprobanteCargado && (
        <div className={`alert ${estaCancelado ? 'alert-secondary' : tipoComprobanteCargado === 'NC' ? 'alert-danger' : tipoComprobanteCargado === 'FC' ? 'alert-success' : 'alert-warning'} mb-4`}>
          <div className="d-flex justify-content-between align-items-center flex-wrap">
            <div className="d-flex align-items-center gap-3">
              <span className={`badge ${estaCancelado ? 'bg-secondary' : getTipoComprobanteBadgeClass(tipoComprobanteCargado)} fs-6`}>
                {getTipoComprobanteNombre(tipoComprobanteCargado)}
              </span>
              <span className="fw-bold fs-5">{numeroComprobanteCargado}</span>
              {getEstadoBadge(estadoComprobante)}
              {estaCancelado && (
                <span className="badge bg-dark">CANCELADA</span>
              )}
            </div>
            <div className="d-flex gap-4 text-muted">
              <span><Icon name="fa-solid fa-calendar" /> {fechaCargada}</span>
              {caeCargado && (
                <span><Icon name="fa-solid fa-certificate" /> CAE: {caeCargado}</span>
              )}
            </div>
          </div>
          {/* Mostrar relación con otros comprobantes */}
          {estaCancelado && notaCreditoNumero && (
            <div className="mt-2 pt-2 border-top">
              <Icon name="fa-solid fa-link" /> Cancelada por Nota de Crédito: <strong>{notaCreditoNumero}</strong>
            </div>
          )}
          {tipoComprobanteCargado === 'NC' && comprobanteAsociadoNumero && (
            <div className="mt-2 pt-2 border-top">
              <Icon name="fa-solid fa-link" /> Cancela Factura: <strong>{comprobanteAsociadoNumero}</strong>
            </div>
          )}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <GradientCard title="Tipo de Comprobante" icon="fa-solid fa-file-invoice">
          <div className="row align-items-center">
            <div className="col-md-3">
              <FormGroup label="Modalidad" required>
                <select
                  className="form-select"
                  value={esElectronica ? 'electronica' : 'manual'}
                  onChange={(e) => {
                    const isElectronica = e.target.value === 'electronica';
                    setEsElectronica(isElectronica);
                    if (isElectronica) {
                      setEsPresupuesto(false);
                    }
                  }}
                  disabled={isEditMode || isViewMode}
                >
                  <option value="electronica">Electrónica</option>
                  <option value="manual">Manual</option>
                </select>
              </FormGroup>
            </div>
            {!esElectronica && (
              <div className="col-md-3">
                <div className="form-check mt-4">
                  <input
                    type="checkbox"
                    className="form-check-input"
                    id="esPresupuesto"
                    checked={esPresupuesto}
                    onChange={(e) => setEsPresupuesto(e.target.checked)}
                    disabled={isEditMode || isViewMode}
                  />
                  <label className="form-check-label" htmlFor="esPresupuesto">
                    <strong>Es Presupuesto</strong>
                  </label>
                </div>
              </div>
            )}
            <div className={!esElectronica ? 'col-md-6' : 'col-md-9'}>
              <div className={`alert mb-0 ${esElectronica ? 'alert-success' : esPresupuesto ? 'alert-warning' : 'alert-danger'}`}>
                <Icon name={esElectronica ? 'fa-solid fa-bolt' : esPresupuesto ? 'fa-solid fa-file-lines' : 'fa-solid fa-triangle-exclamation'} />
                <strong>
                  {esElectronica ? 'Factura Electrónica' : esPresupuesto ? 'Presupuesto' : 'Debe marcar Presupuesto'}
                </strong>
                <small className="d-block">
                  {esElectronica ? 'Se solicitará CAE a AFIP' : esPresupuesto ? 'Sin CAE - Tipo PRE' : 'Seleccione el tilde para continuar'}
                </small>
              </div>
            </div>
          </div>
        </GradientCard>

        <GradientCard title="Datos del Cliente" icon="fa-solid fa-circle-user" className="mt-4">
          <div className="row">
            <div className="col-md-6">
              <FormGroup label="Cliente" required>
                <Select<SelectOption>
                  options={clienteOptions}
                  value={selectedClienteOption}
                  onChange={handleClienteChange}
                  placeholder="Buscar cliente..."
                  isClearable
                  isSearchable
                  isDisabled={isViewMode}
                  noOptionsMessage={() => "No se encontraron clientes"}
                  loadingMessage={() => "Cargando..."}
                  classNamePrefix="react-select"
                  styles={{
                    control: (base) => ({
                      ...base,
                      minHeight: '38px',
                    }),
                  }}
                />
              </FormGroup>
            </div>

            {selectedCliente && (
              <>
                <div className="col-md-6">
                  <FormGroup label="Documento">
                    <input
                      type="text"
                      className="form-control"
                      value={`${selectedCliente.tipoDocumento || ''} ${selectedCliente.nroDocumento || ''}`}
                      readOnly
                    />
                  </FormGroup>
                </div>
                <div className="col-md-6">
                  <FormGroup label="Domicilio">
                    <input
                      type="text"
                      className="form-control"
                      value={selectedCliente.domicilioComercial || '-'}
                      readOnly
                    />
                  </FormGroup>
                </div>
                <div className="col-md-3">
                  <FormGroup label="Teléfono">
                    <input
                      type="text"
                      className="form-control"
                      value={selectedCliente.telefono || '-'}
                      readOnly
                    />
                  </FormGroup>
                </div>
                <div className="col-md-3">
                  <FormGroup label="Condición IVA">
                    <input
                      type="text"
                      className="form-control"
                      value={selectedCliente.categoriaIva || '-'}
                      readOnly
                    />
                  </FormGroup>
                </div>
              </>
            )}
          </div>
        </GradientCard>

        <GradientCard title="Vendedor" icon="fa-solid fa-user-tie" className="mt-4">
          <div className="row">
            <div className="col-md-6">
              <FormGroup label="Vendedor Asignado">
                <select
                  className="form-select"
                  value={selectedVendedorId || ''}
                  onChange={handleVendedorChange}
                  disabled={isViewMode}
                >
                  <option value="">Sin vendedor</option>
                  {vendedores.map((vendedor) => (
                    <option key={vendedor.id} value={vendedor.id}>
                      {vendedor.descripcion}
                    </option>
                  ))}
                </select>
              </FormGroup>
            </div>
          </div>
        </GradientCard>

        <GradientCard title="Cálculo de Cuotas" icon="fa-solid fa-calculator" className="mt-4">
          <div className="row">
            <div className="col-md-2">
              <FormGroup label="Anticipo">
                <div className="input-group">
                  <span className="input-group-text">$</span>
                  <input
                    type="number"
                    className="form-control"
                    value={anticipo || ''}
                    onChange={(e) => setAnticipo(parseFloat(e.target.value) || 0)}
                    onBlur={(e) => { if (!e.target.value) setAnticipo(0); }}
                    onFocus={(e) => e.target.select()}
                    min="0"
                    step="0.01"
                    placeholder="0.00"
                    disabled={isViewMode}
                  />
                </div>
                <small className="form-text text-muted">Pago anticipado</small>
              </FormGroup>
            </div>
            <div className="col-md-2">
              <FormGroup label="Contra Entrega">
                <div className="input-group">
                  <span className="input-group-text">$</span>
                  <input
                    type="number"
                    className="form-control"
                    value={contraEntrega || ''}
                    onChange={(e) => setContraEntrega(parseFloat(e.target.value) || 0)}
                    onBlur={(e) => { if (!e.target.value) setContraEntrega(0); }}
                    onFocus={(e) => e.target.select()}
                    min="0"
                    step="0.01"
                    placeholder="0.00"
                    disabled={isViewMode}
                  />
                </div>
                <small className="form-text text-muted">Factura</small>
              </FormGroup>
            </div>
            <div className="col-md-2">
              <FormGroup label="Cuotas">
                <input
                  type="number"
                  className="form-control"
                  value={cantidadCuotas || ''}
                  onChange={(e) => setCantidadCuotas(parseInt(e.target.value) || 0)}
                  onBlur={(e) => { if (!e.target.value || cantidadCuotas < 1) setCantidadCuotas(1); }}
                  onFocus={(e) => e.target.select()}
                  min="1"
                  max="60"
                  disabled={isViewMode}
                />
              </FormGroup>
            </div>
            <div className="col-md-2">
              <FormGroup label="Valor Cuota">
                <div className="input-group">
                  <span className="input-group-text">$</span>
                  <input
                    type="number"
                    className="form-control"
                    value={valorCuota || ''}
                    onChange={(e) => setValorCuota(parseFloat(e.target.value) || 0)}
                    onBlur={(e) => { if (!e.target.value) setValorCuota(0); }}
                    onFocus={(e) => e.target.select()}
                    min="0"
                    step="0.01"
                    placeholder="0.00"
                    disabled={isViewMode}
                  />
                </div>
              </FormGroup>
            </div>
            <div className="col-md-4">
              <FormGroup label="TOTAL">
                <div className="input-group">
                  <span className="input-group-text">$</span>
                  <input
                    type="text"
                    className="form-control form-control-lg fw-bold text-end bg-light"
                    value={calcularTotalComprobante().toFixed(2)}
                    readOnly
                  />
                </div>
                <small className="form-text text-muted">
                  Anticipo + Contra Entrega + ({cantidadCuotas} × ${valorCuota.toFixed(2)})
                </small>
              </FormGroup>
            </div>
          </div>
        </GradientCard>

        <GradientCard title="Gastos de Envío" icon="fa-solid fa-truck" className="mt-4">
          <div className="row">
            <div className="col-md-4">
              <FormGroup label="Valor de Envío">
                <div className="input-group">
                  <span className="input-group-text">$</span>
                  <input
                    type="number"
                    className="form-control"
                    value={gastosEnvio || ''}
                    onChange={(e) => setGastosEnvio(parseFloat(e.target.value) || 0)}
                    onBlur={(e) => { if (!e.target.value) setGastosEnvio(0); }}
                    onFocus={(e) => e.target.select()}
                    min="0"
                    step="0.01"
                    placeholder="0.00"
                    disabled={isViewMode}
                  />
                </div>
                <small className="form-text text-muted">Este valor se mostrará solo en la primera hoja del comprobante</small>
              </FormGroup>
            </div>
          </div>
        </GradientCard>

        <GradientCard title="Items del Comprobante" icon="fa-solid fa-box" className="mt-4">
          {!isViewMode && (
            <div className="d-flex justify-content-end">
              <GradientButton
                className='btn-primary-action'
                type="button"
                icon="fa-solid fa-circle-plus"
                onClick={handleAddItem}
              >
                Agregar Item
              </GradientButton>
            </div>
          )}
          {items.length === 0 ? (
            <div className="alert alert-info mt-3 d-flex align-items-center justify-content-center" onClick={isViewMode ? undefined : handleAddItem}>
              {isViewMode ? (
                'No hay items en este comprobante.'
              ) : (
                <>
                  No hay items agregados. Haga clic en
                  <Link to={"#"} onClick={handleAddItem}>Agregar Item</Link>
                  para comenzar.
                </>
              )}
            </div>
          ) : (
            <div className="table-responsive mt-3">
              <table className="table table-hover mb-0">
                <thead>
                  <tr>
                    <th>Código</th>
                    <th>Descripción</th>
                    <th className="text-end">Cantidad</th>
                    <th className="text-end">Precio Unit.</th>
                    <th className="text-end">Subtotal</th>
                    {!isViewMode && <th className="text-center">Acciones</th>}
                  </tr>
                </thead>
                <tbody>
                  {items.map((item) => (
                    <tr key={item.tempId}>
                      <td>{item.articuloCodigo || '-'}</td>
                      <td>{item.articuloDescripcion}</td>
                      <td className="text-end">{item.cantidad}</td>
                      <td className="text-end">${item.precio_Unitario.toFixed(2)}</td>
                      <td className="text-end">${item.subtotal.toFixed(2)}</td>
                      {!isViewMode && (
                        <td className="text-center">
                          <IconButton
                            className='me-2'
                            icon="fa-solid fa-pen"
                            title="Editar"
                            variant="primary"
                            onClick={() => handleEditItem(item)}
                          />
                          <IconButton
                            icon="fa-solid fa-trash"
                            title="Eliminar"
                            variant="danger"
                            onClick={() => handleDeleteItem(item.tempId)}
                          />
                        </td>
                      )}
                    </tr>
                  ))}
                  <tr className="table-primary">
                    <td colSpan={4} className="text-end"><strong>TOTAL ITEMS:</strong></td>
                    <td className="text-end"><strong>${calcularTotalItems().toFixed(2)}</strong></td>
                    {!isViewMode && <td></td>}
                  </tr>
                </tbody>
              </table>
            </div>
          )}
          {items.length > 0 && calcularTotalComprobante() > 0 && (
            <div className={`alert mt-3 mb-0 ${validarTotales() ? 'alert-success' : 'alert-warning'}`}>
              <Icon name={validarTotales() ? 'fa-solid fa-circle-check' : 'fa-solid fa-triangle-exclamation'} />
              {validarTotales() ? (
                <strong>Los totales coinciden correctamente</strong>
              ) : (
                <>
                  <strong>Diferencia:</strong> ${(calcularTotalComprobante() - calcularTotalItems()).toFixed(2)}
                  <span className="ms-2">(Total calculado: ${calcularTotalComprobante().toFixed(2)} - Total items: ${calcularTotalItems().toFixed(2)})</span>
                </>
              )}
            </div>
          )}
        </GradientCard>

        {!isViewMode && (
          <div className="d-flex gap-2 mt-4">
            <GradientButton
              type="submit"
              icon="fa-solid fa-floppy-disk"
              disabled={loading || items.length === 0}
            >
              {loading ? 'Guardando...' : isEditMode ? 'Actualizar' : 'Crear'}
            </GradientButton>
            <Link to="/comprobantes" className="btn-secondary-action">
              Cancelar
            </Link>
          </div>
        )}
      </form>

      {showItemModal && (
        <div className="modal show d-block" tabIndex={-1} style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">
                  <Icon name="fa-solid fa-box" />
                  {editingItem ? 'Editar Item' : 'Agregar Item'}
                </h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setShowItemModal(false)}
                  aria-label="Close"
                ></button>
              </div>
              <div className="modal-body">
                <FormGroup label="Artículo" required>
                  <Select<ArticuloOption>
                    ref={articuloSelectRef}
                    options={articuloOptions}
                    value={selectedArticuloOption}
                    onChange={(option) => {
                      handleArticuloSelect(option);
                      if (option) {
                        setTimeout(() => cantidadRef.current?.focus(), 50);
                      }
                    }}
                    placeholder="Buscar artículo..."
                    isClearable
                    isSearchable
                    autoFocus
                    noOptionsMessage={() => "No se encontraron artículos"}
                    loadingMessage={() => "Cargando..."}
                    classNamePrefix="react-select"
                    menuPortalTarget={document.body}
                    styles={{
                      control: (base) => ({
                        ...base,
                        minHeight: '38px',
                      }),
                      menuPortal: (base) => ({ ...base, zIndex: 9999 }),
                    }}
                  />
                </FormGroup>

                <FormGroup label="Cantidad" required>
                  <input
                    ref={cantidadRef}
                    type="number"
                    className="form-control"
                    name="cantidad"
                    value={itemForm.cantidad || ''}
                    onChange={handleItemFormChange}
                    onBlur={(e) => { if (!e.target.value) setItemForm(f => ({ ...f, cantidad: 1 })); }}
                    onFocus={(e) => e.target.select()}
                    onKeyDown={(e) => { if (e.key === 'Enter') { e.preventDefault(); precioRef.current?.focus(); } }}
                    min="1"
                    required
                  />
                </FormGroup>

                <FormGroup label="Precio Unitario" required>
                  <div className="input-group">
                    <span className="input-group-text">$</span>
                    <input
                      ref={precioRef}
                      type="number"
                      className="form-control"
                      name="precio_Unitario"
                      value={itemForm.precio_Unitario || ''}
                      onChange={handleItemFormChange}
                      onBlur={(e) => { if (!e.target.value) setItemForm(f => ({ ...f, precio_Unitario: 0 })); }}
                      onFocus={(e) => e.target.select()}
                      onKeyDown={(e) => { if (e.key === 'Enter') { e.preventDefault(); agregarBtnRef.current?.focus(); } }}
                      min="0"
                      step="0.01"
                      placeholder="0.00"
                      required
                    />
                  </div>
                </FormGroup>

                <div className="alert alert-info mb-0">
                  <Icon name="fa-solid fa-calculator" />
                  <strong>Subtotal:</strong> ${(itemForm.cantidad * itemForm.precio_Unitario).toFixed(2)}
                </div>
              </div>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn-secondary-action"
                  onClick={() => setShowItemModal(false)}
                >
                  Cancelar
                </button>
                <GradientButton
                  ref={agregarBtnRef}
                  onClick={handleSaveItem}
                  icon={editingItem ? 'fa-solid fa-circle-check' : 'fa-solid fa-circle-plus'}
                >
                  {editingItem ? 'Actualizar' : 'Agregar'}
                </GradientButton>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ComprobanteFormPage;
