using System.Security.Claims;
using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Reservations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace backendapi.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize(Roles = "admin,staff")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ReservationDto>>>> Get([FromQuery] ReservationQueryParams queryParams)
    {
        var result = await _reservationService.GetFilteredAsync(queryParams);
        return Ok(ApiResponse<PagedResult<ReservationDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ReservationDto>>> GetById(int id)
    {
        var result = await _reservationService.GetByIdAsync(id);
        if (result is null)
        {
            return NotFound(ApiResponse<ReservationDto>.Fail("Reservation not found"));
        }

        return Ok(ApiResponse<ReservationDto>.Ok(result));
    }

    [HttpPost("check-in")]
    public async Task<ActionResult<ApiResponse<ReservationDto>>> CheckIn([FromBody] ReservationCheckInRequest request)
    {
        var staffId = GetCurrentUserId();
        var result = await _reservationService.CheckInAsync(staffId, request);
        return Ok(ApiResponse<ReservationDto>.Ok(result, "Check-in successful"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] ReservationRequest request)
    {
        var success = await _reservationService.UpdateAsync(id, request);
        if (!success)
        {
            return NotFound(ApiResponse.Fail("Reservation not found"));
        }

        return Ok(ApiResponse.Ok("Reservation updated"));
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<ActionResult<ApiResponse>> Cancel(int id)
    {
        var success = await _reservationService.CancelAsync(id);
        if (!success)
        {
            return NotFound(ApiResponse.Fail("Reservation not found"));
        }

        return Ok(ApiResponse.Ok("Reservation cancelled"));
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
