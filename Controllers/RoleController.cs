
using APi.Models.Role;
using APi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RoleController> _logger;

        public RoleController(IRoleService roleService, ILogger<RoleController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        [HttpPost("assign")]
        [Authorize(Roles = "SuperAdmin,Admin")] // Chỉ SuperAdmin và Admin
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var adminUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _roleService.AssignRoleAsync(request, adminUserId);

                return Ok(new
                {
                    success = true,
                    message = "Gán role thành công",
                    data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }

        [HttpPost("bulk-assign")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> BulkAssignRole([FromBody] BulkAssignRoleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var adminUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var results = await _roleService.BulkAssignRoleAsync(request, adminUserId);

                return Ok(new
                {
                    success = true,
                    message = $"Gán role cho {results.Count} người dùng thành công",
                    data = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk assigning roles");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }

        [HttpGet("users/{role}")]
        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            try
            {
                var users = await _roleService.GetUsersWithRoleAsync(role);
                return Ok(new
                {
                    success = true,
                    data = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by role {Role}", role);
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }

        [HttpGet("history/{userId}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetRoleHistory(int userId)
        {
            try
            {
                var history = await _roleService.GetRoleHistoryAsync(userId);
                return Ok(new
                {
                    success = true,
                    data = history
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role history for user {UserId}", userId);
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }

        [HttpDelete("remove/{userId}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> RemoveRole(int userId, [FromQuery] string reason = null)
        {
            try
            {
                var adminUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var success = await _roleService.RemoveUserRoleAsync(userId, adminUserId, reason);

                if (success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Xóa role thành công"
                    });
                }

                return BadRequest(new { success = false, message = "Không thể xóa role" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role for user {UserId}", userId);
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }

        [HttpGet("available")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetAvailableRoles()
        {
            try
            {
                var roles = await _roleService.GetAvailableRolesAsync();
                return Ok(new
                {
                    success = true,
                    data = roles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available roles");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserRole(int userId)
        {
            try
            {
                // Chỉ cho phép xem role của chính mình hoặc admin
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserId != userId && !UserRoles.IsAdminRole(currentUserRole))
                {
                    return Forbid("Bạn chỉ có thể xem role của chính mình");
                }

                var userRole = await _roleService.GetUserRoleAsync(userId);
                if (userRole == null)
                {
                    return NotFound(new { success = false, message = "User không tồn tại" });
                }

                return Ok(new
                {
                    success = true,
                    data = userRole
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user role for user {UserId}", userId);
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }

        [HttpGet("check-permission/{permission}")]
        [Authorize]
        public async Task<IActionResult> CheckPermission(string permission)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var hasPermission = await _roleService.HasPermissionAsync(userId, permission);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        userId = userId,
                        permission = permission,
                        hasPermission = hasPermission
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission {Permission}", permission);
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }
    }
}