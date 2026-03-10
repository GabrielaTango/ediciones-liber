-- Tabla de usuarios para autenticación
CREATE TABLE IF NOT EXISTS usuarios (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    NombreCompleto VARCHAR(100) NOT NULL,
    Email VARCHAR(100),
    Rol VARCHAR(20) NOT NULL DEFAULT 'usuario',
    Activo BOOLEAN NOT NULL DEFAULT TRUE,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UltimoAcceso DATETIME NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Usuario admin por defecto (password: admin123)
-- El hash se genera con BCrypt
INSERT INTO usuarios (Username, PasswordHash, NombreCompleto, Email, Rol, Activo)
VALUES ('admin', '$2a$11$q.ntnyyIzpUiXvdQ0QiISeRKmYS79caDD6qk8FhyCF6DyZ6rWITUS', 'Administrador', 'admin@edicionesliber.com', 'admin', TRUE)
ON DUPLICATE KEY UPDATE Username = Username;
