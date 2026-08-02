/**
 * CertificadoEstadoPanel Component
 *
 * Panel semáforo con la vigencia del certificado AFIP.
 * Verde: falta más de 2 meses. Amarillo: vence pronto. Rojo: vencido.
 */

import type React from 'react';
import type { CertificadoEstadoDto } from '../types/configuracion';
import { Icon } from './Icon';

interface CertificadoEstadoPanelProps {
  estado: CertificadoEstadoDto | null;
  loading?: boolean;
}

const ICONOS: Record<string, string> = {
  vigente: 'fa-solid fa-shield-halved',
  por_vencer: 'fa-solid fa-triangle-exclamation',
  vencido: 'fa-solid fa-circle-xmark',
  sin_certificado: 'fa-solid fa-file-circle-question',
  error: 'fa-solid fa-circle-exclamation'
};

const TITULOS: Record<string, string> = {
  vigente: 'Certificado AFIP vigente',
  por_vencer: 'El certificado AFIP vence pronto',
  vencido: 'Certificado AFIP VENCIDO',
  sin_certificado: 'Sin certificado AFIP cargado',
  error: 'No se pudo leer el certificado AFIP'
};

const formatearFecha = (fecha: string) =>
  new Date(fecha).toLocaleDateString('es-AR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  });

const plural = (cantidad: number, singular: string, plural: string) =>
  cantidad === 1 ? singular : plural;

export const CertificadoEstadoPanel: React.FC<CertificadoEstadoPanelProps> = ({ estado, loading = false }) => {
  // Mientras carga no se muestra nada, para no mostrar un color equivocado
  if (loading || !estado) return null;

  const { estado: situacion, fechaVencimiento, diasRestantes, mensaje } = estado;

  const getDetalle = () => {
    if (!fechaVencimiento || diasRestantes === null) {
      return mensaje ?? 'Cargá el certificado en Configuración para poder facturar.';
    }

    const fecha = formatearFecha(fechaVencimiento);

    if (diasRestantes < 0) {
      const dias = Math.abs(diasRestantes);
      return `Venció el ${fecha} (hace ${dias} ${plural(dias, 'día', 'días')}).`;
    }
    if (diasRestantes === 0) {
      return `Vence hoy, ${fecha}.`;
    }
    return `Vence el ${fecha} (en ${diasRestantes} ${plural(diasRestantes, 'día', 'días')}).`;
  };

  const getBadge = () => {
    if (diasRestantes === null) return 'Sin datos';
    if (diasRestantes < 0) return 'Vencido';
    if (diasRestantes === 0) return 'Vence hoy';
    return `${diasRestantes} ${plural(diasRestantes, 'día', 'días')}`;
  };

  return (
    <div className={`cert-status-card ${situacion}`}>
      <div className="cert-status-icon">
        <Icon name={ICONOS[situacion] ?? ICONOS.error} />
      </div>
      <div className="cert-status-body">
        <h6>{TITULOS[situacion] ?? TITULOS.error}</h6>
        <p>{getDetalle()}</p>
      </div>
      <span className="cert-status-badge">{getBadge()}</span>
    </div>
  );
};

export default CertificadoEstadoPanel;
