using APi.Models.DTO;
using APi.Models;
using Microsoft.AspNetCore.Identity.Data;

namespace APi.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequestDTO request);
        Task<AuthResponse> RegisterAsync(RegisterRequestDTO request);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string refreshToken);
        Task<bool> LogoutAsync(int userId);
        string GenerateAccessToken(UserModel user);
        string GenerateRefreshToken();
        Task<bool> ValidateRefreshTokenAsync(string refreshToken);
    }
}
