import { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { comprobanteProveedorService } from '../services/comprobanteProveedorService';
import { proveedorService } from '../services/proveedorService';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { GradientCard } from '../components/GradientCard';
import { FormGroup } from '../components/FormGroup';
import { Icon } from '../components/Icon';
import { showConfirmDialog, showSuccessAlert, showErrorAlert } from '../utils/sweetalert';
import type { CreateComprobanteProveedorDto, CuotaPreview } from '../types/comprobanteProveedor';
import type { Proveedor } from '../types/proveedor';

const ComprobanteProveedorFormPage = () => {
  const navigate = useNavigate();

  const [formData, setFormData] = useState<CreateComprobanteProveedorDto>({
    proveedor_Id: 0,
    tipoComprobante: 'FC',
    fechaEmision: new Date().toISOString().split('T')[0],
    nroComprobante: '',
    importeTotal: 0,
    cantidadCuotas: 1,
    fechaPrimerVencimiento: new Date().toISOString().split('T')[0],
  });

  const [proveedores, setProveedores] = useState<Proveedor[]>([]);
  const [cuotasPreview, setCuotasPreview] = useState<CuotaPreview[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadProveedores();
  }, []);

  useEffect(() => {
    recalcularCuotas();
  }, [formData.importeTotal, formData.cantidadCuotas, formData.fechaPrimerVencimiento]);

  const loadProveedores = async () => {
    try {
      const data = await proveedorService.getAll();
      setProveedores(data);
    } catch (err) {
      console.error('Error loading proveedores:', err);
      setProveedores([]);
    }
  };

  const addMonthsSafe = (dateStr: string, months: number): string => {
    const date = new Date(dateStr + 'T00:00:00');
    const originalDay = date.getDate();
    date.setMonth(date.getMonth() + months);
    // If the day changed (e.g. Jan 31 + 1 month = Mar 3), go back to last day of target month
    if (date.getDate() !== originalDay) {
      date.setDate(0); // sets to last day of previous month
    }
    return date.toISOString().split('T')[0];
  };

  const recalcularCuotas = () => {
    const { importeTotal, cantidadCuotas, fechaPrimerVencimiento } = formData;

    if (importeTotal <= 0 || cantidadCuotas <= 0 || !fechaPrimerVencimiento) {
      setCuotasPreview([]);
      return;
    }

    const valorBase = Math.round((importeTotal / cantidadCuotas) * 100) / 100;
    const ultimaCuota = Math.round((importeTotal - valorBase * (cantidadCuotas - 1)) * 100) / 100;

    const cuotas: CuotaPreview[] = [];
    for (let i = 1; i <= cantidadCuotas; i++) {
      cuotas.push({
        numeroCuota: i,
        fechaVencimiento: addMonthsSafe(fechaPrimerVencimiento, i - 1),
        importe: i === cantidadCuotas ? ultimaCuota : valorBase,
      });
    }

    setCuotasPreview(cuotas);
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value, type } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'number' ? parseFloat(value) || 0 : value,
    }));
  };

  const handleProveedorChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setFormData((prev) => ({
      ...prev,
      proveedor_Id: parseInt(e.target.value) || 0,
    }));
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('es-AR', {
      style: 'currency',
      currency: 'ARS',
    }).format(amount);
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString + 'T00:00:00');
    return date.toLocaleDateString('es-AR');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (formData.proveedor_Id === 0) {
      setError('Debe seleccionar un proveedor');
      return;
    }

    if (!formData.nroComprobante.trim()) {
      setError('El número de comprobante es obligatorio');
      return;
    }

    if (formData.importeTotal <= 0) {
      setError('El importe total debe ser mayor a 0');
      return;
    }

    if (formData.cantidadCuotas <= 0) {
      setError('La cantidad de cuotas debe ser mayor a 0');
      return;
    }

    const result = await showConfirmDialog(
      'Confirmar comprobante',
      `Se creará el comprobante ${formData.nroComprobante} con ${formData.cantidadCuotas} cuota(s) por ${formatCurrency(formData.importeTotal)}. ¿Desea continuar?`
    );

    if (!result.isConfirmed) return;

    try {
      setLoading(true);
      await comprobanteProveedorService.create(formData);
      await showSuccessAlert('Comprobante creado', 'El comprobante de proveedor ha sido registrado correctamente');
      navigate('/comprobantes-proveedores');
    } catch (err: any) {
      setError(
        err.response?.data?.message || 'Error al crear el comprobante de proveedor'
      );
      await showErrorAlert('Error', 'No se pudo crear el comprobante');
      console.error('Error creating comprobante proveedor:', err);
    } finally {
      setLoading(false);
    }
  };

  const totalCuotas = cuotasPreview.reduce((sum, c) => sum + c.importe, 0);

  return (
    <div>
      <PageHeader
        title="Nuevo Comprobante de Proveedor"
        icon="fa-solid fa-file-invoice"
        actions={
          <Link to="/comprobantes-proveedores" className="btn-secondary-action">
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
        <GradientCard title="Datos del Comprobante" icon="fa-solid fa-file-invoice-dollar">
          <div className="row">
            <div className="col-md-6">
              <FormGroup label="Proveedor" required>
                <select
                  className="form-select"
                  name="proveedor_Id"
                  value={formData.proveedor_Id}
                  onChange={handleProveedorChange}
                  required
                >
                  <option value={0}>Seleccione un proveedor...</option>
                  {proveedores.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.nombre}
                    </option>
                  ))}
                </select>
              </FormGroup>
            </div>

            <div className="col-md-3">
              <FormGroup label="Tipo" required>
                <select
                  className="form-select"
                  name="tipoComprobante"
                  value={formData.tipoComprobante}
                  onChange={handleChange}
                  required
                >
                  <option value="FC">Factura (FC)</option>
                  <option value="NC">Nota de Crédito (NC)</option>
                  <option value="ND">Nota de Débito (ND)</option>
                </select>
              </FormGroup>
            </div>

            <div className="col-md-3">
              <FormGroup label="Fecha Emisión" required>
                <input
                  type="date"
                  className="form-control"
                  name="fechaEmision"
                  value={formData.fechaEmision}
                  onChange={handleChange}
                  required
                />
              </FormGroup>
            </div>

            <div className="col-md-4">
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

            <div className="col-md-4">
              <FormGroup label="Importe Total" required>
                <div className="input-group">
                  <span className="input-group-text">$</span>
                  <input
                    type="number"
                    className="form-control"
                    name="importeTotal"
                    value={formData.importeTotal}
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

            <div className="col-md-2">
              <FormGroup label="Cuotas" required>
                <input
                  type="number"
                  className="form-control"
                  name="cantidadCuotas"
                  value={formData.cantidadCuotas}
                  onChange={handleChange}
                  onFocus={(e) => e.target.select()}
                  min="1"
                  step="1"
                  required
                />
              </FormGroup>
            </div>

            <div className="col-md-2">
              <FormGroup label="1er Vencimiento" required>
                <input
                  type="date"
                  className="form-control"
                  name="fechaPrimerVencimiento"
                  value={formData.fechaPrimerVencimiento}
                  onChange={handleChange}
                  required
                />
              </FormGroup>
            </div>
          </div>
        </GradientCard>

        {cuotasPreview.length > 0 && (
          <GradientCard title="Previsualización de Cuotas" icon="fa-solid fa-list-ol">
            <div className="table-responsive">
              <table className="custom-table">
                <thead>
                  <tr>
                    <th># Cuota</th>
                    <th>Fecha Vencimiento</th>
                    <th className="text-end">Importe</th>
                  </tr>
                </thead>
                <tbody>
                  {cuotasPreview.map((cuota) => (
                    <tr key={cuota.numeroCuota}>
                      <td>{cuota.numeroCuota}</td>
                      <td>{formatDate(cuota.fechaVencimiento)}</td>
                      <td className="text-end">{formatCurrency(cuota.importe)}</td>
                    </tr>
                  ))}
                </tbody>
                <tfoot>
                  <tr className="fw-bold">
                    <td colSpan={2}>Total</td>
                    <td className="text-end">{formatCurrency(totalCuotas)}</td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </GradientCard>
        )}

        <div className="d-flex gap-2 mt-4">
          <GradientButton type="submit" icon="fa-solid fa-floppy-disk" disabled={loading}>
            {loading ? 'Guardando...' : 'Crear Comprobante'}
          </GradientButton>
          <Link to="/comprobantes-proveedores" className="btn-secondary-action">
            Cancelar
          </Link>
        </div>
      </form>
    </div>
  );
};

export default ComprobanteProveedorFormPage;
