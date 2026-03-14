import { useState, useEffect } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { clienteService } from '../services/clienteService';
import { referenceService } from '../services/referenceService';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { GradientCard } from '../components/GradientCard';
import { FormGroup } from '../components/FormGroup';
import { Icon } from '../components/Icon';
import type { CreateClienteDto, UpdateClienteDto } from '../types/cliente';
import type { Zona, SubZona, Provincia, Vendedor } from '../types/references';

const ClienteFormPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEditMode = !!id;

  const [formData, setFormData] = useState<CreateClienteDto | UpdateClienteDto>({
    codigo: '',
    nombre: '',
    zona_Id: undefined,
    subZona_Id: undefined,
    vendedor_Id: undefined,
    provincia_Id: undefined,
    telefono: '',
    telefonoMovil: '',
    domicilioComercial: '',
    domicilioParticular: '',
    codigoPostal: '',
    contacto: '',
    tipoDocumento: 'DNI',
    nroDocumento: '',
    nroIIBB: '',
    categoriaIva: 'Consumidor Final',
    condicionPago: '',
    descuento: 0,
    soloContado: false,
    observaciones: '',
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Referencias
  const [zonas, setZonas] = useState<Zona[]>([]);
  const [subzonas, setSubzonas] = useState<SubZona[]>([]);
  const [provincias, setProvincias] = useState<Provincia[]>([]);
  const [vendedores, setVendedores] = useState<Vendedor[]>([]);

  useEffect(() => {
    loadReferences();
    if (isEditMode && id) {
      loadCliente(parseInt(id));
    }
  }, [id, isEditMode]);

  const loadReferences = async () => {
    try {
      const [zonasData, provinciasData, vendedoresData] = await Promise.all([
        referenceService.getZonas(),
        referenceService.getProvincias(),
        referenceService.getVendedores(),
      ]);
      setZonas(zonasData);
      setProvincias(provinciasData);
      setVendedores(vendedoresData);
    } catch (err) {
      console.error('Error loading references:', err);
    }
  };

  const loadSubzonasByZona = async (zonaId: number) => {
    try {
      const data = await referenceService.getSubZonasByZona(zonaId);
      setSubzonas(data);
    } catch (err) {
      console.error('Error loading subzonas:', err);
      setSubzonas([]);
    }
  };

  const loadCliente = async (clienteId: number) => {
    try {
      setLoading(true);
      const cliente = await clienteService.getById(clienteId);
      if (cliente.zona_Id) {
        await loadSubzonasByZona(cliente.zona_Id);
      }
      setFormData({
        codigo: cliente.codigo || '',
        nombre: cliente.nombre,
        zona_Id: cliente.zona_Id,
        subZona_Id: cliente.subZona_Id,
        vendedor_Id: cliente.vendedor_Id,
        provincia_Id: cliente.provincia_Id,
        telefono: cliente.telefono || '',
        telefonoMovil: cliente.telefonoMovil || '',
        domicilioComercial: cliente.domicilioComercial || '',
        domicilioParticular: cliente.domicilioParticular || '',
        codigoPostal: cliente.codigoPostal || '',
        contacto: cliente.contacto || '',
        tipoDocumento: cliente.tipoDocumento || '',
        nroDocumento: cliente.nroDocumento || '',
        nroIIBB: cliente.nroIIBB || '',
        categoriaIva: cliente.categoriaIva || '',
        condicionPago: cliente.condicionPago || '',
        descuento: cliente.descuento,
        soloContado: cliente.soloContado,
        observaciones: cliente.observaciones || '',
      });
    } catch (err) {
      setError('Error al cargar el cliente');
      console.error('Error loading cliente:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value, type } = e.target;

    // Si cambia la Zona, cargar subzonas de esa zona y limpiar subzona seleccionada
    if (name === 'zona_Id') {
      const zonaId = value ? parseInt(value) : undefined;
      setFormData((prev) => ({ ...prev, zona_Id: zonaId, subZona_Id: undefined }));
      if (zonaId) {
        loadSubzonasByZona(zonaId);
      } else {
        setSubzonas([]);
      }
      return;
    }

    // Si se selecciona una SubZona, autocompletar Provincia y Código Postal
    if (name === 'subZona_Id' && value) {
      const selectedSubZona = subzonas.find(sz => sz.id === parseInt(value));
      if (selectedSubZona) {
        setFormData((prev) => ({
          ...prev,
          subZona_Id: parseInt(value),
          provincia_Id: selectedSubZona.provinciaId || prev.provincia_Id,
          codigoPostal: selectedSubZona.codigoPostal || prev.codigoPostal,
        }));
        return;
      }
    }

    setFormData((prev) => ({
      ...prev,
      [name]:
        type === 'checkbox'
          ? (e.target as HTMLInputElement).checked
          : type === 'number'
          ? parseFloat(value) || 0
          : name.includes('_Id') && value === ''
          ? undefined
          : name.includes('_Id')
          ? parseInt(value)
          : value,
    }));
  };

  // Obtener la localidad de la SubZona seleccionada
  const getLocalidadFromSubZona = (): string => {
    if (!formData.subZona_Id) return '';
    const selectedSubZona = subzonas.find(sz => sz.id === formData.subZona_Id);
    return selectedSubZona?.localidad || '';
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!formData.nombre.trim()) {
      setError('El nombre es obligatorio');
      return;
    }

    if (!formData.nroDocumento || !formData.nroDocumento.trim()) {
      setError('El número de documento es obligatorio');
      return;
    }

    if (!formData.telefono && !formData.telefonoMovil) {
      setError('Debe ingresar al menos un teléfono de contacto');
      return;
    }

    if (!formData.categoriaIva) {
      setError('La categoría IVA es obligatoria');
      return;
    }

    try {
      setLoading(true);
      if (isEditMode && id) {
        await clienteService.update(parseInt(id), formData as UpdateClienteDto);
      } else {
        await clienteService.create(formData as CreateClienteDto);
      }
      navigate('/clientes');
    } catch (err: any) {
      setError(
        err.response?.data?.message ||
          `Error al ${isEditMode ? 'actualizar' : 'crear'} el cliente`
      );
      console.error('Error saving cliente:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <PageHeader
        title={isEditMode ? 'Editar Cliente' : 'Nuevo Cliente'}
        icon="fa-solid fa-users"
        actions={
          <Link to="/clientes" className="btn-secondary-action">
            <Icon name="fa-solid fa-arrow-left" />
            Volver
          </Link>
        }
      />

      {error && (
        <div className="alert alert-danger alert-dismissible fade show" role="alert">
          {error}
          <button
            type="button"
            className="btn-close"
            onClick={() => setError(null)}
            aria-label="Close"
          ></button>
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <GradientCard title="Información General" icon="fa-solid fa-user">
          <div className="row">
            {isEditMode && (
              <div className="col-md-6">
                <FormGroup label="Código">
                  <input
                    type="text"
                    className="form-control"
                    value={formData.codigo}
                    readOnly
                    disabled
                  />
                </FormGroup>
              </div>
            )}

            <div className={isEditMode ? "col-md-6" : "col-md-12"}>
              <FormGroup label="Nombre" required>
                <input
                  type="text"
                  className="form-control"
                  name="nombre"
                  value={formData.nombre}
                  onChange={handleChange}
                  placeholder="Nombre del cliente"
                  required
                />
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Zona">
                <select
                  className="form-select"
                  name="zona_Id"
                  value={formData.zona_Id || ''}
                  onChange={handleChange}
                >
                  <option value="">Seleccione...</option>
                  {zonas.map((zona) => (
                    <option key={zona.id} value={zona.id}>
                      {zona.descripcion}
                    </option>
                  ))}
                </select>
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="SubZona">
                <select
                  className="form-select"
                  name="subZona_Id"
                  value={formData.subZona_Id || ''}
                  onChange={handleChange}
                >
                  <option value="">Seleccione...</option>
                  {subzonas.map((subzona) => (
                    <option key={subzona.id} value={subzona.id}>
                      {subzona.descripcion}
                    </option>
                  ))}
                </select>
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Vendedor">
                <select
                  className="form-select"
                  name="vendedor_Id"
                  value={formData.vendedor_Id || ''}
                  onChange={handleChange}
                >
                  <option value="">Seleccione...</option>
                  {vendedores.map((vendedor) => (
                    <option key={vendedor.id} value={vendedor.id}>
                      {vendedor.descripcion}
                    </option>
                  ))}
                </select>
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Persona de Contacto">
                <input
                  type="text"
                  className="form-control"
                  name="contacto"
                  value={formData.contacto}
                  onChange={handleChange}
                  placeholder="Nombre del contacto"
                />
              </FormGroup>
            </div>
          </div>
        </GradientCard>

        <GradientCard title="Datos Fiscales" icon="fa-solid fa-file-lines" className="mt-4">
          <div className="row">
            <div className="col-md-6">
              <FormGroup label="Tipo de Documento" required>
                <select
                  className="form-select"
                  name="tipoDocumento"
                  value={formData.tipoDocumento}
                  onChange={handleChange}
                  required
                >
                  <option value="">Seleccione...</option>
                  <option value="DNI">DNI</option>
                  <option value="CUIT">CUIT</option>
                  <option value="CUIL">CUIL</option>
                </select>
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Número de Documento" required>
                <input
                  type="text"
                  className="form-control"
                  name="nroDocumento"
                  value={formData.nroDocumento}
                  onChange={handleChange}
                  placeholder="Número de documento"
                  required
                />
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Número de IIBB">
                <input
                  type="text"
                  className="form-control"
                  name="nroIIBB"
                  value={formData.nroIIBB}
                  onChange={handleChange}
                  placeholder="Número de Ingresos Brutos"
                />
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Categoría IVA" required>
                <select
                  className="form-select"
                  name="categoriaIva"
                  value={formData.categoriaIva}
                  onChange={handleChange}
                  required
                >
                  <option value="">Seleccione...</option>
                  <option value="Responsable Inscripto">Responsable Inscripto</option>
                  <option value="Monotributista">Monotributista</option>
                  <option value="Exento">Exento</option>
                  <option value="Consumidor Final">Consumidor Final</option>
                </select>
              </FormGroup>
            </div>
          </div>
        </GradientCard>

        <GradientCard title="Contacto" icon="fa-solid fa-phone" className="mt-4">
          <div className="row">
            <div className="col-md-6">
              <FormGroup label="Teléfono">
                <input
                  type="text"
                  className="form-control"
                  name="telefono"
                  value={formData.telefono}
                  onChange={handleChange}
                  placeholder="Teléfono fijo"
                />
                <small className="form-text text-muted">
                  * Debe ingresar al menos un teléfono (fijo o móvil)
                </small>
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Teléfono Móvil">
                <input
                  type="text"
                  className="form-control"
                  name="telefonoMovil"
                  value={formData.telefonoMovil}
                  onChange={handleChange}
                  placeholder="Teléfono móvil"
                />
              </FormGroup>
            </div>

          </div>
        </GradientCard>

        <GradientCard title="Domicilio" icon="fa-solid fa-building" className="mt-4">
          <div className="row">
            <div className="col-md-6">
              <FormGroup label="Domicilio Comercial">
                <input
                  type="text"
                  className="form-control"
                  name="domicilioComercial"
                  value={formData.domicilioComercial}
                  onChange={handleChange}
                  placeholder="Dirección comercial"
                />
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Domicilio Particular">
                <input
                  type="text"
                  className="form-control"
                  name="domicilioParticular"
                  value={formData.domicilioParticular}
                  onChange={handleChange}
                  placeholder="Dirección particular"
                />
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Provincia">
                <select
                  className="form-select"
                  name="provincia_Id"
                  value={formData.provincia_Id || ''}
                  onChange={handleChange}
                >
                  <option value="">Seleccione...</option>
                  {provincias.map((provincia) => (
                    <option key={provincia.id} value={provincia.id}>
                      {provincia.descripcion}
                    </option>
                  ))}
                </select>
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Código Postal">
                <input
                  type="text"
                  className="form-control"
                  name="codigoPostal"
                  value={formData.codigoPostal}
                  onChange={handleChange}
                  placeholder="Código postal"
                />
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Localidad">
                <input
                  type="text"
                  className="form-control"
                  value={getLocalidadFromSubZona()}
                  placeholder=""
                  readOnly
                  disabled
                />
                <small className="form-text text-muted">
                  Se completa automáticamente al seleccionar una SubZona
                </small>
              </FormGroup>
            </div>
          </div>
        </GradientCard>

        <GradientCard title="Observaciones" icon="fa-solid fa-clipboard" className="mt-4">
          <div className="row">
            <div className="col">
              <FormGroup label="Observaciones">
                <textarea
                  className="form-control"
                  rows={3}
                  name="observaciones"
                  value={formData.observaciones}
                  onChange={handleChange}
                  placeholder="Observaciones adicionales"
                />
              </FormGroup>
            </div>
          </div>
        </GradientCard>

        <div className="d-flex gap-2 mt-4">
          <GradientButton type="submit" icon="fa-solid fa-floppy-disk" disabled={loading}>
            {loading ? 'Guardando...' : isEditMode ? 'Actualizar' : 'Crear'}
          </GradientButton>
          <Link to="/clientes" className="btn-secondary-action">
            Cancelar
          </Link>
        </div>
      </form>
    </div>
  );
};

export default ClienteFormPage;
