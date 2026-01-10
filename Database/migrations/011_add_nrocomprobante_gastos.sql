-- Crear tabla categorias_gasto si no existe
CREATE TABLE IF NOT EXISTS categorias_gasto (
  Id INT NOT NULL AUTO_INCREMENT,
  Nombre VARCHAR(100) NOT NULL,
  Descripcion VARCHAR(255) NULL,
  Activo TINYINT(1) NOT NULL DEFAULT 1,
  PRIMARY KEY (Id),
  UNIQUE KEY uk_nombre (Nombre)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Insertar algunas categorías por defecto
INSERT IGNORE INTO categorias_gasto (Nombre, Descripcion, Activo) VALUES
('Servicios', 'Servicios públicos y privados', 1),
('Alquiler', 'Alquiler de local o depósito', 1),
('Impuestos', 'Impuestos y tasas', 1),
('Sueldos', 'Sueldos y cargas sociales', 1),
('Insumos', 'Insumos de oficina y papelería', 1),
('Transporte', 'Gastos de envío y transporte', 1),
('Mantenimiento', 'Mantenimiento y reparaciones', 1),
('Otros', 'Otros gastos', 1);

-- Agregar columna NroComprobante a la tabla gastos si no existe
ALTER TABLE gastos
ADD COLUMN IF NOT EXISTS NroComprobante VARCHAR(50) NULL AFTER Id;

-- Actualizar registros existentes con valor por defecto
UPDATE gastos SET NroComprobante = CONCAT('GASTO-', Id) WHERE NroComprobante IS NULL;
