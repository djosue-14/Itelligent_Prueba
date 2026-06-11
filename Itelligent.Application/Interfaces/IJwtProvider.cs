using Itelligent.Domain.Entities;

namespace Itelligent.Application.Interfaces;

public interface IJwtProvider
{
    string Generate(User user);
}
