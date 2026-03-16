using BusinessObjects.DTOs.Settings;
using Repositories;

namespace Services;

public class SettingsService : ISettingsService
{
    private readonly IUserRepository _userRepository;

    public SettingsService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<MySettingsDto> GetMySettingsAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        return new MySettingsDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Phone = user.Phone,
            Email = user.Email
        };
    }

    public async Task<bool> UpdateMySettingsAsync(int userId, UpdateMySettingsRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        user.FullName = request.FullName;
        user.Phone = request.Phone;
        user.Email = request.Email;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }
}
