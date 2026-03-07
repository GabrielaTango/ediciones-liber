/**
 * Sidebar Component
 *
 * Main navigation sidebar with theme-consistent design.
 * Features: Dark theme, gradient brand icon, active state indicators, mobile toggle.
 */

import { Link, useLocation } from 'react-router-dom';
import { useState } from 'react';
import { Icon } from './Icon';
import shopImg from '../assets/shop.png';

interface SidebarProps {
  collapsed: boolean;
  onToggleCollapse: () => void;
}

const Sidebar = ({ collapsed, onToggleCollapse }: SidebarProps) => {
  const location = useLocation();
  const [isMobileOpen, setIsMobileOpen] = useState(false);

  const isActive = (path: string) => {
    return location.pathname === path || location.pathname.startsWith(path + '/');
  };

  const closeMobileMenu = () => {
    if (window.innerWidth <= 768) {
      setIsMobileOpen(false);
    }
  };

  return (
    <>
      {/* Mobile Menu Toggle */}
      <button
        className="menu-toggle"
        onClick={() => setIsMobileOpen(!isMobileOpen)}
        aria-label="Toggle menu"
      >
        <Icon name={isMobileOpen ? 'fa-solid fa-xmark' : 'fa-solid fa-bars'} />
      </button>

      {/* Sidebar */}
      <aside className={`sidebar ${isMobileOpen ? 'show' : ''} ${collapsed ? 'collapsed' : ''}`}>
        {/* Brand */}
        <div className="sidebar-brand">
          {collapsed
            ? <img src={shopImg} alt="EL" style={{ width: 32, height: 32 }} />
            : <h4><span>Ediciones</span><span> Liber</span></h4>
          }
        </div>

        {/* Collapse Toggle (desktop only) */}
        <button
          className="sidebar-collapse-btn"
          onClick={onToggleCollapse}
          title={collapsed ? 'Expandir menú' : 'Colapsar menú'}
        >
          <Icon name={collapsed ? 'fa-solid fa-angles-right' : 'fa-solid fa-angles-left'} />
        </button>

        {/* Main Navigation */}
        <ul className="sidebar-menu">
          <li>
            <Link
              to="/"
              className={isActive('/') && location.pathname === '/' ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-gauge" />
              <span>Principal</span>
            </Link>
          </li>
          <li>
            <Link
              to="/comprobantes"
              className={isActive('/comprobantes') ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-receipt" />
              <span>Comprobantes</span>
            </Link>
          </li>
          <li>
            <Link
              to="/remitos"
              className={isActive('/remitos') ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-file-invoice" />
              <span>Remitos</span>
            </Link>
          </li>
          <li>
            <Link
              to="/clientes"
              className={isActive('/clientes') ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-users" />
              <span>Clientes</span>
            </Link>
          </li>
          <li>
            <Link
              to="/articulos"
              className={isActive('/articulos') ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-box" />
              <span>Artículos</span>
            </Link>
          </li>
          {/* Compras Section */}
          <li style={{ marginTop: '2rem' }}>
            <div
              style={{
                padding: '0.5rem 1.5rem',
                fontSize: '0.75rem',
                fontWeight: 600,
                color: 'rgba(255, 255, 255, 0.5)',
                textTransform: 'uppercase',
                letterSpacing: '0.5px'
              }}
            >
              Compras
            </div>
          </li>
          <li>
            <Link
              to="/proveedores"
              className={isActive('/proveedores') ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-truck-field" />
              <span>Proveedores</span>
            </Link>
          </li>
          <li>
            <Link
              to="/comprobantes-proveedores"
              className={isActive('/comprobantes-proveedores') ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-file-contract" />
              <span>Comp. Proveedores</span>
            </Link>
          </li>
          <li>
            <Link
              to="/cuotas-proveedores"
              className={isActive('/cuotas-proveedores') ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-money-check-dollar" />
              <span>Cuotas Proveed.</span>
            </Link>
          </li>
          <li>
            <Link
              to="/gastos"
              className={isActive('/gastos') ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-money-bill-trend-up" />
              <span>Gastos</span>
            </Link>
          </li>

          {/* Listados Section */}
          <li style={{ marginTop: '2rem' }}>
            <div
              style={{
                padding: '0.5rem 1.5rem',
                fontSize: '0.75rem',
                fontWeight: 600,
                color: 'rgba(255, 255, 255, 0.5)',
                textTransform: 'uppercase',
                letterSpacing: '0.5px'
              }}
            >
              Listados
            </div>
          </li>
          <li>
            <Link
              to="/iva-ventas"
              className={isActive('/iva-ventas') ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-file-lines" />
              <span>IVA Ventas</span>
            </Link>
          </li>
          <li>
            <Link
              to="/iva-compras"
              className={isActive('/iva-compras') ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-file-lines" />
              <span>IVA Compras</span>
            </Link>
          </li>
          <li>
            <Link
              to="/deudores"
              className={isActive('/deudores') ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-file-invoice-dollar" />
              <span>Deudores</span>
            </Link>
          </li>
          <li>
            <Link
              to="/cuotas"
              className={isActive('/cuotas') ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-money-check-dollar" />
              <span>Cuotas</span>
            </Link>
          </li>
          <li>
            <Link
              to="/articulos-vendidos-zona"
              className={isActive('/articulos-vendidos-zona') ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-chart-bar" />
              <span>Art. x Zona</span>
            </Link>
          </li>
          {/* References Section */}
          <li style={{ marginTop: '2rem' }}>
            <div
              style={{
                padding: '0.5rem 1.5rem',
                fontSize: '0.75rem',
                fontWeight: 600,
                color: 'rgba(255, 255, 255, 0.5)',
                textTransform: 'uppercase',
                letterSpacing: '0.5px'
              }}
            >
              Referencias
            </div>
          </li>
          <li>
            <Link
              to="/vendedores"
              className={isActive('/vendedores') ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-user-tie" />
              <span>Vendedores</span>
            </Link>
          </li>
          <li>
            <Link
              to="/transportes"
              className={isActive('/transportes') ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-truck" />
              <span>Transportes</span>
            </Link>
          </li>
          <li>
            <Link
              to="/zonas"
              className={isActive('/zonas') ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-location-dot" />
              <span>Zonas</span>
            </Link>
          </li>
          <li>
            <Link
              to="/subzonas"
              className={isActive('/subzonas') ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-map-location-dot" />
              <span>SubZonas</span>
            </Link>
          </li>
          <li>
            <Link
              to="/provincias"
              className={isActive('/provincias') ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-map" />
              <span>Provincias</span>
            </Link>
          </li>
          <li>
            <Link
              to="/categorias-gasto"
              className={isActive('/categorias-gasto') ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-tags" />
              <span>Categorías Gasto</span>
            </Link>
          </li>

          {/* Configuración */}
          <li style={{ marginTop: '2rem' }}>
            <Link
              to="/configuracion"
              className={isActive('/configuracion') ? 'active' : ''}
              onClick={closeMobileMenu}
            >
              <Icon name="fa-solid fa-gear" />
              <span>Configuración</span>
            </Link>
          </li>
        </ul>

        {/* User Section (Optional - commented out for now) */}
        {/*
        <div style={{ marginTop: 'auto', padding: '1.5rem', borderTop: '1px solid rgba(255, 255, 255, 0.1)' }}>
          <div className="d-flex align-items-center text-white">
            <div style={{
              width: '40px',
              height: '40px',
              borderRadius: '10px',
              background: 'var(--primary-gradient)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              marginRight: '0.75rem',
              fontWeight: 600
            }}>
              U
            </div>
            <div>
              <div style={{ fontWeight: 600, fontSize: '0.9rem' }}>Usuario</div>
              <div style={{ fontSize: '0.75rem', color: 'rgba(255, 255, 255, 0.6)' }}>Administrador</div>
            </div>
          </div>
        </div>
        */}
      </aside>
    </>
  );
};

export default Sidebar;
