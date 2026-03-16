using BusinessObjects.DTOs.Auth;
using BusinessObjects.Models;
using Repositories;

namespace Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AuthService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username)
            ?? throw new UnauthorizedAccessException("Invalid username or password.");

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User is inactive.");
        }

        var isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isValid)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existedUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existedUser is not null)
        {
            throw new InvalidOperationException("Username already exists.");
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var existedEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (existedEmail is not null)
            {
                throw new InvalidOperationException("Email already exists.");
            }
        }

        var user = new User
        {
            FullName = request.FullName,
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Email = request.Email,
            Phone = request.Phone,
            Role = string.IsNullOrWhiteSpace(request.Role) ? "staff" : request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return BuildAuthResponse(user);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var isValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
        if (!isValid)
        {
            throw new UnauthorizedAccessException("Current password is incorrect.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task<UserDto> MeAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        return MapUser(user);
    }

    private AuthResponse BuildAuthResponse(User user) => new()
    {
        Token = _jwtService.GenerateToken(user),
        ExpiresAt = _jwtService.GetExpiryUtc(),
        User = MapUser(user)
    };

    private static UserDto MapUser(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Username = user.Username,
        Email = user.Email,
        Phone = user.Phone,
        Role = user.Role,
        CreatedAt = user.CreatedAt,
        IsActive = user.IsActive
    };
}
