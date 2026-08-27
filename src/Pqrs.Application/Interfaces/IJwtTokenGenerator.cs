using Pqrs.Domain.Entities;

namespace Pqrs.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
