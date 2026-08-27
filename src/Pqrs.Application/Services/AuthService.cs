using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pqrs.Application.DTOs.Auth;
using Pqrs.Application.Exceptions;
using Pqrs.Application.Interfaces;

namespace Pqrs.Application.Services;

public class AuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthService(IApplicationDbContext context, IJwtTokenGenerator tokenGenerator)
    {
        _context = context;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ValidationException("Email and password are required.");
        }

        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.IsActive, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            throw new AppException("INVALID_CREDENTIALS", "Invalid email or password.");
        }

        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == user.TenantId, cancellationToken);

        var token = _tokenGenerator.GenerateToken(user);

        return new AuthResponseDto
        {
            AccessToken = token,
            User = new UserDto
            {
                Id = user.Id,
                TenantId = user.TenantId,
                TenantName = tenant?.Name ?? "Tenant",
                TenantSlug = tenant?.Slug ?? "tenant",
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            }
        };
    }
}
