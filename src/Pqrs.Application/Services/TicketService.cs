using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pqrs.Application.DTOs.Common;
using Pqrs.Application.DTOs.Ticket;
using Pqrs.Application.DTOs.Widget;
using Pqrs.Application.Exceptions;
using Pqrs.Application.Interfaces;
using Pqrs.Domain.Entities;
using Pqrs.Domain.Enums;
using Pqrs.Domain.Interfaces;

namespace Pqrs.Application.Services;

public class TicketService
{
    private readonly IApplicationDbContext _context;
    private readonly ITicketQueue _ticketQueue;
    private readonly ITenantContext _tenantContext;

    public TicketService(
        IApplicationDbContext context, 
        ITicketQueue ticketQueue, 
        ITenantContext tenantContext)
    {
        _context = context;
        _ticketQueue = ticketQueue;
        _tenantContext = tenantContext;
    }

    public async Task<TicketDto> CreateWidgetTicketAsync(CreateTicketWidgetDto dto, Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.CustomerName) || string.IsNullOrWhiteSpace(dto.CustomerEmail) ||
            string.IsNullOrWhiteSpace(dto.Subject) || string.IsNullOrWhiteSpace(dto.Description))
        {
            throw new ValidationException("CustomerName, CustomerEmail, Subject, and Description are required.");
        }

        var ticket = new Ticket
        {
            TenantId = tenantId,
            CustomerName = dto.CustomerName.Trim(),
            CustomerEmail = dto.CustomerEmail.Trim(),
            Subject = dto.Subject.Trim(),
            Description = dto.Description.Trim(),
            Status = TicketStatus.TRIAGE_PENDING,
            Type = TicketType.PETITION,
            Priority = Priority.MEDIUM,
            Sentiment = Sentiment.NEUTRAL,
            Summary = string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync(cancellationToken);

        // Queue ticket for background AI triage
        await _ticketQueue.EnqueueTicketAsync(ticket.Id, cancellationToken);

        return MapToDto(ticket);
    }

    public async Task<PaginatedListDto<TicketDto>> GetTicketsAsync(TicketFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Tickets.AsNoTracking();

        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        if (filter.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == filter.Priority.Value);
        }

        if (filter.Type.HasValue)
        {
            query = query.Where(t => t.Type == filter.Type.Value);
        }

        if (filter.Sentiment.HasValue)
        {
            query = query.Where(t => t.Sentiment == filter.Sentiment.Value);
        }

        query = query.OrderByDescending(t => t.CreatedAt);

        int count = await query.CountAsync(cancellationToken);
        int page = filter.Page < 1 ? 1 : filter.Page;
        int pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => MapToDto(t))
            .ToListAsync(cancellationToken);

        return new PaginatedListDto<TicketDto>(items, count, page, pageSize);
    }

    public async Task<TicketDto> GetTicketByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ticket = await _context.Tickets
            .Include(t => t.StatusHistory)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (ticket == null)
        {
            throw new NotFoundException("Ticket", id);
        }

        return MapToDto(ticket);
    }

    public async Task<TicketDto> UpdateTicketStatusAsync(Guid id, UpdateTicketStatusDto dto, string changedBy, CancellationToken cancellationToken = default)
    {
        var ticket = await _context.Tickets
            .Include(t => t.StatusHistory)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (ticket == null)
        {
            throw new NotFoundException("Ticket", id);
        }

        var previousStatus = ticket.Status;
        ticket.Status = dto.Status;
        if (dto.Priority.HasValue)
        {
            ticket.Priority = dto.Priority.Value;
        }
        ticket.UpdatedAt = DateTime.UtcNow;

        _context.TicketStatusHistories.Add(new TicketStatusHistory
        {
            TenantId = ticket.TenantId,
            TicketId = ticket.Id,
            PreviousStatus = previousStatus,
            NewStatus = dto.Status,
            ChangedBy = changedBy,
            Reason = dto.Reason,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(ticket);
    }

    private static TicketDto MapToDto(Ticket ticket)
    {
        return new TicketDto
        {
            Id = ticket.Id,
            TenantId = ticket.TenantId,
            CustomerName = ticket.CustomerName,
            CustomerEmail = ticket.CustomerEmail,
            Subject = ticket.Subject,
            Description = ticket.Description,
            Type = ticket.Type,
            Status = ticket.Status,
            Priority = ticket.Priority,
            Sentiment = ticket.Sentiment,
            Summary = ticket.Summary,
            ResolvedByRag = ticket.ResolvedByRag,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            StatusHistory = ticket.StatusHistory?.Select(sh => new TicketStatusHistoryDto
            {
                Id = sh.Id,
                TicketId = sh.TicketId,
                PreviousStatus = sh.PreviousStatus,
                NewStatus = sh.NewStatus,
                ChangedBy = sh.ChangedBy,
                Reason = sh.Reason,
                CreatedAt = sh.CreatedAt
            }).OrderBy(sh => sh.CreatedAt).ToList() ?? new()
        };
    }
}
