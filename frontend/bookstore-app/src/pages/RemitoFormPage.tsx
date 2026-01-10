import { useState, useEffect, useMemo } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import Select from 'react-select';
import { remitoService } from '../services/remitoService';
import { clienteService } from '../services/clienteService';
import { referenceService } from '../services/referenceService';
import type { CreateRemitoDto, UpdateRemitoDto } from '../types/remito';
import type { Cliente } from '../types/cliente';
import type { Transporte, SubZona, Provincia } from '../types/references';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';

interface SelectOption {
  value: number;
  label: string;
}

const RemitoFormPage = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = !!id;

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [transportes, setTransportes] = useState<Transporte[]>([]);
  const [subZonas, setSubZonas] = useState<SubZona[]>([]);
  const [provincias, setProvincias] = useState<Provincia[]>([]);

  const [formData, setFormData] = useState<CreateRemitoDto | UpdateRemitoDto>({
    clienteId: 0,
    transporteId: 0,
    cantidadBultos: 1,
    valorDeclarado: 0,
    observaciones: '',
  });

  // Datos del cliente seleccionado para mostrar
  const [clienteInfo, setClienteInfo] = useState<{
    nombre: string;
    domicilio: string;
    localidad: string;
    provincia: string;
  } | null>(null);

  // Estado para React Select
  const [selectedCliente, setSelectedCliente] = useState<SelectOption | null>(null);
  const [selectedTransporte, setSelectedTransporte] = useState<SelectOption | null>(null);

  // Opciones para React Select
  const clienteOptions = useMemo<SelectOption[]>(() =>
    clientes.map(c => ({
      value: c.id,
      label: `${c.codigo || ''} - ${c.nombre}`
    })), [clientes]);

  const transporteOptions = useMemo<SelectOption[]>(() =>
    transportes.map(t => ({
      value: t.id,
      label: t.codigo ? `${t.codigo} - ${t.nombre}` : t.nombre
    })), [transportes]);

  useEffect(() => {
    loadInitialData();
  }, []);

  useEffect(() => {
    if (isEditing && id) {
      loadRemito(parseInt(id, 10));
    }
  }, [id, isEditing]);

  const loadInitialData = async () => {
    try {
      setLoading(true);
      const [clientesData, transportesData, subZonasData, provinciasData] = await Promise.all([
        clienteService.getAll(),
        referenceService.getTransportes(),
        referenceService.getSubZonas(),
        referenceService.getProvincias(),
      ]);
      setClientes(clientesData);
      setTransportes(transportesData);
      setSubZonas(subZonasData);
      setProvincias(provinciasData);
    } catch (err) {
      setError('Error al cargar los datos');
      console.error('Error loading initial data:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadRemito = async (remitoId: number) => {
    try {
      setLoading(true);
      const remito = await remitoService.getById(remitoId);
      setFormData({
        clienteId: remito.clienteId,
        transporteId: remito.transporteId,
        cantidadBultos: remito.cantidadBultos,
        valorDeclarado: remito.valorDeclarado,
        observaciones: remito.observaciones || '',
      });

      // Establecer info del cliente
      setClienteInfo({
        nombre: remito.clienteNombre || '',
        domicilio: remito.clienteDomicilio || '',
        localidad: remito.clienteLocalidad || '',
        provincia: remito.clienteProvincia || '',
      });

      // Establecer valores de React Select
      if (remito.clienteId) {
        const clienteLabel = remito.clienteNombre || `Cliente ${remito.clienteId}`;
        setSelectedCliente({ value: remito.clienteId, label: clienteLabel });
      }
      if (remito.transporteId) {
        const transporteLabel = remito.transporteNombre || `Transporte ${remito.transporteId}`;
        setSelectedTransporte({ value: remito.transporteId, label: transporteLabel });
      }
    } catch (err) {
      setError('Error al cargar el remito');
      console.error('Error loading remito:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleClienteSelectChange = (option: SelectOption | null) => {
    setSelectedCliente(option);
    const clienteId = option?.value || 0;

    const cliente = clientes.find((c) => c.id === clienteId);

    if (cliente) {
      // Buscar localidad y provincia de la subzona del cliente
      const subZona = subZonas.find((s) => s.id === cliente.subZona_Id);
      const provincia = provincias.find((p) => p.id === (subZona?.provinciaId || cliente.provincia_Id));

      setClienteInfo({
        nombre: cliente.nombre,
        domicilio: cliente.domicilioComercial || '',
        localidad: subZona?.localidad || '',
        provincia: provincia?.descripcion || '',
      });
    } else {
      setClienteInfo(null);
    }

    setFormData((prev) => ({ ...prev, clienteId }));
  };

  const handleTransporteSelectChange = (option: SelectOption | null) => {
    setSelectedTransporte(option);
    const transporteId = option?.value || 0;
    setFormData((prev) => ({ ...prev, transporteId }));
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>
  ) => {
    const { name, value } = e.target;

    if (name === 'cantidadBultos') {
      setFormData((prev) => ({ ...prev, [name]: parseInt(value, 10) || 0 }));
    } else if (name === 'valorDeclarado') {
      setFormData((prev) => ({ ...prev, [name]: parseFloat(value) || 0 }));
    } else {
      setFormData((prev) => ({ ...prev, [name]: value }));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!formData.clienteId) {
      setError('Debe seleccionar un cliente');
      return;
    }

    if (!formData.transporteId) {
      setError('Debe seleccionar un transporte');
      return;
    }

    if (formData.cantidadBultos < 1) {
      setError('La cantidad de bultos debe ser mayor a 0');
      return;
    }

    if (formData.valorDeclarado <= 0) {
      setError('El valor declarado debe ser mayor a 0');
      return;
    }

    try {
      setLoading(true);

      if (isEditing && id) {
        await remitoService.update(parseInt(id, 10), formData);
      } else {
        await remitoService.create(formData);
      }

      navigate('/remitos');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al guardar el remito');
      console.error('Error saving remito:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <PageHeader
        title={isEditing ? 'Editar Remito' : 'Nuevo Remito'}
        icon="fa-solid fa-file-invoice"
      />

      {error && <div className="alert alert-danger">{error}</div>}

      <form onSubmit={handleSubmit}>
        <div className="card mb-4">
          <div className="card-header">
            <h5 className="mb-0">
              <i className="fa-solid fa-user me-2"></i>
              Datos del Cliente
            </h5>
          </div>
          <div className="card-body">
            <div className="row">
              <div className="col-md-6 mb-3">
                <label className="form-label">
                  Cliente <span className="text-danger">*</span>
                </label>
                <Select
                  isClearable
                  placeholder="Buscar cliente..."
                  options={clienteOptions}
                  value={selectedCliente}
                  onChange={handleClienteSelectChange}
                />
              </div>
              <div className="col-md-6 mb-3">
                <label className="form-label">Domicilio</label>
                <input
                  type="text"
                  className="form-control"
                  value={clienteInfo?.domicilio || ''}
                  readOnly
                  disabled
                />
              </div>
            </div>
            <div className="row">
              <div className="col-md-6 mb-3">
                <label className="form-label">Localidad</label>
                <input
                  type="text"
                  className="form-control"
                  value={clienteInfo?.localidad || ''}
                  readOnly
                  disabled
                />
              </div>
              <div className="col-md-6 mb-3">
                <label className="form-label">Provincia</label>
                <input
                  type="text"
                  className="form-control"
                  value={clienteInfo?.provincia || ''}
                  readOnly
                  disabled
                />
              </div>
            </div>
          </div>
        </div>

        <div className="card mb-4">
          <div className="card-header">
            <h5 className="mb-0">
              <i className="fa-solid fa-truck me-2"></i>
              Datos del Transporte
            </h5>
          </div>
          <div className="card-body">
            <div className="row">
              <div className="col-md-6 mb-3">
                <label className="form-label">
                  Transporte <span className="text-danger">*</span>
                </label>
                <Select
                  isClearable
                  placeholder="Buscar transporte..."
                  options={transporteOptions}
                  value={selectedTransporte}
                  onChange={handleTransporteSelectChange}
                />
              </div>
            </div>
          </div>
        </div>

        <div className="card mb-4">
          <div className="card-header">
            <h5 className="mb-0">
              <i className="fa-solid fa-box me-2"></i>
              Detalle del Envío
            </h5>
          </div>
          <div className="card-body">
            <div className="row">
              <div className="col-md-4 mb-3">
                <label className="form-label">
                  Cantidad de Bultos <span className="text-danger">*</span>
                </label>
                <input
                  type="number"
                  className="form-control"
                  name="cantidadBultos"
                  value={formData.cantidadBultos}
                  onChange={handleChange}
                  onFocus={(e) => e.target.select()}
                  min="1"
                  required
                />
                <small className="text-muted">
                  Se generarán {formData.cantidadBultos} etiqueta(s)
                </small>
              </div>
              <div className="col-md-4 mb-3">
                <label className="form-label">
                  Valor Declarado <span className="text-danger">*</span>
                </label>
                <div className="input-group">
                  <span className="input-group-text">$</span>
                  <input
                    type="number"
                    className="form-control"
                    name="valorDeclarado"
                    value={formData.valorDeclarado}
                    onChange={handleChange}
                    onFocus={(e) => e.target.select()}
                    min="0.01"
                    step="0.01"
                    required
                  />
                </div>
              </div>
            </div>
            <div className="row">
              <div className="col-12 mb-3">
                <label className="form-label">Observaciones</label>
                <textarea
                  className="form-control"
                  name="observaciones"
                  value={formData.observaciones || ''}
                  onChange={handleChange}
                  rows={3}
                  maxLength={500}
                  placeholder="Ingrese observaciones opcionales..."
                />
              </div>
            </div>
          </div>
        </div>

        <div className="d-flex justify-content-end gap-2">
          <button
            type="button"
            className="btn btn-secondary"
            onClick={() => navigate('/remitos')}
            disabled={loading}
          >
            Cancelar
          </button>
          <GradientButton
            type="submit"
            icon={loading ? 'fa-solid fa-spinner fa-spin' : 'fa-solid fa-save'}
            disabled={loading}
          >
            {loading ? 'Guardando...' : 'Guardar'}
          </GradientButton>
        </div>
      </form>
    </div>
  );
};

export default RemitoFormPage;
