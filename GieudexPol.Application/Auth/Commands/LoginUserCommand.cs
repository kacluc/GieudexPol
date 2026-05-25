using MediatR;
using GieudexPol.Application.Auth.DTOs;

namespace GieudexPol.Application.Auth.Commands
{
    public class LoginUserCommand : IRequest<AuthResponse>
    {
        public LoginRequest LoginRequest { get; set; } = null!;
    }
}
