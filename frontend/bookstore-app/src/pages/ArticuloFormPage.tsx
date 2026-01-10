import { useState, useEffect } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { articuloService } from '../services/articuloService';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { GradientCard } from '../components/GradientCard';
import { FormGroup } from '../components/FormGroup';
import { Icon } from '../components/Icon';
import type { CreateArticuloDto, UpdateArticuloDto } from '../types/articulo';

const ArticuloFormPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEditMode = !!id;

  const [formData, setFormData] = useState<CreateArticuloDto | UpdateArticuloDto>({
    codigo: '',
    descripcion: '',
    codBarras: '',
    observaciones: '',
    tomos: undefined,
    tema: '',
    precio: undefined,
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isEditMode && id) {
      loadArticulo(parseInt(id));
    }
  }, [id, isEditMode]);

  const loadArticulo = async (articuloId: number) => {
    try {
      setLoading(true);
      const articulo = await articuloService.getById(articuloId);
      setFormData({
        codigo: articulo.codigo || '',
        descripcion: articulo.descripcion || '',
        codBarras: articulo.codBarras || '',
        observaciones: articulo.observaciones || '',
        tomos: articulo.tomos,
        tema: articulo.tema || '',
        precio: articulo.precio,
      });
    } catch (err) {
      setError('Error al cargar el artículo');
      console.error('Error loading articulo:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const { name, value, type } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]:
        type === 'number'
          ? value === ''
            ? undefined
            : parseFloat(value)
          : value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!formData.descripcion.trim()) {
      setError('La descripción es obligatoria');
      return;
    }

    try {
      setLoading(true);
      if (isEditMode && id) {
        await articuloService.update(parseInt(id), formData as UpdateArticuloDto);
      } else {
        await articuloService.create(formData as CreateArticuloDto);
      }
      navigate('/articulos');
    } catch (err: any) {
      setError(
        err.response?.data?.message ||
          `Error al ${isEditMode ? 'actualizar' : 'crear'} el artículo`
      );
      console.error('Error saving articulo:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <PageHeader
        title={isEditMode ? 'Editar Artículo' : 'Nuevo Artículo'}
        icon="fa-solid fa-box"
        actions={
          <Link to="/articulos" className="btn-secondary-action">
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
        <GradientCard title="Información del Artículo" icon="fa-solid fa-box">
          <div className="row">
            <div className="col-md-6">
              <FormGroup label="Código">
                <input
                  type="text"
                  className="form-control"
                  name="codigo"
                  value={formData.codigo}
                  onChange={handleChange}
                  placeholder="Código del artículo"
                  maxLength={25}
                />
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Descripción" required>
                <input
                  type="text"
                  className="form-control"
                  name="descripcion"
                  value={formData.descripcion}
                  onChange={handleChange}
                  placeholder="Descripción del artículo"
                  maxLength={100}
                  required
                />
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Código de Barras">
                <input
                  type="text"
                  className="form-control"
                  name="codBarras"
                  value={formData.codBarras}
                  onChange={handleChange}
                  placeholder="Código de barras (EAN-13)"
                  maxLength={13}
                />
                <small className="form-text text-muted">
                  Formato EAN-13 (13 dígitos)
                </small>
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Tema">
                <input
                  type="text"
                  className="form-control"
                  name="tema"
                  value={formData.tema}
                  onChange={handleChange}
                  placeholder="Tema o categoría"
                  maxLength={50}
                />
              </FormGroup>
            </div>
          </div>
        </GradientCard>

        <GradientCard title="Detalles Adicionales" icon="fa-solid fa-clipboard" className="mt-4">
          <div className="row">
            <div className="col-md-6">
              <FormGroup label="Tomos">
                <input
                  type="number"
                  className="form-control"
                  name="tomos"
                  value={formData.tomos || ''}
                  onChange={handleChange}
                  onFocus={(e) => e.target.select()}
                  placeholder="Número de tomos"
                  min="0"
                />
                <small className="form-text text-muted">
                  Cantidad de tomos o volúmenes
                </small>
              </FormGroup>
            </div>

            <div className="col-md-6">
              <FormGroup label="Precio" required>
                <div className="input-group">
                  <span className="input-group-text">$</span>
                  <input
                    type="number"
                    className="form-control"
                    name="precio"
                    value={formData.precio || ''}
                    onChange={handleChange}
                    onFocus={(e) => e.target.select()}
                    placeholder="0.00"
                    min="0"
                    step="0.01"
                    required
                  />
                </div>
              </FormGroup>
            </div>

            <div className="col-12">
              <FormGroup label="Observaciones">
                <textarea
                  className="form-control"
                  rows={5}
                  name="observaciones"
                  value={formData.observaciones}
                  onChange={handleChange}
                  placeholder="Observaciones o notas adicionales"
                  maxLength={2000}
                />
                <small className="form-text text-muted">
                  Máximo 2000 caracteres
                </small>
              </FormGroup>
            </div>
          </div>
        </GradientCard>

        <div className="d-flex gap-2 mt-4">
          <GradientButton type="submit" icon="fa-solid fa-floppy-disk" disabled={loading}>
            {loading ? 'Guardando...' : isEditMode ? 'Actualizar' : 'Crear'}
          </GradientButton>
          <Link to="/articulos" className="btn-secondary-action">
            Cancelar
          </Link>
        </div>
      </form>
    </div>
  );
};

export default ArticuloFormPage;
