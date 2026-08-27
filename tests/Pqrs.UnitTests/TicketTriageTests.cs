using System;
using System.Threading.Tasks;
using Pqrs.Domain.Enums;
using Pqrs.Infrastructure.Services;
using Xunit;

namespace Pqrs.UnitTests;

public class TicketTriageTests
{
    [Fact]
    public async Task TriageTicketAsync_WithDamagedStructuralIssue_CategorizesAsClaimAndHighPriority()
    {
        var httpClient = new System.Net.Http.HttpClient();
        var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();
        var aiService = new AiService(config, httpClient);

        var result = await aiService.TriageTicketAsync(
            "El puente presenta un problema en una union",
            "Se observa corrosion y fisuras en las vigas del soporte central."
        );

        Assert.NotNull(result);
        Assert.Equal(TicketType.CLAIM, result.Type);
        Assert.Equal(Priority.HIGH, result.Priority);
        Assert.Equal(Sentiment.NEGATIVE, result.Sentiment);
        Assert.NotNull(result.Summary);
    }
}
