import { useState, useEffect } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { gastoService } from '../services/gastoService';
import { categoriaGastoService } from '../services/categoriaGastoService';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { GradientCard } from '../components/GradientCard';
import { FormGroup } from '../components/FormGroup';
import { Icon } from '../components/Icon';
import type { CreateGastoDto, UpdateGastoDto } from '../types/gasto';
import type { CategoriaGasto } from '../types/categoriaGasto';

const GastoFormPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEditMode = !!id;

  const [formData, setFormData] = useState<CreateGastoDto | UpdateGastoDto>({
    nroComprobante: '',
    importe: 0,
    categoria: '',
    descripcion: '',
    fecha: new Date().toISOString().split('T')[0],
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [categorias, setCategorias] = useState<CategoriaGasto[]>([]);

  useEffect(() => {
    loadCategorias();
    if (isEditMode && id) {
      loadGasto(parseInt(id));
    }
  }, [id, isEditMode]);

  const loadCategorias = async () => {
    try {
      const data = await categoriaGastoService.getActivas();
      setCategorias(data);
    } catch (err) {
      console.error('Error loading categorias:', err);
      setCategorias([]);
    }
  };

  const loadGasto = async (gastoId: number) => {
    try {
      setLoading(true);
      const gasto = await gastoService.getById(gastoId);
      setFormData({
        nroComprobante: gasto.nroComprobante,
        importe: gasto.importe,
        categoria: gasto.categoria,
        descripcion: gasto.descripcion,
        fecha: gasto.fecha.split('T')[0],
      });
    } catch (err) {
      setError('Error al cargar el gasto');
      console.error('Error loading gasto:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value, type } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'number' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!formData.nroComprobante.trim()) {
      setError('El número de comprobante es obligatorio');
      return;
    }

    if (formData.importe <= 0) {
      setError('El importe debe ser mayor a 0');
      return;
    }

    if (!formData.categoria.trim()) {
      setError('La categoría es obligatoria');
      return;
    }

    if (!formData.descripcion.trim()) {
      setError('La descripción es obligatoria');
      return;
    }

    if (!formData.fecha) {
      setError('La fecha es obligatoria');
      return;
    }

    try {
      setLoading(true);
      if (isEditMode && id) {
        await gastoService.update(parseInt(id), formData as UpdateGastoDto);
      } else {
        await gastoService.create(formData as CreateGastoDto);
      }
      navigate('/gastos');
    } catch (err: any) {
      setError(
        err.response?.data?.message ||
          `Error al ${isEditMode ? 'actualizar' : 'crear'} el gasto`
      );
      console.error('Error saving gasto:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <PageHeader
        title={isEditMode ? 'Editar Gasto' : 'Nuevo Gasto'}
        icon="fa-solid fa-receipt"
        actions={
          <Link to="/gastos" className="btn-secondary-action">
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
        <GradientCard title="Datos del Gasto" icon="fa-solid fa-file-invoice-dollar">
          <div className="row">
            <div className="col-md-6">
              <FormGroup label="Nro. Comprobante" required>
                <input
                  type="text"
                  className="form-control"
                  name="nroComprobante"
                  value={formData.nroComprobante}
                  onChange={handleChange}
                  placeholder="Ej: 0001-00001234"
                  required
                />
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Fecha" required>
                <input
                  type="date"
                  className="form-control"
                  name="fecha"
                  value={formData.fecha}
                  onChange={handleChange}
                  required
                />
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Categoría" required>
                <select
                  className="form-select"
                  name="categoria"
                  value={formData.categoria}
                  onChange={handleChange}
                  required
                >
                  <option value="">Seleccione una categoría...</option>
                  {categorias.map((cat) => (
                    <option key={cat.id} value={cat.nombre}>
                      {cat.nombre}
                    </option>
                  ))}
                </select>
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Importe" required>
                <div className="input-group">
                  <span className="input-group-text">$</span>
                  <input
                    type="number"
                    className="form-control"
                    name="importe"
                    value={formData.importe}
                    onChange={handleChange}
                    onFocus={(e) => e.target.select()}
                    min="0.01"
                    step="0.01"
                    placeholder="0.00"
                    required
                  />
                </div>
              </FormGroup>
            </div>

            <div className="col-12">
              <FormGroup label="Descripción" required>
                <textarea
                  className="form-control"
                  rows={3}
                  name="descripcion"
                  value={formData.descripcion}
                  onChange={handleChange}
                  placeholder="Descripción detallada del gasto"
                  required
                />
              </FormGroup>
            </div>
          </div>
        </GradientCard>

        <div className="d-flex gap-2 mt-4">
          <GradientButton type="submit" icon="fa-solid fa-floppy-disk" disabled={loading}>
            {loading ? 'Guardando...' : isEditMode ? 'Actualizar' : 'Crear'}
          </GradientButton>
          <Link to="/gastos" className="btn-secondary-action">
            Cancelar
          </Link>
        </div>
      </form>
    </div>
  );
};

export default GastoFormPage;
