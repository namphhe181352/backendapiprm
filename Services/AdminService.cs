using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Admin;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace Services;

public class AdminService : IAdminService
{
    private readonly IGenericRepository<DiningTable> _tableRepository;
    private readonly IGenericRepository<Order> _orderRepository;
    private readonly IGenericRepository<Invoice> _invoiceRepository;
    private readonly IGenericRepository<OrderDetail> _orderDetailRepository;
    private readonly IUserRepository _userRepository;

    public AdminService(
        IGenericRepository<DiningTable> tableRepository,
        IGenericRepository<Order> orderRepository,
        IGenericRepository<Invoice> invoiceRepository,
        IGenericRepository<OrderDetail> orderDetailRepository,
        IUserRepository userRepository)
    {
        _tableRepository = tableRepository;
        _orderRepository = orderRepository;
        _invoiceRepository = invoiceRepository;
        _orderDetailRepository = orderDetailRepository;
        _userRepository = userRepository;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var totalTables = await _tableRepository.Query().CountAsync();
        var occupiedTables = await _tableRepository.Query().CountAsync(x => x.Status == "occupied");
        var todayOrders = await _orderRepository.Query().CountAsync(x => x.CreatedAt >= today && x.CreatedAt < tomorrow);
        var todayRevenue = await _invoiceRepository.Query()
            .Where(x => x.PaidAt >= today && x.PaidAt < tomorrow)
            .SumAsync(x => (decimal?)x.FinalTotal) ?? 0;

        return new DashboardSummaryDto
        {
            TotalTables = totalTables,
            OccupiedTables = occupiedTables,
            TodayOrders = todayOrders,
            TodayRevenue = todayRevenue
        };
    }

    public async Task<RevenueStatisticsDto> GetRevenueStatisticsAsync(string period)
    {
        var now = DateTime.UtcNow;
        DateTime from;
        DateTime to;

        switch (period.ToLowerInvariant())
        {
            case "today":
                from = now.Date;
                to = from.AddDays(1);
                break;
            case "week":
                var diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                from = now.Date.AddDays(-diff);
                to = from.AddDays(7);
                break;
            case "month":
                from = new DateTime(now.Year, now.Month, 1);
                to = from.AddMonths(1);
                break;
            default:
                throw new InvalidOperationException("Period must be today|week|month.");
        }

        var query = _invoiceRepository.Query().Where(x => x.PaidAt >= from && x.PaidAt < to);
        var revenue = await query.SumAsync(x => (decimal?)x.FinalTotal) ?? 0;
        var totalInvoices = await query.CountAsync();

        return new RevenueStatisticsDto
        {
            Period = period,
            Revenue = revenue,
            TotalInvoices = totalInvoices
        };
    }

    public async Task<IEnumerable<TopItemDto>> GetTopItemsAsync(DateTime? from, DateTime? to, int limit)
    {
        limit = limit <= 0 ? 10 : limit;

        var query = _orderDetailRepository.Query()
            .Include(x => x.MenuItem)
            .Include(x => x.Order)
            .AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(x => x.Order.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.Order.CreatedAt <= to.Value);
        }

        return await query
            .GroupBy(x => new { x.MenuItemId, x.MenuItem.Name })
            .Select(g => new TopItemDto
            {
                MenuItemId = g.Key.MenuItemId,
                MenuItemName = g.Key.Name,
                TotalQuantity = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<PagedResult<StaffDto>> GetStaffAsync(StaffQueryParams queryParams)
    {
        var query = _userRepository.Query().Where(x => x.Role == "staff");

        if (!string.IsNullOrWhiteSpace(queryParams.Keyword))
        {
            var keyword = queryParams.Keyword.Trim();
            query = query.Where(x => x.FullName.Contains(keyword) || x.Username.Contains(keyword) || (x.Email != null && x.Email.Contains(keyword)));
        }

        if (queryParams.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == queryParams.IsActive.Value);
        }

        var page = queryParams.Page <= 0 ? 1 : queryParams.Page;
        var pageSize = queryParams.PageSize <= 0 ? 10 : queryParams.PageSize;

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<StaffDto>
        {
            Items = items.Select(MapStaff).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<StaffDto?> GetStaffByIdAsync(int id)
    {
        var staff = await _userRepository.Query().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.Role == "staff");
        return staff is null ? null : MapStaff(staff);
    }

    public async Task<StaffDto> CreateStaffAsync(StaffRequest request)
    {
        var existedUsername = await _userRepository.GetByUsernameAsync(request.Username);
        if (existedUsername is not null)
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
            Role = "staff",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();
        return MapStaff(user);
    }

    public async Task<bool> UpdateStaffAsync(int id, StaffUpdateRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null || user.Role != "staff")
        {
            return false;
        }

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.Phone = request.Phone;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleStaffActiveAsync(int id, bool isActive)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null || user.Role != "staff")
        {
            return false;
        }

        user.IsActive = isActive;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    private static StaffDto MapStaff(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Username = user.Username,
        Email = user.Email,
        Phone = user.Phone,
        Role = user.Role,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}
