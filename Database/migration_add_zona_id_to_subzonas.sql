-- Migration: Add zona_id to subzonas table
-- Run this script against the BookstoreApp database

ALTER TABLE subzonas ADD COLUMN zona_id int DEFAULT NULL AFTER descripcion;
ALTER TABLE subzonas ADD KEY FK_subzonas_zonas (zona_id);
ALTER TABLE subzonas ADD CONSTRAINT FK_subzonas_zonas FOREIGN KEY (zona_id) REFERENCES zonas(id);
