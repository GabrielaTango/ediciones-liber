using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookstoreAPI.DTOs;
using BookstoreAPI.Models;
using BookstoreAPI.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace BookstoreAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
        {
            _usuarioRepository = usuarioRepository;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var usuario = await _usuarioRepository.GetByUsernameAsync(loginDto.Username);
            if (usuario == null || !usuario.Activo)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, usuario.PasswordHash))
                return null;

            await _usuarioRepository.UpdateUltimoAccesoAsync(usuario.Id);

            var token = GenerateJwtToken(usuario);

            return new LoginResponseDto
            {
                Token = token,
                Usuario = MapToDto(usuario)
            };
        }

        public async Task<UsuarioDto> CreateUsuarioAsync(CreateUsuarioDto createDto)
        {
            var existing = await _usuarioRepository.GetByUsernameAsync(createDto.Username);
            if (existing != null)
                throw new InvalidOperationException("El nombre de usuario ya existe");

            var usuario = new Usuario
            {
                Username = createDto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(createDto.Password),
                NombreCompleto = createDto.NombreCompleto,
                Email = createDto.Email,
                Rol = createDto.Rol,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            var id = await _usuarioRepository.CreateAsync(usuario);
            usuario.Id = id;

            return MapToDto(usuario);
        }

        public async Task<UsuarioDto?> UpdateUsuarioAsync(int id, UpdateUsuarioDto updateDto)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
                return null;

            usuario.NombreCompleto = updateDto.NombreCompleto;
            usuario.Email = updateDto.Email;
            usuario.Rol = updateDto.Rol;
            usuario.Activo = updateDto.Activo;

            await _usuarioRepository.UpdateAsync(id, usuario);

            return MapToDto(usuario);
        }

        public async Task<bool> ChangePasswordAsync(int id, ChangePasswordDto changePasswordDto)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
                return false;

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            return await _usuarioRepository.UpdatePasswordAsync(id, passwordHash);
        }

        public async Task<bool> DeleteUsuarioAsync(int id)
        {
            return await _usuarioRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<UsuarioDto>> GetAllUsuariosAsync()
        {
            var usuarios = await _usuarioRepository.GetAllAsync();
            return usuarios.Select(MapToDto);
        }

        public async Task<UsuarioDto?> GetUsuarioByIdAsync(int id)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            return usuario == null ? null : MapToDto(usuario);
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Username),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim("nombreCompleto", usuario.NombreCompleto)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static UsuarioDto MapToDto(Usuario usuario)
        {
            return new UsuarioDto
            {
                Id = usuario.Id,
                Username = usuario.Username,
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email,
                Rol = usuario.Rol,
                Activo = usuario.Activo,
                FechaCreacion = usuario.FechaCreacion,
                UltimoAcceso = usuario.UltimoAcceso
            };
        }
    }
}
