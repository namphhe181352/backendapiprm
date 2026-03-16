using System.Security.Claims;
using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace backendapi.Controllers;

[ApiController]
[Route("api/checkout")]
[Authorize(Roles = "admin,staff")]
public class CheckoutController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public CheckoutController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> Checkout([FromBody] CheckoutRequest request)
    {
        var staffId = GetCurrentUserId();
        var result = await _invoiceService.CheckoutAsync(staffId, request);
        return Ok(ApiResponse<InvoiceDto>.Ok(result, "Checkout successful"));
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
