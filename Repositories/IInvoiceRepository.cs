using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Invoices;
using BusinessObjects.Models;

namespace Repositories;

public interface IInvoiceRepository : IGenericRepository<Invoice>
{
    Task<Invoice?> GetDetailByIdAsync(int id);
    Task<PagedResult<Invoice>> GetFilteredAsync(InvoiceQueryParams queryParams);
    Task<int> CheckoutByStoredProcedureAsync(int orderId, int staffId, string paymentMethod, decimal taxAmount, decimal discountAmount, decimal tipAmount);
}
