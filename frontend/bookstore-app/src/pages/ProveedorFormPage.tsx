import { useState, useEffect } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { proveedorService } from '../services/proveedorService';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { GradientCard } from '../components/GradientCard';
import { FormGroup } from '../components/FormGroup';
import { Icon } from '../components/Icon';
import type { CreateProveedorDto, UpdateProveedorDto } from '../types/proveedor';

const ProveedorFormPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEditMode = !!id;

  const [formData, setFormData] = useState<CreateProveedorDto | UpdateProveedorDto>({
    nombre: '',
    razonSocial: '',
    cuit: '',
    domicilio: '',
    telefono: '',
    mail: '',
  });

  const [codigo, setCodigo] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isEditMode && id) {
      loadProveedor(parseInt(id));
    }
  }, [id, isEditMode]);

  const loadProveedor = async (proveedorId: number) => {
    try {
      setLoading(true);
      const proveedor = await proveedorService.getById(proveedorId);
      setCodigo(proveedor.codigo || '');
      setFormData({
        nombre: proveedor.nombre,
        razonSocial: proveedor.razonSocial || '',
        cuit: proveedor.cuit || '',
        domicilio: proveedor.domicilio || '',
        telefono: proveedor.telefono || '',
        mail: proveedor.mail || '',
        fechaInhabilitacion: proveedor.fechaInhabilitacion || '',
      });
    } catch (err) {
      setError('Error al cargar el proveedor');
      console.error('Error loading proveedor:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;

    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!formData.nombre.trim()) {
      setError('El nombre es obligatorio');
      return;
    }

    try {
      setLoading(true);
      if (isEditMode && id) {
        await proveedorService.update(parseInt(id), formData as UpdateProveedorDto);
      } else {
        await proveedorService.create(formData as CreateProveedorDto);
      }
      navigate('/proveedores');
    } catch (err: any) {
      setError(
        err.response?.data?.message ||
          `Error al ${isEditMode ? 'actualizar' : 'crear'} el proveedor`
      );
      console.error('Error saving proveedor:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <PageHeader
        title={isEditMode ? 'Editar Proveedor' : 'Nuevo Proveedor'}
        icon="fa-solid fa-truck-field"
        actions={
          <Link to="/proveedores" className="btn-secondary-action">
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
        <GradientCard title="Información General" icon="fa-solid fa-building">
          <div className="row">
            {isEditMode && (
              <div className="col-md-6">
                <FormGroup label="Código">
                  <input
                    type="text"
                    className="form-control"
                    value={codigo}
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
                  placeholder="Nombre del proveedor"
                  required
                />
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Razón Social">
                <input
                  type="text"
                  className="form-control"
                  name="razonSocial"
                  value={formData.razonSocial}
                  onChange={handleChange}
                  placeholder="Razón social"
                />
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="CUIT">
                <input
                  type="text"
                  className="form-control"
                  name="cuit"
                  value={formData.cuit}
                  onChange={handleChange}
                  placeholder="XX-XXXXXXXX-X"
                />
              </FormGroup>
            </div>
          </div>
        </GradientCard>

        <GradientCard title="Contacto" icon="fa-solid fa-phone" className="mt-4">
          <div className="row">
            <div className="col-md-12">
              <FormGroup label="Domicilio">
                <input
                  type="text"
                  className="form-control"
                  name="domicilio"
                  value={formData.domicilio}
                  onChange={handleChange}
                  placeholder="Dirección del proveedor"
                />
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Teléfono">
                <input
                  type="text"
                  className="form-control"
                  name="telefono"
                  value={formData.telefono}
                  onChange={handleChange}
                  placeholder="Teléfono"
                />
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Email">
                <input
                  type="email"
                  className="form-control"
                  name="mail"
                  value={formData.mail}
                  onChange={handleChange}
                  placeholder="correo@ejemplo.com"
                />
              </FormGroup>
            </div>
          </div>
        </GradientCard>

        {isEditMode && (
          <GradientCard title="Estado" icon="fa-solid fa-circle-info" className="mt-4">
            <div className="row">
              <div className="col-md-6">
                <FormGroup label="Fecha de Inhabilitación">
                  <input
                    type="date"
                    className="form-control"
                    name="fechaInhabilitacion"
                    value={(formData as UpdateProveedorDto).fechaInhabilitacion ? (formData as UpdateProveedorDto).fechaInhabilitacion!.substring(0, 10) : ''}
                    onChange={handleChange}
                  />
                  <small className="form-text text-muted">
                    Dejar vacío si el proveedor está activo
                  </small>
                </FormGroup>
              </div>
            </div>
          </GradientCard>
        )}

        <div className="d-flex gap-2 mt-4">
          <GradientButton type="submit" icon="fa-solid fa-floppy-disk" disabled={loading}>
            {loading ? 'Guardando...' : isEditMode ? 'Actualizar' : 'Crear'}
          </GradientButton>
          <Link to="/proveedores" className="btn-secondary-action">
            Cancelar
          </Link>
        </div>
      </form>
    </div>
  );
};

export default ProveedorFormPage;
