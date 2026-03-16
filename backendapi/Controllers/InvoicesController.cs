using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace backendapi.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize(Roles = "admin,staff")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> GetById(int id)
    {
        var result = await _invoiceService.GetByIdAsync(id);
        if (result is null)
        {
            return NotFound(ApiResponse<InvoiceDto>.Fail("Invoice not found"));
        }

        return Ok(ApiResponse<InvoiceDto>.Ok(result));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<InvoiceDto>>>> Get([FromQuery] InvoiceQueryParams queryParams)
    {
        var result = await _invoiceService.GetFilteredAsync(queryParams);
        return Ok(ApiResponse<PagedResult<InvoiceDto>>.Ok(result));
    }
}
