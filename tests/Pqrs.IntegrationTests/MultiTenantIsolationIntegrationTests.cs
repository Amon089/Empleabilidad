using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Pqrs.Application.DTOs.Auth;
using Pqrs.Application.DTOs.Common;
using Pqrs.Application.DTOs.Ticket;
using Pqrs.Application.DTOs.Widget;
using Xunit;

namespace Pqrs.IntegrationTests;

public class MultiTenantIsolationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MultiTenantIsolationIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Auth_Login_WithValidCredentials_ReturnsAccessToken()
    {
        var request = new LoginRequestDto
        {
            Email = "admin@leggumbres.local",
            Password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
    }

    [Fact]
    public async Task WidgetRag_TenantA_ReturnsTenantAAnswer()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/widget/rag-search");
        request.Headers.Add("X-Widget-Key", "pk_live_escoba_12345");
        request.Content = JsonContent.Create(new { query = "Zonas de cobertura y horarios de entrega" });

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RagSearchResponseDto>();
        Assert.NotNull(result);
        Assert.True(result.Resolved, $"Expected resolved true but topScore was {result?.TopScore}");
        Assert.Contains("Leggumbres", result.Answer ?? string.Empty, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task WidgetRag_CrossTenantQuery_ReturnsResolvedFalse_WithoutDataLeak()
    {
        // Tenant A key asking about metal structures (Tenant B's knowledge domain)
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/widget/rag-search");
        request.Headers.Add("X-Widget-Key", "pk_live_escoba_12345");
        request.Content = JsonContent.Create(new { query = "Como solicito una visita tecnica para una estructura metalica o puente?" });

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RagSearchResponseDto>();
        Assert.NotNull(result);
        Assert.False(result.Resolved);
        Assert.Empty(result.Sources);
    }

    [Fact]
    public async Task Tickets_TenantIsolation_OnlyReturnsOwnTenantTickets()
    {
        // 1. Login as Tenant A Admin
        var loginA = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequestDto { Email = "admin@leggumbres.local", Password = "Password123!" });
        var authA = await loginA.Content.ReadFromJsonAsync<AuthResponseDto>();

        var reqA = new HttpRequestMessage(HttpMethod.Get, "/api/v1/tickets");
        reqA.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authA!.AccessToken);
        var respA = await _client.SendAsync(reqA);
        var ticketsA = await respA.Content.ReadFromJsonAsync<PaginatedListDto<TicketDto>>();

        // 2. Login as Tenant B Admin
        var loginB = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequestDto { Email = "admin@todometal.local", Password = "Password123!" });
        var authB = await loginB.Content.ReadFromJsonAsync<AuthResponseDto>();

        var reqB = new HttpRequestMessage(HttpMethod.Get, "/api/v1/tickets");
        reqB.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authB!.AccessToken);
        var respB = await _client.SendAsync(reqB);
        var ticketsB = await respB.Content.ReadFromJsonAsync<PaginatedListDto<TicketDto>>();

        Assert.NotEmpty(ticketsA!.Items);
        Assert.NotEmpty(ticketsB!.Items);

        // Verify none of Tenant A tickets belong to Tenant B and vice versa
        foreach (var ticket in ticketsA.Items)
        {
            Assert.DoesNotContain(ticketsB.Items, t => t.Id == ticket.Id);
        }
    }

    [Fact]
    public async Task Tickets_CrossTenantDirectAccess_IsRejectedWith404NotFound()
    {
        // 1. Get a Ticket ID from Tenant B
        var loginB = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequestDto { Email = "admin@todometal.local", Password = "Password123!" });
        var authB = await loginB.Content.ReadFromJsonAsync<AuthResponseDto>();

        var reqB = new HttpRequestMessage(HttpMethod.Get, "/api/v1/tickets");
        reqB.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authB!.AccessToken);
        var respB = await _client.SendAsync(reqB);
        var ticketsB = await respB.Content.ReadFromJsonAsync<PaginatedListDto<TicketDto>>();
        var tenantBTicketId = ticketsB!.Items[0].Id;

        // 2. Tenant A tries to access Tenant B's ticket directly by ID
        var loginA = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequestDto { Email = "admin@leggumbres.local", Password = "Password123!" });
        var authA = await loginA.Content.ReadFromJsonAsync<AuthResponseDto>();

        var reqA = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/tickets/{tenantBTicketId}");
        reqA.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authA!.AccessToken);
        var respA = await _client.SendAsync(reqA);

        // Security check: Must be rejected with 404 Not Found
        Assert.Equal(HttpStatusCode.NotFound, respA.StatusCode);
    }
}
