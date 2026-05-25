using MediatR;
using GieudexPol.Application.Auth.DTOs;

namespace GieudexPol.Application.Auth.Commands
{
    public class RegisterUserCommand : IRequest<AuthResponse>
    {
        public RegisterRequest RegisterRequest { get; set; } = null!;
    }
}
