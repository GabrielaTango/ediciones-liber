/**
 * Dashboard Page
 *
 * Main dashboard with stats, recent activity, and quick access links.
 * Uses theme-consistent components and styling.
 */

import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import Swal from 'sweetalert2';
import { dashboardService } from '../services/dashboardService';
import type { DashboardStats, ActividadReciente } from '../types/dashboard';
import { PageHeader } from '../components/PageHeader';
import { StatCard } from '../components/StatCard';
import { GradientCard } from '../components/GradientCard';
import { Icon } from '../components/Icon';

const Dashboard = () => {
  const navigate = useNavigate();
  const [stats, setStats] = useState<DashboardStats>({
    totalComprobantes: 0,
    totalClientes: 0,
    totalArticulos: 0,
    ventasHoy: 0
  });
  const [actividades, setActividades] = useState<ActividadReciente[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      setLoading(true);
      const [statsData, actividadesData] = await Promise.all([
        dashboardService.getStats(),
        dashboardService.getActividadesRecientes(5)
      ]);
      setStats(statsData);
      setActividades(actividadesData);
    } catch (error) {
      console.error('Error loading dashboard data:', error);
      Swal.fire({
        title: 'Error',
        text: 'No se pudieron cargar los datos del dashboard',
        icon: 'error',
        confirmButtonText: 'Entendido'
      });
    } finally {
      setLoading(false);
    }
  };

  const getTimeAgo = (fecha: string) => {
    const now = new Date();
    const actividadDate = new Date(fecha);
    const diffInMs = now.getTime() - actividadDate.getTime();
    const diffInMinutes = Math.floor(diffInMs / 1000 / 60);
    const diffInHours = Math.floor(diffInMinutes / 60);
    const diffInDays = Math.floor(diffInHours / 24);

    if (diffInMinutes < 1) return 'Hace un momento';
    if (diffInMinutes < 60) return `Hace ${diffInMinutes} minuto${diffInMinutes > 1 ? 's' : ''}`;
    if (diffInHours < 24) return `Hace ${diffInHours} hora${diffInHours > 1 ? 's' : ''}`;
    return `Hace ${diffInDays} día${diffInDays > 1 ? 's' : ''}`;
  };

  const getActivityVariant = (color: string): 'primary' | 'success' | 'danger' | 'warning' => {
    if (color === 'success') return 'success';
    if (color === 'danger') return 'danger';
    if (color === 'warning') return 'warning';
    return 'primary';
  };

  return (
    <div>
      <PageHeader
        title="Bienvenido"

        subtitle="Bienvenido de nuevo, aquí está tu resumen del día"
      />

      {/* Stats Cards */}
      <div className="row g-4 mb-4">
        <div className="col-md-3">
          <StatCard
            title="Comprobantes"
            value={stats.totalComprobantes}
            icon="fa-solid fa-receipt"
            variant="primary"
            onClick={() => navigate('/comprobantes')}
          />
        </div>
        <div className="col-md-3">
          <StatCard
            title="Clientes"
            value={stats.totalClientes}
            icon="fa-solid fa-users"
            variant="warning"
            onClick={() => navigate('/clientes')}
          />
        </div>
        <div className="col-md-3">
          <StatCard
            title="Artículos"
            value={stats.totalArticulos}
            icon="fa-solid fa-box"
            variant="success"
            onClick={() => navigate('/articulos')}
          />
        </div>
        <div className="col-md-3">
          <StatCard
            title="Ventas Hoy"
            value={stats.ventasHoy}
            icon="fa-solid fa-chart-line"
            variant="danger"
            onClick={() => navigate('/comprobantes')}
          />
        </div>
      </div>

      {/* Quick Access */}
      <div className="row g-4 mb-4">
        <div className="col-12">
          <GradientCard title="Accesos Rápidos" icon="fa-solid fa-bolt">
            <div className="row g-3">
              <div className="col-md-3">
                <Link to="/comprobantes/nuevo" className="text-decoration-none">
                  <div
                    className="card text-white text-center p-4"
                    style={{
                      background: 'var(--primary-gradient)',
                      cursor: 'pointer',
                      transition: 'var(--transition-base)'
                    }}
                    onMouseEnter={(e) => {
                      e.currentTarget.style.transform = 'translateY(-5px)';
                      e.currentTarget.style.boxShadow = 'var(--shadow-lg)';
                    }}
                    onMouseLeave={(e) => {
                      e.currentTarget.style.transform = 'translateY(0)';
                      e.currentTarget.style.boxShadow = '';
                    }}
                  >
                    <div style={{
                      width: '60px',
                      height: '60px',
                      background: 'rgba(255, 255, 255, 0.2)',
                      borderRadius: '50%',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      margin: '0 auto 1rem'
                    }}>
                      <Icon name="fa-solid fa-circle-plus" style={{ fontSize: '2rem' }} />
                    </div>
                    <h6 className="mb-0 fw-bold">Nuevo Comprobante</h6>
                  </div>
                </Link>
              </div>
              <div className="col-md-3">
                <Link to="/clientes/nuevo" className="text-decoration-none">
                  <div
                    className="card text-white text-center p-4"
                    style={{
                      background: 'var(--warning-gradient)',
                      cursor: 'pointer',
                      transition: 'var(--transition-base)'
                    }}
                    onMouseEnter={(e) => {
                      e.currentTarget.style.transform = 'translateY(-5px)';
                      e.currentTarget.style.boxShadow = 'var(--shadow-lg)';
                    }}
                    onMouseLeave={(e) => {
                      e.currentTarget.style.transform = 'translateY(0)';
                      e.currentTarget.style.boxShadow = '';
                    }}
                  >
                    <div style={{
                      width: '60px',
                      height: '60px',
                      background: 'rgba(255, 255, 255, 0.2)',
                      borderRadius: '50%',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      margin: '0 auto 1rem'
                    }}>
                      <Icon name="fa-solid fa-user-plus" style={{ fontSize: '2rem' }} />
                    </div>
                    <h6 className="mb-0 fw-bold">Nuevo Cliente</h6>
                  </div>
                </Link>
              </div>
              <div className="col-md-3">
                <Link to="/articulos/nuevo" className="text-decoration-none">
                  <div
                    className="card text-white text-center p-4"
                    style={{
                      background: 'var(--success-gradient)',
                      cursor: 'pointer',
                      transition: 'var(--transition-base)'
                    }}
                    onMouseEnter={(e) => {
                      e.currentTarget.style.transform = 'translateY(-5px)';
                      e.currentTarget.style.boxShadow = 'var(--shadow-lg)';
                    }}
                    onMouseLeave={(e) => {
                      e.currentTarget.style.transform = 'translateY(0)';
                      e.currentTarget.style.boxShadow = '';
                    }}
                  >
                    <div style={{
                      width: '60px',
                      height: '60px',
                      background: 'rgba(255, 255, 255, 0.2)',
                      borderRadius: '50%',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      margin: '0 auto 1rem'
                    }}>
                    
                      <Icon name="fa-solid fa-box-open" style={{ fontSize: '2rem' }} />
                    </div>
                    <h6 className="mb-0 fw-bold">Nuevo Artículo</h6>
                  </div>
                </Link>
              </div>
              <div className="col-md-3">
                <Link to="/comprobantes" className="text-decoration-none">
                  <div
                    className="card text-white text-center p-4"
                    style={{
                      background: 'var(--info-gradient)',
                      
                      cursor: 'pointer',
                      transition: 'var(--transition-base)'
                    }}
                    onMouseEnter={(e) => {
                      e.currentTarget.style.transform = 'translateY(-5px)';
                      e.currentTarget.style.boxShadow = 'var(--shadow-lg)';
                    }}
                    onMouseLeave={(e) => {
                      e.currentTarget.style.transform = 'translateY(0)';
                      e.currentTarget.style.boxShadow = '';
                    }}
                  >
                    <div style={{
                      width: '60px',
                      height: '60px',
                      background: 'rgba(255, 255, 255, 0.2)',
                      borderRadius: '50%',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      margin: '0 auto 1rem'
                    }}>
                      <Icon name="fa-solid fa-list" style={{ fontSize: '2rem' }} />
                    </div>
                    <h6 className="mb-0 fw-bold">Ver Comprobantes</h6>
                  </div>
                </Link>
              </div>
            </div>
          </GradientCard>
        </div>
      </div>

      {/* Recent Activity & System Status */}
      <div className="row g-4">
        <div className="col-md-6">
          <GradientCard title="Actividad Reciente" icon="fa-solid fa-clock-rotate-left">
            {loading ? (
              <div className="text-center py-4">
                <div className="spinner-gradient" />
              </div>
            ) : actividades.length === 0 ? (
              <div className="alert-info">
                <Icon name="fa-solid fa-circle-info" />
                <p>No hay actividades recientes</p>
              </div>
            ) : (
              <ul className="activity-list">
                {actividades.map((actividad, index) => (
                  <li key={index} className="activity-item">
                    <div className={`activity-icon ${getActivityVariant(actividad.color)}`}>
                      <Icon name={actividad.icono} />
                    </div>
                    <div className="activity-content">
                      <h6>{actividad.descripcion}</h6>
                      <p>{getTimeAgo(actividad.fecha)}</p>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </GradientCard>
        </div>

        <div className="col-md-6">
          <GradientCard title="Estado del Sistema" icon="fa-solid fa-server">
            <div className="mb-4">
              <div className="d-flex justify-content-between align-items-center mb-2">
                <div className="d-flex align-items-center">
                  <div className="activity-icon success me-3">
                    <Icon name="fa-solid fa-server" />
                  </div>
                  <span className="fw-600">Conexión API</span>
                </div>
                <span className="badge badge-success">Activa</span>
              </div>
              <div className="progress" style={{ height: '8px', borderRadius: '10px' }}>
                <div
                  className="progress-bar gradient-success"
                  style={{ width: '100%', borderRadius: '10px' }}
                />
              </div>
            </div>
            <div className="mb-4">
              <div className="d-flex justify-content-between align-items-center mb-2">
                <div className="d-flex align-items-center">
                  <div className="activity-icon success me-3">
                    <Icon name="fa-solid fa-shield-halved" />
                  </div>
                  <span className="fw-600">AFIP/ARCA</span>
                </div>
                <span className="badge badge-success">Conectado</span>
              </div>
              <div className="progress" style={{ height: '8px', borderRadius: '10px' }}>
                <div
                  className="progress-bar gradient-success"
                  style={{ width: '100%', borderRadius: '10px' }}
                />
              </div>
            </div>
            <div>
              <div className="d-flex justify-content-between align-items-center mb-2">
                <div className="d-flex align-items-center">
                  <div className="activity-icon success me-3">
                    <Icon name="fa-solid fa-database" />
                  </div>
                  <span className="fw-600">Base de Datos</span>
                </div>
                <span className="badge badge-success">Operativa</span>
              </div>
              <div className="progress" style={{ height: '8px', borderRadius: '10px' }}>
                <div
                  className="progress-bar gradient-success"
                  style={{ width: '100%', borderRadius: '10px' }}
                />
              </div>
            </div>
          </GradientCard>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
