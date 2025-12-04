using APi.Models;
using APi.Models.DTO;
using APi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace APi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService,ILogger<AuthController> logger,IConfiguration configuration)
        {
            _authService = authService;
            _logger = logger;
            _configuration= configuration;
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var result =await _authService.LoginAsync(loginRequest);
                return Ok(new
                {
                    success = true,
                    message = "Đăng nhập thành công",
                    data = result
                });

            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống"
                });
            }
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _authService.RegisterAsync(request);
                return Ok(new
                {
                    success = true,
                    message = "Đăng ký thành công",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register error");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống"
                });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _authService.RefreshTokenAsync(request.RefreshToken);
                return Ok(new
                {
                    success = true,
                    message = "Làm mới token thành công",
                    data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh token error");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống"
                });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _authService.LogoutAsync(userId);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Đăng xuất thành công"
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "Không thể đăng xuất"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout error");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống"
                });
            }
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                
                var result = await _authService.RevokeTokenAsync(request.RefreshToken);
                return Ok(new
                {
                    success = result,
                    message = result ? "Thu hồi token thành công" : "Không thể thu hồi token"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Revoke token error");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống"
                });
            }
        }
        [HttpGet("profile1")]
        [Authorize]
        public IActionResult GetProfile()
        {
            try
            {
                var user = new
                {
                    Id = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
                    Username = User.FindFirst(ClaimTypes.Name)?.Value,
                    Email = User.FindFirst(ClaimTypes.Email)?.Value,
                    Fullname = User.FindFirst("fullname")?.Value,
                    Role = User.FindFirst(ClaimTypes.Role)?.Value
                };

                return Ok(new
                {
                    success = true,
                    data = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get profile error");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống"
                });
            }
        }
        [HttpGet("TestConnect")]
        public IActionResult TestConnect()
        {
            string chuoi = _configuration.GetConnectionString("TestApi");

            using (SqlConnection connection = new SqlConnection(chuoi))
            {
                try
                {
                    connection.Open();
                    return Ok(new { success = true, message = "✅ Kết nối thành công!" });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { success = false, message = "❌ Lỗi kết nối: " + ex.Message });
                }
            }
        }

    }
}
