/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

CREATE TABLE IF NOT EXISTS `articulos` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Codigo` varchar(25) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Descripcion` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `CodBarras` varchar(13) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Observaciones` varchar(2000) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Tomos` int DEFAULT NULL,
  `Tema` varchar(50) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Precio` decimal(20,2) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `clientes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Codigo` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Nombre` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Zona_Id` int DEFAULT NULL,
  `SubZona_Id` int DEFAULT NULL,
  `Vendedor_Id` int DEFAULT NULL,
  `DomicilioComercial` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `DomicilioParticular` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Provincia_Id` int DEFAULT NULL,
  `CodigoPostal` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `FechaAlta` date NOT NULL,
  `FechaInha` date DEFAULT NULL,
  `SoloContado` tinyint(1) DEFAULT '0',
  `Telefono` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `TelefonoMovil` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `EMail` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Contacto` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `TipoDocumento` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `NroDocumento` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `NroIIBB` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CategoriaIva` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CondicionPago` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Descuento` decimal(10,2) DEFAULT '0.00',
  `Observaciones` text COLLATE utf8mb4_unicode_ci,
  `TipoDocArca` varchar(2) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `comprobantes` (
  `id` int NOT NULL AUTO_INCREMENT,
  `cliente_id` int NOT NULL,
  `fecha` datetime DEFAULT CURRENT_TIMESTAMP,
  `tipoComprobante` varchar(3) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `numeroComprobante` varchar(14) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `total` decimal(12,2) NOT NULL,
  `CAE` varchar(50) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `VTO` date DEFAULT NULL,
  `Bonificacion` decimal(17,2) DEFAULT NULL,
  `PorcentajeBonif` decimal(17,0) DEFAULT NULL,
  `Anticipo` decimal(17,2) DEFAULT NULL,
  `ContraEntrega` decimal(17,2) DEFAULT 0,
  `Cuotas` int DEFAULT NULL,
  `ValorCuota` decimal(17,2) DEFAULT NULL,
  `vendedor_id` int DEFAULT NULL,
  `ContraEntregaPagado` decimal(17,2) DEFAULT 0,
  `GastosEnvio` decimal(17,2) DEFAULT NULL,
  `EsElectronica` tinyint(1) NOT NULL DEFAULT 1,
  `EsPresupuesto` tinyint(1) NOT NULL DEFAULT 0,
  `comprobante_asociado_id` int DEFAULT NULL,
  `estado` varchar(20) COLLATE utf8mb4_general_ci DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `cliente_id` (`cliente_id`),
  KEY `FK_comprobantes_vendedores` (`vendedor_id`),
  KEY `idx_comprobantes_tipo` (`EsElectronica`, `EsPresupuesto`),
  KEY `idx_comprobante_asociado` (`comprobante_asociado_id`),
  CONSTRAINT `comprobantes_ibfk_1` FOREIGN KEY (`cliente_id`) REFERENCES `clientes` (`Id`),
  CONSTRAINT `FK_comprobantes_vendedores` FOREIGN KEY (`vendedor_id`) REFERENCES `vendedores` (`id`),
  CONSTRAINT `fk_comprobante_asociado` FOREIGN KEY (`comprobante_asociado_id`) REFERENCES `comprobantes` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=34 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `comprobante_detalle` (
  `id` int NOT NULL AUTO_INCREMENT,
  `factura_id` int NOT NULL,
  `articulo_id` int NOT NULL,
  `cantidad` int NOT NULL,
  `precio_unitario` decimal(10,2) NOT NULL,
  `subtotal` decimal(12,2) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `factura_id` (`factura_id`),
  KEY `producto_id` (`articulo_id`) USING BTREE,
  CONSTRAINT `comprobante_detalle_ibfk_1` FOREIGN KEY (`factura_id`) REFERENCES `comprobantes` (`id`),
  CONSTRAINT `comprobante_detalle_ibfk_2` FOREIGN KEY (`articulo_id`) REFERENCES `articulos` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=31 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `condicionVenta` (
  `id` int NOT NULL AUTO_INCREMENT,
  `codigo` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `descripcion` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

CREATE TABLE IF NOT EXISTS `cuotas` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Comprobante_Id` int DEFAULT NULL,
  `numero_cuota` int DEFAULT 1,
  `Fecha` datetime DEFAULT NULL,
  `Importe` decimal(17,2) DEFAULT NULL,
  `importe_pagado` decimal(17,2) DEFAULT 0,
  `Estado` varchar(3) COLLATE utf8mb4_general_ci DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK__comprobantes` (`Comprobante_Id`) USING BTREE,
  KEY `idx_cuotas_numero` (`Comprobante_Id`, `numero_cuota`),
  CONSTRAINT `FK__comprobantes` FOREIGN KEY (`Comprobante_Id`) REFERENCES `comprobantes` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `customers` (
  `id` int NOT NULL AUTO_INCREMENT,
  `full_name` varchar(100) COLLATE utf8mb4_general_ci NOT NULL,
  `document_number` varchar(20) COLLATE utf8mb4_general_ci NOT NULL,
  `street_address` varchar(150) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `state` varchar(50) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `postal_code` varchar(10) COLLATE utf8mb4_general_ci DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `document_number` (`document_number`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `categorias_gasto` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) COLLATE utf8mb4_general_ci NOT NULL,
  `Descripcion` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Activo` tinyint(1) NOT NULL DEFAULT 1,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_nombre` (`Nombre`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `gastos` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `NroComprobante` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  `Categoria` varchar(100) COLLATE utf8mb4_general_ci NOT NULL,
  `Descripcion` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `Fecha` date NOT NULL,
  `Importe` decimal(10,2) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `listas` (
  `id` int NOT NULL AUTO_INCREMENT,
  `codigo` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `descripcion` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

CREATE TABLE IF NOT EXISTS `precios` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Articulo_Id` int DEFAULT NULL,
  `Precio` decimal(17,2) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_precios_articulos` (`Articulo_Id`),
  CONSTRAINT `FK_precios_articulos` FOREIGN KEY (`Articulo_Id`) REFERENCES `articulos` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `provincias` (
  `id` int NOT NULL AUTO_INCREMENT,
  `codigo` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `descripcion` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

CREATE TABLE IF NOT EXISTS `subzonas` (
  `id` int NOT NULL AUTO_INCREMENT,
  `codigo` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `descripcion` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `zona_id` int DEFAULT NULL,
  `provincia_id` int NOT NULL,
  `codigo_postal` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `localidad` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  KEY `FK_subzonas_provincias` (`provincia_id`),
  KEY `FK_subzonas_zonas` (`zona_id`),
  CONSTRAINT `FK_subzonas_provincias` FOREIGN KEY (`provincia_id`) REFERENCES `provincias` (`id`),
  CONSTRAINT `FK_subzonas_zonas` FOREIGN KEY (`zona_id`) REFERENCES `zonas` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

CREATE TABLE IF NOT EXISTS `transportes` (
  `id` int NOT NULL AUTO_INCREMENT,
  `codigo` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `nombre` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `direccion` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `localidad` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `provincia_id` int DEFAULT NULL,
  `cuit` varchar(13) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_transportes_provincias` (`provincia_id`),
  CONSTRAINT `FK_transportes_provincias` FOREIGN KEY (`provincia_id`) REFERENCES `provincias` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

CREATE TABLE IF NOT EXISTS `tipodocumento` (
  `id` int NOT NULL AUTO_INCREMENT,
  `codigo` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `descripcion` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

CREATE TABLE IF NOT EXISTS `vendedores` (
  `id` int NOT NULL AUTO_INCREMENT,
  `codigo` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `descripcion` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

CREATE TABLE IF NOT EXISTS `zonas` (
  `id` int NOT NULL AUTO_INCREMENT,
  `codigo` varchar(20) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `descripcion` varchar(50) COLLATE utf8mb4_general_ci DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `remitos` (
  `id` int NOT NULL AUTO_INCREMENT,
  `numero` varchar(20) COLLATE utf8mb4_general_ci NOT NULL,
  `fecha` datetime DEFAULT CURRENT_TIMESTAMP,
  `cliente_id` int NOT NULL,
  `transporte_id` int NOT NULL,
  `cantidad_bultos` int NOT NULL,
  `valor_declarado` decimal(12,2) NOT NULL,
  `observaciones` text COLLATE utf8mb4_general_ci,
  PRIMARY KEY (`id`),
  KEY `FK_remitos_clientes` (`cliente_id`),
  KEY `FK_remitos_transportes` (`transporte_id`),
  CONSTRAINT `FK_remitos_clientes` FOREIGN KEY (`cliente_id`) REFERENCES `clientes` (`Id`),
  CONSTRAINT `FK_remitos_transportes` FOREIGN KEY (`transporte_id`) REFERENCES `transportes` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `proveedores` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Codigo` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Nombre` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL,
  `RazonSocial` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Cuit` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Domicilio` varchar(300) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Telefono` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Mail` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `FechaInhabilitacion` date DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `comprobantes_proveedores` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Proveedor_Id` int NOT NULL,
  `TipoComprobante` varchar(3) COLLATE utf8mb4_general_ci NOT NULL,
  `FechaEmision` date NOT NULL,
  `NroComprobante` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  `ImporteTotal` decimal(17,2) NOT NULL,
  `CantidadCuotas` int NOT NULL,
  `FechaPrimerVencimiento` date NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_comp_prov_proveedores` (`Proveedor_Id`),
  CONSTRAINT `FK_comp_prov_proveedores` FOREIGN KEY (`Proveedor_Id`) REFERENCES `proveedores` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `cuotas_proveedores` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ComprobanteProveedor_Id` int NOT NULL,
  `NumeroCuota` int NOT NULL,
  `FechaVencimiento` date NOT NULL,
  `Importe` decimal(17,2) NOT NULL,
  `ImportePagado` decimal(17,2) NOT NULL DEFAULT 0,
  `Estado` varchar(3) COLLATE utf8mb4_general_ci NOT NULL DEFAULT 'PEN',
  PRIMARY KEY (`Id`),
  KEY `FK_cuotas_prov_comp` (`ComprobanteProveedor_Id`),
  CONSTRAINT `FK_cuotas_prov_comp` FOREIGN KEY (`ComprobanteProveedor_Id`) REFERENCES `comprobantes_proveedores` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `pagos_cuotas` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Cuota_Id` int NOT NULL,
  `NroReferencia` varchar(100) COLLATE utf8mb4_general_ci NOT NULL,
  `Fecha` date NOT NULL,
  `Importe` decimal(17,2) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_pagos_cuotas_cuota` (`Cuota_Id`),
  CONSTRAINT `FK_pagos_cuotas_cuota` FOREIGN KEY (`Cuota_Id`) REFERENCES `cuotas` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `pagos_cuotas_proveedores` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CuotaProveedor_Id` int NOT NULL,
  `NroReferencia` varchar(100) COLLATE utf8mb4_general_ci NOT NULL,
  `Fecha` date NOT NULL,
  `Importe` decimal(17,2) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_pagos_cuotas_prov` (`CuotaProveedor_Id`),
  CONSTRAINT `FK_pagos_cuotas_prov` FOREIGN KEY (`CuotaProveedor_Id`) REFERENCES `cuotas_proveedores` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `configuracion` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Clave` varchar(100) COLLATE utf8mb4_general_ci NOT NULL,
  `Valor` text COLLATE utf8mb4_general_ci DEFAULT NULL,
  `ValorBinario` longblob DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UK_configuracion_clave` (`Clave`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `usuarios` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Username` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  `PasswordHash` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `NombreCompleto` varchar(100) COLLATE utf8mb4_general_ci NOT NULL,
  `Email` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Rol` varchar(20) COLLATE utf8mb4_general_ci NOT NULL DEFAULT 'usuario',
  `Activo` tinyint(1) NOT NULL DEFAULT 1,
  `FechaCreacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UltimoAcceso` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UK_usuarios_username` (`Username`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
