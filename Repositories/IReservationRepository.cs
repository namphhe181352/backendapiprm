using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Reservations;
using BusinessObjects.Models;

namespace Repositories;

public interface IReservationRepository : IGenericRepository<Reservation>
{
    Task<PagedResult<Reservation>> GetFilteredAsync(ReservationQueryParams queryParams);
    Task<Reservation?> GetDetailByIdAsync(int id);
    Task<int> CreateWithOrderByStoredProcedureAsync(Reservation reservation);
}
