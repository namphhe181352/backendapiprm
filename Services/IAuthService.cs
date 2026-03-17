using BusinessObjects.DTOs.Auth;

namespace Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<UserDto> MeAsync(int userId);
}
