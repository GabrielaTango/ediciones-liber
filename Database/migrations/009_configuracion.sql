CREATE TABLE IF NOT EXISTS `configuracion` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Clave` varchar(100) COLLATE utf8mb4_general_ci NOT NULL,
  `Valor` text COLLATE utf8mb4_general_ci,
  `ValorBinario` longblob,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UK_configuracion_clave` (`Clave`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Migrar valores por defecto
INSERT IGNORE INTO configuracion (Clave, Valor) VALUES
('Afip_CUIT', '20290077207'),
('Afip_WsaaUrl', 'https://wsaahomo.afip.gov.ar/ws/services/LoginCms'),
('Afip_WsfevUrl', 'https://wswhomo.afip.gov.ar/wsfev1/service.asmx'),
('Afip_PuntoVenta', '13'),
('Afip_IsProduction', 'false');
