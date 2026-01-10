-- Migración: Agregar campo para relacionar NC con Factura
-- Fecha: 2024

-- Agregar campo comprobante_asociado_id para relacionar Notas de Crédito con Facturas
ALTER TABLE comprobantes
ADD COLUMN comprobante_asociado_id INT NULL,
ADD CONSTRAINT fk_comprobante_asociado
    FOREIGN KEY (comprobante_asociado_id)
    REFERENCES comprobantes(id)
    ON DELETE SET NULL;

-- Índice para mejorar búsquedas
CREATE INDEX idx_comprobante_asociado ON comprobantes(comprobante_asociado_id);
