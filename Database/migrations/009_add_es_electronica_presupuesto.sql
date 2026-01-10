-- Migration: Add EsElectronica and EsPresupuesto to comprobantes table
-- Date: 2026-01-03
-- Description: Adds fields to differentiate electronic invoices from manual/budget

ALTER TABLE comprobantes
ADD COLUMN EsElectronica TINYINT(1) NOT NULL DEFAULT 1,
ADD COLUMN EsPresupuesto TINYINT(1) NOT NULL DEFAULT 0;

-- Index for filtering by type
CREATE INDEX idx_comprobantes_tipo ON comprobantes (EsElectronica, EsPresupuesto);
