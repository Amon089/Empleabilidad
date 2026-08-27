using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Pqrs.Application.DTOs.Ticket;
using Pqrs.Application.DTOs.Widget;
using Pqrs.Application.Services;
using Pqrs.Domain.Interfaces;

namespace Pqrs.API.Controllers;

[ApiController]
[Route("api/v1/widget")]
[AllowAnonymous]
[EnableRateLimiting("WidgetPolicy")]
public class WidgetController : ControllerBase
{
    private readonly RagService _ragService;
    private readonly TicketService _ticketService;
    private readonly ITenantContext _tenantContext;
    private readonly IConfiguration _configuration;

    public WidgetController(
        RagService ragService, 
        TicketService ticketService, 
        ITenantContext tenantContext, 
        IConfiguration configuration)
    {
        _ragService = ragService;
        _ticketService = ticketService;
        _tenantContext = tenantContext;
        _configuration = configuration;
    }

    [HttpPost("rag-search")]
    public async Task<ActionResult<RagSearchResponseDto>> RagSearch([FromBody] RagSearchRequestDto request, CancellationToken cancellationToken)
    {
        double threshold = double.TryParse(_configuration["Rag:SimilarityThreshold"], NumberStyles.Any, CultureInfo.InvariantCulture, out var t) ? t : 0.35;
        int topK = int.TryParse(_configuration["Rag:TopK"], out var k) ? k : 5;

        var result = await _ragService.SearchAndAnswerAsync(
            request.Query, 
            _tenantContext.TenantId, 
            threshold, 
            topK, 
            cancellationToken: cancellationToken);

        return Ok(result);
    }

    [HttpPost("tickets")]
    public async Task<ActionResult<TicketDto>> CreateTicket([FromBody] CreateTicketWidgetDto request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.CreateWidgetTicketAsync(request, _tenantContext.TenantId, cancellationToken);
        return CreatedAtAction(nameof(TicketsController.GetTicketById), "Tickets", new { id = ticket.Id }, ticket);
    }
}
