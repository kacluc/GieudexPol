using GieudexPol.Application.Auth.Commands;
using GieudexPol.Application.Auth.DTOs;
using GieudexPol.Domain.Auth;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GieudexPol.Application.Auth.Services
{
    // Interfejsy, które powinny znajdować się w GieudexPol.Application.Interfaces 
    // lub bezpośrednio w folderze z serwisami autoryzacji.
    public interface IJwtService
    {
        string GenerateToken(string userId, string email, string role);
    }

    public interface IIdentityService
    {
        Task<bool> CheckPasswordAsync(string email, string password);
    }

    public class AuthService : 
        IRequestHandler<RegisterUserCommand, AuthResponse>,
        IRequestHandler<LoginUserCommand, AuthResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IIdentityService _identityService;

        public AuthService(
            IUserRepository userRepository, 
            IJwtService jwtService, 
            IIdentityService identityService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _identityService = identityService;
        }

        public async Task<AuthResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var email = request.RegisterRequest.Email.Trim();
            var displayName = request.RegisterRequest.DisplayName.Trim();
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
            {
                throw new UserAlreadyExistsException(email);
            }

            // Dodano brakujący na liście błędów Guid.NewGuid() - upewnij się, że system to obsługuje
            var user = new User(
                Guid.NewGuid(),
                email,
                request.RegisterRequest.Password,
                displayName: displayName);
            await _userRepository.AddAsync(user);

            var token = _jwtService.GenerateToken(user.Id.ToString(), user.Email, user.Role);

            return new AuthResponse
            {
                Token = token,
                Email = user.Email,
                UserId = user.ApplicationUserId,
                Role = user.Role
            };
        }

        public async Task<AuthResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            // Sprawdzenie hasła przeniesione do zewnętrznej abstrakcji IIdentityService,
            // która w projekcie Infrastructure bezpiecznie używa UserManager/SignInManager
            var isPasswordValid = await _identityService.CheckPasswordAsync(request.LoginRequest.Email, request.LoginRequest.Password);
            
            if (!isPasswordValid)
            {
                throw new InvalidCredentialsException();
            }

            var user = await _userRepository.GetByEmailAsync(request.LoginRequest.Email);
            if (user == null)
            {
                throw new InvalidCredentialsException();
            }

            var token = _jwtService.GenerateToken(user.Id.ToString(), user.Email, user.Role);

            return new AuthResponse
            {
                Token = token,
                Email = user.Email,
                UserId = user.ApplicationUserId,
                Role = user.Role
            };
        }
    }
}
