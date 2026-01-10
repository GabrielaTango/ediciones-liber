-- Migration: Add GastosEnvio to comprobantes table
-- Date: 2026-01-03
-- Description: Adds GastosEnvio field for shipping costs

ALTER TABLE comprobantes
ADD COLUMN GastosEnvio decimal(17,2) DEFAULT NULL;
