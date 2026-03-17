using BusinessObjects.DTOs.Settings;

namespace Services;

public interface ISettingsService
{
    Task<MySettingsDto> GetMySettingsAsync(int userId);
    Task<bool> UpdateMySettingsAsync(int userId, UpdateMySettingsRequest request);
}
