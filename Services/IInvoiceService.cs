using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Invoices;

namespace Services;

public interface IInvoiceService
{
    Task<InvoiceDto> CheckoutAsync(int staffId, CheckoutRequest request);
    Task<InvoiceDto?> GetByIdAsync(int id);
    Task<PagedResult<InvoiceDto>> GetFilteredAsync(InvoiceQueryParams queryParams);
}
