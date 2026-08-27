using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pqrs.Application.DTOs.Common;
using Pqrs.Application.DTOs.Ticket;
using Pqrs.Application.Services;

namespace Pqrs.API.Controllers;

[ApiController]
[Route("api/v1/tickets")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly TicketService _ticketService;

    public TicketsController(TicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedListDto<TicketDto>>> GetTickets([FromQuery] TicketFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await _ticketService.GetTicketsAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketDto>> GetTicketById(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.GetTicketByIdAsync(id, cancellationToken);
        return Ok(ticket);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<TicketDto>> UpdateTicketStatus(Guid id, [FromBody] UpdateTicketStatusDto dto, CancellationToken cancellationToken)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email") ?? "SYSTEM_AGENT";
        var ticket = await _ticketService.UpdateTicketStatusAsync(id, dto, userEmail, cancellationToken);
        return Ok(ticket);
    }
}
