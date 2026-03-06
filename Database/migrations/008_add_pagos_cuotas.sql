-- Tabla de pagos individuales para cuotas de clientes
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

-- Migrar datos existentes: crear un pago por cada cuota que tenga ImportePagado > 0
INSERT INTO pagos_cuotas (Cuota_Id, NroReferencia, Fecha, Importe)
SELECT Id, 'Migrado', COALESCE(Fecha, CURDATE()), ImportePagado
FROM cuotas
WHERE ImportePagado > 0;
