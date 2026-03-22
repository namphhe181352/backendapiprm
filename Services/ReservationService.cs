using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Reservations;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ITableRepository _tableRepository;
    private readonly INotificationService _notificationService;

    public ReservationService(
        IReservationRepository reservationRepository,
        ITableRepository tableRepository,
        INotificationService notificationService)
    {
        _reservationRepository = reservationRepository;
        _tableRepository = tableRepository;
        _notificationService = notificationService;
    }

    public async Task<PagedResult<ReservationDto>> GetFilteredAsync(ReservationQueryParams queryParams)
    {
        var result = await _reservationRepository.GetFilteredAsync(queryParams);
        return new PagedResult<ReservationDto>
        {
            Items = result.Items.Select(MapReservation).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<ReservationDto?> GetByIdAsync(int id)
    {
        var reservation = await _reservationRepository.GetDetailByIdAsync(id);
        return reservation is null ? null : MapReservation(reservation);
    }

    public async Task<ReservationDto> CheckInAsync(int staffId, ReservationCheckInRequest request)
    {
        var table = await _tableRepository.GetByIdAsync(request.TableId)
            ?? throw new InvalidOperationException("Table does not exist.");

        if (!table.IsActive || !string.Equals(table.Status, "available", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Check-in only allowed for active and available tables.");
        }

        var reservation = new Reservation
        {
            TableId = request.TableId,
            StaffId = staffId,
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            GuestCount = request.GuestCount,
            Note = request.Note
        };

        var reservationId = await _reservationRepository.CreateWithOrderByStoredProcedureAsync(reservation);
        if (reservationId <= 0)
        {
            throw new InvalidOperationException("Cannot create reservation with order.");
        }

        var created = await _reservationRepository.GetDetailByIdAsync(reservationId)
            ?? throw new KeyNotFoundException("Created reservation not found.");

        await _notificationService.CreateOrderIncomingNotificationAsync(
            created.Id,
            created.Order?.Id,
            created.Table?.Name ?? created.TableId.ToString(),
            created.CustomerName,
            created.GuestCount);

        return MapReservation(created);
    }

    public async Task<bool> UpdateAsync(int id, ReservationRequest request)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation is null)
        {
            return false;
        }

        reservation.TableId = request.TableId;
        reservation.CustomerName = request.CustomerName;
        reservation.CustomerPhone = request.CustomerPhone;
        reservation.GuestCount = request.GuestCount;
        reservation.Note = request.Note;
        reservation.Status = request.Status;

        _reservationRepository.Update(reservation);
        await _reservationRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelAsync(int id)
    {
        var reservation = await _reservationRepository.Query()
            .Include(x => x.Table)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (reservation is null)
        {
            return false;
        }

        reservation.Status = "cancelled";
        reservation.CheckOutTime = DateTime.UtcNow;
        reservation.Table.Status = "available";

        _reservationRepository.Update(reservation);
        await _reservationRepository.SaveChangesAsync();
        return true;
    }

    private static ReservationDto MapReservation(Reservation reservation) => new()
    {
        Id = reservation.Id,
        TableId = reservation.TableId,
        TableName = reservation.Table?.Name ?? string.Empty,
        StaffId = reservation.StaffId,
        StaffName = reservation.Staff?.FullName ?? string.Empty,
        CustomerName = reservation.CustomerName,
        CustomerPhone = reservation.CustomerPhone,
        GuestCount = reservation.GuestCount,
        CheckInTime = reservation.CheckInTime,
        CheckOutTime = reservation.CheckOutTime,
        Note = reservation.Note,
        Status = reservation.Status,
        CreatedAt = reservation.CreatedAt,
        OrderId = reservation.Order?.Id
    };
}
