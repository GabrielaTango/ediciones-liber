-- Seed: Default admin user (admin / admin123)
-- BCrypt hash of "admin123"
INSERT INTO usuarios (Username, PasswordHash, NombreCompleto, Rol, Activo, FechaCreacion)
SELECT 'admin', '$2b$10$KTrErMTdSzzdndG5hQC9W.YQinBwciG9N9vlwuacfffIVnSfvn9X.', 'Administrador', 'admin', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM usuarios WHERE Username = 'admin');

-- Default backup path
INSERT INTO configuracion (Clave, Valor)
SELECT 'Backup_RutaCarpeta', '/backup'
WHERE NOT EXISTS (SELECT 1 FROM configuracion WHERE Clave = 'Backup_RutaCarpeta');
