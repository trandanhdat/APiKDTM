using APi.Models;
using APi.Models.DTO;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace APi.Services
{
    public class AuthService : IAuthService
    {
        private readonly DbLoaiContext _dbLoaiContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(DbLoaiContext dbLoaiContext,IConfiguration configuration,ILogger<AuthService> logger) {
            _dbLoaiContext = dbLoaiContext;
            _configuration = configuration;
            _logger= logger;
        }
        public string GenerateAccessToken(UserModel user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]!));
            var credentials = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

            var claim = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.id.ToString()),
                new Claim(ClaimTypes.Name,user.username!.ToString()),
                new Claim(ClaimTypes.Email,user.email ?? ""),
                new Claim(ClaimTypes.Role,user.role!),
                new Claim("fullname",user.fullname ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),ClaimValueTypes.Integer64)
            };
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims:claim,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JWT:ExpiryMinutes"])),
                signingCredentials : credentials

                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var Randombyte = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(Randombyte);
            return Convert.ToBase64String(Randombyte);

        }

        public async Task<AuthResponse> LoginAsync(LoginRequestDTO request)
        {
            try
            {
                var user = await _dbLoaiContext.userModels
                .FirstOrDefaultAsync(user => user.username == request.Username && user.isActive);

                if (user == null)
                {
                    throw new UnauthorizedAccessException("Tên đăng nhập không tồn tại");
                }
                if (!VerifyPassword(request.Password, user.password!))
                {
                    throw new UnauthorizedAccessException("Mật khẩu không chính xác");
                }

                var accessToken = GenerateAccessToken(user);
                var refreshToken = GenerateRefreshToken();

                var refreshTokenEntity = new RefreshToken
                {
                    Token = accessToken,
                    UserId = user.id,
                    ExpiryDate = DateTime.UtcNow.AddMinutes(7),

                };
                _dbLoaiContext.refreshTokens.Add(refreshTokenEntity);

                return  new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JWT:ExpiryMinutes"])),
                    User = new UserInfo
                    {
                        Id = user.id,
                        Username = user.username!,
                        Email = user.email!,
                        Fullname = user.fullname!,
                        Role = user.role!
                    }
                };
            }catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
                throw;
            }

        }

        public async Task<bool> LogoutAsync(int userId)
        {
            try
            {
                var userTokens = await _dbLoaiContext.refreshTokens
                    .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                    .ToListAsync();

                foreach (var token in userTokens)
                {
                    token.IsRevoked = true;
                }

                await _dbLoaiContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var tokenEntity = await _dbLoaiContext.refreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

                if (tokenEntity == null || tokenEntity.ExpiryDate <= DateTime.UtcNow)
                {
                    throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã hết hạn");
                }

                if (!tokenEntity.User.isActive)
                {
                    throw new UnauthorizedAccessException("Tài khoản đã bị vô hiệu hóa");
                }

                // Generate new tokens
                var newAccessToken = GenerateAccessToken(tokenEntity.User);
                var newRefreshToken = GenerateRefreshToken();

                // Revoke old refresh token
                tokenEntity.IsRevoked = true;

                // Create new refresh token
                var newRefreshTokenEntity = new RefreshToken
                {
                    Token = newRefreshToken,
                    UserId = tokenEntity.UserId,
                    ExpiryDate = DateTime.UtcNow.AddDays(7)
                };

                _dbLoaiContext.refreshTokens.Add(newRefreshTokenEntity);
                await _dbLoaiContext.SaveChangesAsync();

                return new AuthResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiryDate = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JWT:ExpiryMinutes"])),
                    User = new UserInfo
                    {
                        Id = tokenEntity.User.id,
                        Username = tokenEntity.User.username!,
                        Email = tokenEntity.User.email!,
                        Fullname = tokenEntity.User.fullname!,
                        Role = tokenEntity.User.role!
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                throw;
            }
            
        }

        public async  Task<AuthResponse> RegisterAsync(RegisterRequestDTO request)
        {
            try
            {
                // Kiểm tra user đã tồn tại
                var existingUser = await _dbLoaiContext.userModels
                    .AnyAsync(u => u.username == request.Username || u.email == request.Email);

                if (existingUser)
                {
                    throw new InvalidOperationException("Tên đăng nhập hoặc email đã tồn tại");
                }

                // Create new user
                var user = new UserModel
                {
                    username = request.Username,
                    password = HashPassword(request.Password),
                    email = request.Email,
                    phone = request.Phone,
                    fullname = request.Fullname,
                    role = "User",
                    isActive = true,
                    createdAt = DateTime.UtcNow
                };

                _dbLoaiContext.userModels.Add(user);
                await _dbLoaiContext.SaveChangesAsync();

                // Generate tokens
                var accessToken = GenerateAccessToken(user);
                var refreshToken = GenerateRefreshToken();

                // Save refresh token
                var refreshTokenEntity = new RefreshToken
                {
                    Token = refreshToken,
                    UserId = user.id,
                    ExpiryDate = DateTime.UtcNow.AddDays(7)
                };

                _dbLoaiContext.refreshTokens.Add(refreshTokenEntity);
                await _dbLoaiContext.SaveChangesAsync();

                return new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JWT:ExpiryMinutes"])),
                    User = new UserInfo
                    {
                        Id = user.id,
                        Username = user.username,
                        Email = user.email,
                        Fullname = user.fullname,
                        Role = user.role
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user: {Username}", request.Username);
                throw;
            }
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            try
            {
                var tokenEntity = await _dbLoaiContext.refreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                if (tokenEntity != null)
                {
                    tokenEntity.IsRevoked = true;
                    await _dbLoaiContext.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token revocation");
                return false;
            }
        }

        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
        {
            var tokenEntity = await _dbLoaiContext.refreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

            return tokenEntity != null && tokenEntity.ExpiryDate > DateTime.UtcNow;
        }
        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
