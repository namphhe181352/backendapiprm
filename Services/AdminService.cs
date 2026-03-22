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
        var normalizedPeriod = NormalizePeriod(period);
        var now = DateTime.UtcNow;
        var periodRange = ResolvePeriodRange(normalizedPeriod, now);

        var currentRevenueQuery = _invoiceRepository.Query()
            .Where(x => x.PaidAt >= periodRange.From && x.PaidAt < periodRange.To);

        var previousRevenueQuery = _invoiceRepository.Query()
            .Where(x => x.PaidAt >= periodRange.PreviousFrom && x.PaidAt < periodRange.PreviousTo);

        var revenue = await currentRevenueQuery.SumAsync(x => (decimal?)x.FinalTotal) ?? 0;
        var totalInvoices = await currentRevenueQuery.CountAsync();
        var previousRevenue = await previousRevenueQuery.SumAsync(x => (decimal?)x.FinalTotal) ?? 0;

        var chartData = await BuildRevenueChartAsync(normalizedPeriod, periodRange.From, periodRange.To);
        var revenueChangePercent = CalculateGrowthPercent(revenue, previousRevenue);

        return new RevenueStatisticsDto
        {
            Period = normalizedPeriod,
            Title = periodRange.Title,
            Subtitle = periodRange.Subtitle,
            Revenue = revenue,
            PreviousRevenue = previousRevenue,
            RevenueChangePercent = revenueChangePercent,
            TotalInvoices = totalInvoices,
            ChartValues = chartData.Values,
            ChartLabels = chartData.Labels,
        };
    }

    public async Task<IEnumerable<TopItemDto>> GetTopItemsAsync(DateTime? from, DateTime? to, int limit)
    {
        return await GetTopItemsInternalAsync(from, to, limit);
    }

    public async Task<StatisticsOverviewDto> GetStatisticsOverviewAsync(string period, int topLimit)
    {
        var normalizedPeriod = NormalizePeriod(period);
        var periodRange = ResolvePeriodRange(normalizedPeriod, DateTime.UtcNow);

        var revenue = await GetRevenueStatisticsAsync(normalizedPeriod);
        var topItems = await GetTopItemsInternalAsync(periodRange.From, periodRange.To, topLimit <= 0 ? 5 : topLimit);

        return new StatisticsOverviewDto
        {
            Revenue = revenue,
            TopItems = topItems.ToList()
        };
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

    private async Task<List<TopItemDto>> GetTopItemsInternalAsync(DateTime? from, DateTime? to, int limit)
    {
        limit = limit <= 0 ? 10 : limit;

        var query = _orderDetailRepository.Query()
            .Include(x => x.MenuItem)
                .ThenInclude(x => x.Category)
            .Include(x => x.Order)
            .AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(x => x.Order.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.Order.CreatedAt < to.Value);
        }

        return await query
            .GroupBy(x => new
            {
                x.MenuItemId,
                x.MenuItem.Name,
                CategoryName = x.MenuItem.Category.Name,
                x.MenuItem.ImageUrl
            })
            .Select(g => new TopItemDto
            {
                MenuItemId = g.Key.MenuItemId,
                MenuItemName = g.Key.Name,
                CategoryName = g.Key.CategoryName,
                ImageUrl = g.Key.ImageUrl,
                TotalQuantity = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .ThenByDescending(x => x.TotalRevenue)
            .Take(limit)
            .ToListAsync();
    }

    private async Task<(List<decimal> Values, List<string> Labels)> BuildRevenueChartAsync(string period, DateTime from, DateTime to)
    {
        var values = new List<decimal>();
        var labels = new List<string>();

        if (period == "today")
        {
            var slotHours = new[] { 8, 10, 12, 14, 16, 18, 20 };
            var day = from.Date;

            for (var i = 0; i < slotHours.Length; i++)
            {
                var slotFrom = day.AddHours(slotHours[i]);
                var slotTo = i == slotHours.Length - 1 ? slotFrom.AddHours(2) : day.AddHours(slotHours[i + 1]);

                var slotRevenue = await _invoiceRepository.Query()
                    .Where(x => x.PaidAt >= slotFrom && x.PaidAt < slotTo)
                    .SumAsync(x => (decimal?)x.FinalTotal) ?? 0;

                values.Add(slotRevenue);
                labels.Add(slotFrom.ToString("HH:mm"));
            }

            return (values, labels);
        }

        if (period == "week")
        {
            var dayLabels = new[] { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };

            for (var i = 0; i < 7; i++)
            {
                var dayFrom = from.Date.AddDays(i);
                var dayTo = dayFrom.AddDays(1);

                var dayRevenue = await _invoiceRepository.Query()
                    .Where(x => x.PaidAt >= dayFrom && x.PaidAt < dayTo)
                    .SumAsync(x => (decimal?)x.FinalTotal) ?? 0;

                values.Add(dayRevenue);
                labels.Add(dayLabels[i]);
            }

            return (values, labels);
        }

        var cursor = from;
        var weekIndex = 1;
        while (cursor < to)
        {
            var slotTo = cursor.AddDays(7);
            if (slotTo > to)
            {
                slotTo = to;
            }

            var weekRevenue = await _invoiceRepository.Query()
                .Where(x => x.PaidAt >= cursor && x.PaidAt < slotTo)
                .SumAsync(x => (decimal?)x.FinalTotal) ?? 0;

            values.Add(weekRevenue);
            labels.Add($"Tuần {weekIndex}");

            cursor = slotTo;
            weekIndex++;
        }

        return (values, labels);
    }

    private static string NormalizePeriod(string period)
    {
        var normalized = string.IsNullOrWhiteSpace(period) ? "today" : period.Trim().ToLowerInvariant();
        if (normalized is not ("today" or "week" or "month"))
        {
            throw new InvalidOperationException("Period must be today|week|month.");
        }

        return normalized;
    }

    private static decimal CalculateGrowthPercent(decimal revenue, decimal previousRevenue)
    {
        if (previousRevenue <= 0)
        {
            return revenue > 0 ? 100 : 0;
        }

        return Math.Round((revenue - previousRevenue) / previousRevenue * 100, 2);
    }

    private static (DateTime From, DateTime To, DateTime PreviousFrom, DateTime PreviousTo, string Title, string Subtitle) ResolvePeriodRange(string period, DateTime now)
    {
        switch (period)
        {
            case "today":
            {
                var from = now.Date;
                var to = from.AddDays(1);
                var previousFrom = from.AddDays(-1);
                var previousTo = from;
                return (from, to, previousFrom, previousTo, "Doanh thu hôm nay", "So với cùng kỳ hôm qua");
            }
            case "week":
            {
                var diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                var from = now.Date.AddDays(-diff);
                var to = from.AddDays(7);
                var previousFrom = from.AddDays(-7);
                var previousTo = from;
                return (from, to, previousFrom, previousTo, "Doanh thu tuần này", "So với tuần trước");
            }
            case "month":
            {
                var from = new DateTime(now.Year, now.Month, 1);
                var to = from.AddMonths(1);
                var previousFrom = from.AddMonths(-1);
                var previousTo = from;
                return (from, to, previousFrom, previousTo, "Doanh thu tháng này", "So với tháng trước");
            }
            default:
                throw new InvalidOperationException("Period must be today|week|month.");
        }
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
