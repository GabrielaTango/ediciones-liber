-- Migration: Add NumeroCuota to cuotas table
-- Date: 2026-01-03
-- Description: Adds NumeroCuota field to identify installment number (0 = contraentrega, 1+ = regular installments)
--              Migrates ContraEntrega data from comprobantes to cuotas as cuota 0

-- Step 1: Add NumeroCuota column to cuotas table
ALTER TABLE cuotas
ADD COLUMN numero_cuota INT DEFAULT 1;

-- Step 2: Update existing cuotas to have sequential numbers based on fecha
-- This uses a variable to assign sequential numbers per comprobante
SET @row_number = 0;
SET @prev_comprobante = 0;

UPDATE cuotas c
INNER JOIN (
    SELECT
        Id,
        @row_number := IF(@prev_comprobante = Comprobante_Id, @row_number + 1, 1) AS rn,
        @prev_comprobante := Comprobante_Id
    FROM cuotas
    ORDER BY Comprobante_Id, Fecha
) AS numbered ON c.Id = numbered.Id
SET c.numero_cuota = numbered.rn;

-- Step 3: Insert cuota 0 (contraentrega) for comprobantes that have ContraEntrega > 0
INSERT INTO cuotas (comprobante_id, fecha, importe, importe_pagado, estado, numero_cuota)
SELECT
    c.id,
    c.fecha,
    c.ContraEntrega,
    COALESCE(c.ContraEntregaPagado, 0),
    CASE WHEN COALESCE(c.ContraEntregaPagado, 0) >= COALESCE(c.ContraEntrega, 0) THEN 'PAG' ELSE 'PEN' END,
    0
FROM comprobantes c
WHERE COALESCE(c.ContraEntrega, 0) > 0;

-- Step 4: Add index for better performance on numero_cuota queries
CREATE INDEX idx_cuotas_numero ON cuotas (comprobante_id, numero_cuota);
