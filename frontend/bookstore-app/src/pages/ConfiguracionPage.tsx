import { useState, useEffect } from 'react';
import { configuracionService } from '../services/configuracionService';
import type { AfipConfigDto, AfipConfigUpdateDto, UltimoComprobanteDto } from '../types/configuracion';
import { showSuccessAlert, showErrorAlert, showConfirmDialog } from '../utils/sweetalert';
import { PageHeader } from '../components/PageHeader';
import { GradientButton } from '../components/GradientButton';
import { Icon } from '../components/Icon';

const ConfiguracionPage = () => {
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [config, setConfig] = useState<AfipConfigDto | null>(null);

  // Form fields
  const [cuit, setCuit] = useState('');
  const [wsaaUrl, setWsaaUrl] = useState('');
  const [wsfevUrl, setWsfevUrl] = useState('');
  const [puntoVenta, setPuntoVenta] = useState(0);
  const [isProduction, setIsProduction] = useState(false);
  const [crtFile, setCrtFile] = useState<File | null>(null);
  const [keyFile, setKeyFile] = useState<File | null>(null);

  // Ultimo comprobante
  const [ultimosComprobantes, setUltimosComprobantes] = useState<UltimoComprobanteDto[]>([]);
  const [loadingComprobantes, setLoadingComprobantes] = useState(false);

  // Backup
  const [backupPath, setBackupPath] = useState('');
  const [backupPathSaved, setBackupPathSaved] = useState('');
  const [savingPath, setSavingPath] = useState(false);
  const [backingUp, setBackingUp] = useState(false);
  const [restoring, setRestoring] = useState(false);
  const [backupFiles, setBackupFiles] = useState<string[]>([]);
  const [selectedFile, setSelectedFile] = useState('');
  const [purging, setPurging] = useState(false);

  useEffect(() => {
    loadConfig();
    loadBackupConfig();
  }, []);

  const loadConfig = async () => {
    try {
      setLoading(true);
      const data = await configuracionService.getAfipConfig();
      setConfig(data);
      setCuit(data.cuit);
      setWsaaUrl(data.wsaaUrl);
      setWsfevUrl(data.wsfevUrl);
      setPuntoVenta(data.puntoVenta);
      setIsProduction(data.isProduction);
    } catch (err) {
      console.error('Error loading config:', err);
      await showErrorAlert('Error', 'No se pudo cargar la configuración');
    } finally {
      setLoading(false);
    }
  };

  const loadBackupConfig = async () => {
    try {
      const ruta = await configuracionService.getBackupPath();
      setBackupPath(ruta);
      setBackupPathSaved(ruta);
      if (ruta) {
        const archivos = await configuracionService.listarArchivosBackup();
        setBackupFiles(archivos);
      }
    } catch (err) {
      console.error('Error loading backup config:', err);
    }
  };

  const handleSaveBackupPath = async () => {
    try {
      setSavingPath(true);
      await configuracionService.setBackupPath(backupPath);
      setBackupPathSaved(backupPath);
      await showSuccessAlert('Guardado', 'Ruta de backup guardada correctamente');
      const archivos = await configuracionService.listarArchivosBackup();
      setBackupFiles(archivos);
    } catch (err) {
      console.error('Error saving backup path:', err);
      await showErrorAlert('Error', 'No se pudo guardar la ruta de backup');
    } finally {
      setSavingPath(false);
    }
  };

  const handleBackup = async () => {
    try {
      setBackingUp(true);
      const result = await configuracionService.realizarBackup();
      await showSuccessAlert('Backup realizado', `Archivo: ${result.archivo}`);
      const archivos = await configuracionService.listarArchivosBackup();
      setBackupFiles(archivos);
    } catch (err) {
      console.error('Error en backup:', err);
      await showErrorAlert('Error', 'No se pudo realizar el backup');
    } finally {
      setBackingUp(false);
    }
  };

  const handleRestore = async () => {
    if (!selectedFile) {
      await showErrorAlert('Error', 'Seleccione un archivo de backup para restaurar');
      return;
    }
    const confirmed = await showConfirmDialog(
      '¿Restaurar backup?',
      `Se restaurará el archivo "${selectedFile}". Esta acción reemplazará los datos actuales.`
    );
    if (!confirmed.isConfirmed) return;

    try {
      setRestoring(true);
      await configuracionService.restaurarBackup(selectedFile);
      await showSuccessAlert('Restaurado', 'Backup restaurado correctamente');
    } catch (err) {
      console.error('Error en restore:', err);
      await showErrorAlert('Error', 'No se pudo restaurar el backup');
    } finally {
      setRestoring(false);
    }
  };

  const handleVaciarDatos = async () => {
    const confirmed = await showConfirmDialog(
      '¿Vaciar base de datos?',
      'Se eliminarán TODOS los datos (clientes, artículos, comprobantes, etc). Solo se conservará la configuración. Esta acción NO se puede deshacer.'
    );
    if (!confirmed.isConfirmed) return;

    try {
      setPurging(true);
      await configuracionService.vaciarDatos();
      await showSuccessAlert('Listo', 'Base de datos vaciada correctamente. La configuración se mantuvo.');
    } catch (err) {
      console.error('Error al vaciar datos:', err);
      await showErrorAlert('Error', 'No se pudo vaciar la base de datos');
    } finally {
      setPurging(false);
    }
  };

  const fileToBase64 = (file: File): Promise<string> => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => {
        const result = reader.result as string;
        // Remove data URL prefix if present
        const base64 = result.includes(',') ? result.split(',')[1] : result;
        resolve(base64);
      };
      reader.onerror = reject;
      reader.readAsDataURL(file);
    });
  };

  const handleSave = async () => {
    try {
      setSaving(true);
      const updateDto: AfipConfigUpdateDto = {
        cuit,
        wsaaUrl,
        wsfevUrl,
        puntoVenta,
        isProduction,
      };

      if (crtFile) {
        updateDto.crtBase64 = await fileToBase64(crtFile);
      }
      if (keyFile) {
        updateDto.keyBase64 = await fileToBase64(keyFile);
      }

      await configuracionService.updateAfipConfig(updateDto);
      await showSuccessAlert('Guardado', 'Configuración guardada correctamente');
      await loadConfig();
      setCrtFile(null);
      setKeyFile(null);
    } catch (err) {
      console.error('Error saving config:', err);
      await showErrorAlert('Error', 'No se pudo guardar la configuración');
    } finally {
      setSaving(false);
    }
  };

  const handleConsultarComprobantes = async () => {
    try {
      setLoadingComprobantes(true);
      const data = await configuracionService.getUltimoComprobante(puntoVenta || undefined);
      setUltimosComprobantes(data);
    } catch (err) {
      console.error('Error loading comprobantes:', err);
      await showErrorAlert('Error', 'No se pudo consultar los comprobantes. Verifique la configuración de AFIP.');
    } finally {
      setLoadingComprobantes(false);
    }
  };

  if (loading) {
    return (
      <div>
        <PageHeader title="Configuración" icon="fa-solid fa-gear" />
        <div className="text-center py-5">
          <div className="spinner-gradient" />
        </div>
      </div>
    );
  }

  return (
    <div>
      <PageHeader title="Configuración" icon="fa-solid fa-gear" />

      {/* Backup Panel */}
      <div className="card mb-4">
        <div className="card-header">
          <h5 className="mb-0">
            <Icon name="fa-solid fa-database" /> Backup de Base de Datos
          </h5>
        </div>
        <div className="card-body">
          <div className="row align-items-end mb-3">
            <div className="col-md-8 mb-2">
              <label className="form-label">Carpeta de Backup (ruta en el servidor)</label>
              <input
                type="text"
                className="form-control"
                value={backupPath}
                onChange={(e) => setBackupPath(e.target.value)}
                placeholder="C:\Backups\BookstoreApp"
              />
            </div>
            <div className="col-md-4 mb-2">
              <GradientButton
                icon={savingPath ? 'fa-solid fa-spinner fa-spin' : 'fa-solid fa-save'}
                onClick={handleSaveBackupPath}
                disabled={savingPath || backupPath === backupPathSaved}
              >
                {savingPath ? 'Guardando...' : 'Guardar Ruta'}
              </GradientButton>
            </div>
          </div>

          <hr />

          <div className="row align-items-end">
            <div className="col-md-4 mb-2">
              <GradientButton
                icon={backingUp ? 'fa-solid fa-spinner fa-spin' : 'fa-solid fa-download'}
                onClick={handleBackup}
                disabled={backingUp || !backupPathSaved}
              >
                {backingUp ? 'Realizando backup...' : 'Realizar Backup'}
              </GradientButton>
            </div>
            <div className="col-md-5 mb-2">
              <label className="form-label">Archivo para restaurar</label>
              <select
                className="form-select"
                value={selectedFile}
                onChange={(e) => setSelectedFile(e.target.value)}
                disabled={backupFiles.length === 0}
              >
                <option value="">Seleccionar archivo...</option>
                {backupFiles.map((f) => (
                  <option key={f} value={f}>{f}</option>
                ))}
              </select>
            </div>
            <div className="col-md-3 mb-2">
              <GradientButton
                icon={restoring ? 'fa-solid fa-spinner fa-spin' : 'fa-solid fa-upload'}
                onClick={handleRestore}
                disabled={restoring || !selectedFile}
              >
                {restoring ? 'Restaurando...' : 'Restaurar'}
              </GradientButton>
            </div>
          </div>

          <hr />

          <div className="row">
            <div className="col-md-12">
              <button
                className="btn btn-outline-danger"
                onClick={handleVaciarDatos}
                disabled={purging}
              >
                <Icon name={purging ? 'fa-solid fa-spinner fa-spin' : 'fa-solid fa-trash'} />{' '}
                {purging ? 'Vaciando...' : 'Vaciar Base de Datos'}
              </button>
              <small className="text-muted ms-3">
                Elimina todos los datos excepto la configuración
              </small>
            </div>
          </div>
        </div>
      </div>

      {/* AFIP Config */}
      <div className="card mb-4">
        <div className="card-header">
          <h5 className="mb-0">
            <Icon name="fa-solid fa-building-columns" /> Configuración AFIP
          </h5>
        </div>
        <div className="card-body">
          <div className="row">
            <div className="col-md-4 mb-3">
              <label className="form-label">CUIT</label>
              <input
                type="text"
                className="form-control"
                value={cuit}
                onChange={(e) => setCuit(e.target.value)}
                placeholder="20123456789"
              />
            </div>
            <div className="col-md-2 mb-3">
              <label className="form-label">Punto de Venta</label>
              <input
                type="number"
                className="form-control"
                value={puntoVenta}
                onChange={(e) => setPuntoVenta(parseInt(e.target.value, 10) || 0)}
              />
            </div>
            <div className="col-md-3 mb-3 d-flex align-items-end">
              <div className="form-check">
                <input
                  className="form-check-input"
                  type="checkbox"
                  id="isProduction"
                  checked={isProduction}
                  onChange={(e) => setIsProduction(e.target.checked)}
                />
                <label className="form-check-label" htmlFor="isProduction">
                  Producción
                </label>
              </div>
            </div>
          </div>

          <div className="row">
            <div className="col-md-6 mb-3">
              <label className="form-label">URL WSAA (Autenticación)</label>
              <input
                type="text"
                className="form-control"
                value={wsaaUrl}
                onChange={(e) => setWsaaUrl(e.target.value)}
              />
            </div>
            <div className="col-md-6 mb-3">
              <label className="form-label">URL WSFEv1 (Facturación)</label>
              <input
                type="text"
                className="form-control"
                value={wsfevUrl}
                onChange={(e) => setWsfevUrl(e.target.value)}
              />
            </div>
          </div>

          <hr />
          <h6>Certificado Digital</h6>
          <div className="row">
            <div className="col-md-6 mb-3">
              <label className="form-label">
                Archivo CRT
                {config?.tieneCrt && !crtFile && (
                  <span className="badge bg-success ms-2">Cargado</span>
                )}
              </label>
              <input
                type="file"
                className="form-control"
                accept=".crt,.pem,.cer"
                onChange={(e) => setCrtFile(e.target.files?.[0] || null)}
              />
            </div>
            <div className="col-md-6 mb-3">
              <label className="form-label">
                Archivo KEY
                {config?.tieneKey && !keyFile && (
                  <span className="badge bg-success ms-2">Cargado</span>
                )}
              </label>
              <input
                type="file"
                className="form-control"
                accept=".key,.pem"
                onChange={(e) => setKeyFile(e.target.files?.[0] || null)}
              />
            </div>
          </div>

          <div className="mt-3">
            <GradientButton
              icon={saving ? 'fa-solid fa-spinner fa-spin' : 'fa-solid fa-save'}
              onClick={handleSave}
              disabled={saving}
            >
              {saving ? 'Guardando...' : 'Guardar Configuración'}
            </GradientButton>
          </div>
        </div>
      </div>

      {/* Ultimo Comprobante */}
      <div className="card">
        <div className="card-header">
          <h5 className="mb-0">
            <Icon name="fa-solid fa-file-invoice" /> Últimos Comprobantes Autorizados
          </h5>
        </div>
        <div className="card-body">
          <div className="row align-items-end mb-3">
            <div className="col-md-2">
              <label className="form-label">Punto de Venta</label>
              <input
                type="number"
                className="form-control"
                value={puntoVenta}
                readOnly
              />
            </div>
            <div className="col-md-3">
              <GradientButton
                icon={loadingComprobantes ? 'fa-solid fa-spinner fa-spin' : 'fa-solid fa-search'}
                onClick={handleConsultarComprobantes}
                disabled={loadingComprobantes}
              >
                {loadingComprobantes ? 'Consultando...' : 'Consultar AFIP'}
              </GradientButton>
            </div>
          </div>

          {ultimosComprobantes.length > 0 && (
            <div className="table-responsive">
              <table className="custom-table">
                <thead>
                  <tr>
                    <th>Punto de Venta</th>
                    <th>Tipo de Comprobante</th>
                    <th className="text-end">Último Número</th>
                  </tr>
                </thead>
                <tbody>
                  {ultimosComprobantes.map((item) => (
                    <tr key={item.tipoComprobante}>
                      <td>{String(item.puntoVenta).padStart(5, '0')}</td>
                      <td>{item.tipoComprobanteDescripcion}</td>
                      <td className="text-end fw-bold">
                        {String(item.puntoVenta).padStart(5, '0')}-{String(item.ultimoNumero).padStart(8, '0')}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ConfiguracionPage;
