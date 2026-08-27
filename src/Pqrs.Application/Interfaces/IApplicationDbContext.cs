using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pqrs.Domain.Entities;

namespace Pqrs.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<KnowledgeBaseArticle> KnowledgeBaseArticles { get; }
    DbSet<Ticket> Tickets { get; }
    DbSet<TicketStatusHistory> TicketStatusHistories { get; }
    DbSet<RagInteraction> RagInteractions { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
