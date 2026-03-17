using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Reservations;

namespace Services;

public interface IReservationService
{
    Task<PagedResult<ReservationDto>> GetFilteredAsync(ReservationQueryParams queryParams);
    Task<ReservationDto?> GetByIdAsync(int id);
    Task<ReservationDto> CheckInAsync(int staffId, ReservationCheckInRequest request);
    Task<bool> UpdateAsync(int id, ReservationRequest request);
    Task<bool> CancelAsync(int id);
}
