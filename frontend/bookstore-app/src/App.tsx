import { Routes, Route } from 'react-router-dom';
import MainLayout from './layouts/MainLayout';
import Dashboard from './pages/Dashboard';
import ClientesPage from './pages/ClientesPage';
import ClienteFormPage from './pages/ClienteFormPage';
import ArticulosPage from './pages/ArticulosPage';
import ArticuloFormPage from './pages/ArticuloFormPage';
import ZonasPage from './pages/ZonasPage';
import SubZonasPage from './pages/SubZonasPage';
import ProvinciasPage from './pages/ProvinciasPage';
import VendedoresPage from './pages/VendedoresPage';
import TransportesPage from './pages/TransportesPage';
import ComprobantesPage from './pages/ComprobantesPage';
import ComprobanteFormPage from './pages/ComprobanteFormPage';
import RemitosPage from './pages/RemitosPage';
import RemitoFormPage from './pages/RemitoFormPage';
import IvaVentasPage from './pages/IvaVentasPage';
import DeudoresPage from './pages/DeudoresPage';
import CuotasPage from './pages/CuotasPage';
import GastosPage from './pages/GastosPage';
import GastoFormPage from './pages/GastoFormPage';
import CategoriasGastoPage from './pages/CategoriasGastoPage';
import ArticulosVendidosZonaPage from './pages/ArticulosVendidosZonaPage';

function App() {
  return (
    <Routes>
      <Route path="/" element={<MainLayout />}>
        <Route index element={<Dashboard />} />
        <Route path="clientes" element={<ClientesPage />} />
        <Route path="clientes/nuevo" element={<ClienteFormPage />} />
        <Route path="clientes/editar/:id" element={<ClienteFormPage />} />
        <Route path="articulos" element={<ArticulosPage />} />
        <Route path="articulos/nuevo" element={<ArticuloFormPage />} />
        <Route path="articulos/editar/:id" element={<ArticuloFormPage />} />
        <Route path="comprobantes" element={<ComprobantesPage />} />
        <Route path="comprobantes/nuevo" element={<ComprobanteFormPage />} />
        <Route path="comprobantes/ver/:id" element={<ComprobanteFormPage />} />
        <Route path="remitos" element={<RemitosPage />} />
        <Route path="remitos/nuevo" element={<RemitoFormPage />} />
        <Route path="remitos/editar/:id" element={<RemitoFormPage />} />
        <Route path="iva-ventas" element={<IvaVentasPage />} />
        <Route path="deudores" element={<DeudoresPage />} />
        <Route path="cuotas" element={<CuotasPage />} />
        <Route path="articulos-vendidos-zona" element={<ArticulosVendidosZonaPage />} />
        <Route path="gastos" element={<GastosPage />} />
        <Route path="gastos/nuevo" element={<GastoFormPage />} />
        <Route path="gastos/editar/:id" element={<GastoFormPage />} />
        <Route path="categorias-gasto" element={<CategoriasGastoPage />} />
        <Route path="zonas" element={<ZonasPage />} />
        <Route path="subzonas" element={<SubZonasPage />} />
        <Route path="provincias" element={<ProvinciasPage />} />
        <Route path="vendedores" element={<VendedoresPage />} />
        <Route path="transportes" element={<TransportesPage />} />
      </Route>
    </Routes>
  );
}

export default App;
