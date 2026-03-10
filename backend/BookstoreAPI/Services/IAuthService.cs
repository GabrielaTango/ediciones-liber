using BookstoreAPI.DTOs;

namespace BookstoreAPI.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
        Task<UsuarioDto> CreateUsuarioAsync(CreateUsuarioDto createDto);
        Task<UsuarioDto?> UpdateUsuarioAsync(int id, UpdateUsuarioDto updateDto);
        Task<bool> ChangePasswordAsync(int id, ChangePasswordDto changePasswordDto);
        Task<bool> DeleteUsuarioAsync(int id);
        Task<IEnumerable<UsuarioDto>> GetAllUsuariosAsync();
        Task<UsuarioDto?> GetUsuarioByIdAsync(int id);
    }
}
