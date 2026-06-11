using AutoMapper;
using Itelligent.Application.DTOs.Auth;
using Itelligent.Application.Interfaces;
using Itelligent.Domain.Entities;
using Itelligent.Domain.Enums;
using Itelligent.Domain.Interfaces;

namespace Itelligent.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IJwtProvider jwtProvider,
    IPasswordHasher passwordHasher,
    IMapper mapper)
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existing = await userRepository.GetByUsernameAsync(dto.Username);
        if (existing is not null)
            throw new InvalidOperationException("Username already taken.");

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = passwordHasher.Hash(dto.Password),
            Role = Role.User
        };

        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        var token = jwtProvider.Generate(user);
        return new AuthResponseDto { Token = token, User = mapper.Map<UserDto>(user) };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await userRepository.GetByUsernameAsync(dto.Username)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!passwordHasher.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var token = jwtProvider.Generate(user);
        return new AuthResponseDto { Token = token, User = mapper.Map<UserDto>(user) };
    }

    public async Task<UserDto> GetMeAsync(int userId)
    {
        var user = await userRepository.GetByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        return mapper.Map<UserDto>(user);
    }
}
