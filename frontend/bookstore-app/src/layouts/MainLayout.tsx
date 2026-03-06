/**
 * MainLayout Component
 *
 * Main application layout with sidebar and content area.
 * Uses theme-consistent spacing and background colors.
 */

import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import Sidebar from '../components/Sidebar';

const MainLayout = () => {
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);

  return (
    <>
      <Sidebar collapsed={sidebarCollapsed} onToggleCollapse={() => setSidebarCollapsed(!sidebarCollapsed)} />
      <main className={`main-content ${sidebarCollapsed ? 'sidebar-collapsed' : ''}`}>
        <Outlet />
      </main>
    </>
  );
};

export default MainLayout;
