using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pqrs.Application.DTOs.Triage;
using Pqrs.Domain.Entities;

namespace Pqrs.Application.Interfaces;

public interface IAiService
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    Task<string> GenerateRagAnswerAsync(string query, IEnumerable<KnowledgeBaseArticle> contextArticles, CancellationToken cancellationToken = default);
    Task<TriageResultDto> TriageTicketAsync(string subject, string description, CancellationToken cancellationToken = default);
}
