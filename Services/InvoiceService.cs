using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Invoices;
using BusinessObjects.Models;
using Repositories;

namespace Services;

public class InvoiceService : IInvoiceService
{
    private static readonly HashSet<string> AllowedPaymentMethods =
    [
        "cash", "card", "qr_transfer"
    ];

    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IOrderRepository _orderRepository;

    public InvoiceService(IInvoiceRepository invoiceRepository, IOrderRepository orderRepository)
    {
        _invoiceRepository = invoiceRepository;
        _orderRepository = orderRepository;
    }

    public async Task<InvoiceDto> CheckoutAsync(int staffId, CheckoutRequest request)
    {
        if (!AllowedPaymentMethods.Contains(request.PaymentMethod))
        {
            throw new InvalidOperationException("Payment method must be cash, card, or qr_transfer.");
        }

        var order = await _orderRepository.GetDetailByIdAsync(request.OrderId)
            ?? throw new KeyNotFoundException("Order not found.");

        if (order.Invoice is not null)
        {
            throw new InvalidOperationException("Order already has invoice, cannot checkout again.");
        }

        var invoiceId = await _invoiceRepository.CheckoutByStoredProcedureAsync(
            request.OrderId,
            staffId,
            request.PaymentMethod,
            request.TaxAmount,
            request.DiscountAmount,
            request.TipAmount);

        if (invoiceId <= 0)
        {
            throw new InvalidOperationException("Checkout failed.");
        }

        var invoice = await _invoiceRepository.GetDetailByIdAsync(invoiceId)
            ?? throw new KeyNotFoundException("Created invoice not found.");

        return MapInvoice(invoice);
    }

    public async Task<InvoiceDto?> GetByIdAsync(int id)
    {
        var invoice = await _invoiceRepository.GetDetailByIdAsync(id);
        return invoice is null ? null : MapInvoice(invoice);
    }

    public async Task<PagedResult<InvoiceDto>> GetFilteredAsync(InvoiceQueryParams queryParams)
    {
        var result = await _invoiceRepository.GetFilteredAsync(queryParams);
        return new PagedResult<InvoiceDto>
        {
            Items = result.Items.Select(MapInvoice).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    private static InvoiceDto MapInvoice(Invoice invoice) => new()
    {
        Id = invoice.Id,
        OrderId = invoice.OrderId,
        StaffId = invoice.StaffId,
        PaymentMethod = invoice.PaymentMethod,
        TaxAmount = invoice.TaxAmount,
        DiscountAmount = invoice.DiscountAmount,
        TipAmount = invoice.TipAmount,
        FinalTotal = invoice.FinalTotal,
        PaidAt = invoice.PaidAt
    };
}
