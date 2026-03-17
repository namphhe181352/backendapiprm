using System.Security.Claims;
using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace backendapi.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize(Roles = "admin,staff")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<MySettingsDto>>> GetMe()
    {
        var userId = GetCurrentUserId();
        var result = await _settingsService.GetMySettingsAsync(userId);
        return Ok(ApiResponse<MySettingsDto>.Ok(result));
    }

    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse>> UpdateMe([FromBody] UpdateMySettingsRequest request)
    {
        var userId = GetCurrentUserId();
        var success = await _settingsService.UpdateMySettingsAsync(userId, request);
        if (!success)
        {
            return NotFound(ApiResponse.Fail("User not found"));
        }

        return Ok(ApiResponse.Ok("Settings updated"));
    }

    private int GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || !int.TryParse(userId, out var parsed))
        {
            throw new UnauthorizedAccessException("Invalid token.");
        }

        return parsed;
    }
}
