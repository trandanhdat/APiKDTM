using APi.Models;
using APi.Models.Role;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace APi.Services
{
    public class RoleService : IRoleService
    {
        private readonly DbLoaiContext _dbLoaiContext;
        private readonly ILogger<RoleService> _logger;
        private readonly IMemoryCache _cache;

        public RoleService(DbLoaiContext dbLoaiContext,ILogger<RoleService> logger,IMemoryCache cache) 
        {
            _dbLoaiContext = dbLoaiContext;
            _logger = logger;
            _cache = cache;
        }
        public async Task<UserRoleResponse> AssignRoleAsync(AssignRoleRequest request, int adminUserId)
        {
            
            try
            {
                // Validate role
                if (!UserRoles.IsValidRole(request.Role!))
                {
                    throw new ArgumentException($"Role '{request.Role}' không hợp lệ");
                }

                // Tìm user
                var user =await _dbLoaiContext.userModels.FindAsync(request.UserId);
                if (user == null)
                {
                    throw new ArgumentException("User không tồn tại");
                }

                // Tìm admin user
                var adminUser = await _dbLoaiContext.userModels.FindAsync(adminUserId);
                if (adminUser == null)
                {
                    throw new UnauthorizedAccessException("Admin user không tồn tại");
                }

                // Kiểm tra quyền: chỉ SuperAdmin mới được gán SuperAdmin role
                if (request.Role == UserRoles.SuperAdmin && adminUser.role != UserRoles.SuperAdmin)
                {
                    throw new UnauthorizedAccessException("Chỉ SuperAdmin mới có thể gán role SuperAdmin");
                }

                // Kiểm tra quyền: Admin không thể thay đổi role của SuperAdmin
                if (user.role == UserRoles.SuperAdmin && adminUser.role != UserRoles.SuperAdmin)
                {
                    throw new UnauthorizedAccessException("Không thể thay đổi role của SuperAdmin");
                }

                var previousRole = user.role;

                // Lưu lịch sử thay đổi role
                var roleHistory = new UserRoleHistory
                {
                    UserId = user.id,
                    PreviousRole = previousRole,
                    NewRole = request.Role,
                    Reason = request.Reason ?? "Role assignment",
                    ChangedByUserId = adminUserId,
                    ChangedAt = DateTime.UtcNow
                };

                _dbLoaiContext.userRoleHistories.Add(roleHistory);

                // Cập nhật role cho user
                user.role = request.Role;

                await _dbLoaiContext.SaveChangesAsync();

                _logger.LogInformation($"Role changed for user {user.username} from {previousRole} to {request.Role} by {adminUser.username}");

                return new UserRoleResponse
                {
                    UserId = user.id,
                    Username = user.username,
                    Email = user.email,
                    CurrentRole = user.role,
                    PreviousRole = previousRole,
                    RoleChangedAt = roleHistory.ChangedAt,
                    ChangedBy = adminUser.username,
                    Reason = request.Reason
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role to user {UserId}", request.UserId);
                throw;
            }

        }

        public async Task<List<UserRoleResponse>> BulkAssignRoleAsync(BulkAssignRoleRequest request, int adminUserId)
        {
            var results = new List<UserRoleResponse>();

            foreach (var userId in request.UserId)
            {
                try
                {
                    var assignRequest = new AssignRoleRequest
                    {
                        UserId = userId,
                        Role = request.Role,
                        Reason = request.Reason
                    };

                    var result = await AssignRoleAsync(assignRequest, adminUserId);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error assigning role to user {UserId} in bulk operation", userId);
                    // Continue with other users
                }
            }

            return results;
        }

        public async Task<List<string>> GetAvailableRolesAsync()
        {
            return await Task.FromResult(UserRoles.AllRoles.ToList());
        }

        public async Task<List<UserRoleHistory>> GetRoleHistoryAsync(int userId)
        {
            try
            {
                var history = await _dbLoaiContext.userRoleHistories
                    .Include(h => h.ChangedByUser)
                    .Where(h => h.UserId == userId)
                    .OrderByDescending(h => h.ChangedAt)
                    .ToListAsync();

                return history;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role history for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> RemoveRoleAsync(int userId, int adminUserId, string reason = null)
        {
            try
            {
                var assignRequest = new AssignRoleRequest
                {
                    UserId = userId,
                    Role = UserRoles.Guest, // Đặt về Guest role
                    Reason = reason ?? "Role removed"
                };

                await AssignRoleAsync(assignRequest, adminUserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role for user {UserId}", userId);
                return false;
            }
        }

        public async Task<UserRoleResponse> GetUserRoleAsync(int userId)
        {
            try
            {
                var user =await  _dbLoaiContext.userModels.FindAsync(userId);
                if (user == null) return null;

                return new UserRoleResponse
                {
                    UserId = user.id,
                    Username = user.username,
                    Email = user.email,
                    CurrentRole = user.role
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user role for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<UserModel>> GetUsersByRoleAsync(string role)
        {
            try
            {
                return await  _dbLoaiContext.userModels
                    .Where(u => u.role == role && u.isActive)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by role {Role}", role);
                throw;
            }
        }

        public async Task<List<UserRoleResponse>> GetUsersWithRoleAsync(string role)
        {
            try
            {
                var users =await  _dbLoaiContext.userModels
                    .Where(u => u.role == role && u.isActive)
                    .Select(u => new UserRoleResponse
                    {
                        UserId = u.id,
                        Username = u.username,
                        Email = u.email,
                        CurrentRole = u.role
                    })
                    .ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users with role {Role}", role);
                throw;
            }
        }

        public async Task<bool> HasPermissionAsync(int userId, string permission)
        {
            try
            {
                var user = await _dbLoaiContext.userModels.FindAsync(userId);
                if (user == null || !user.isActive) return false;

                // Định nghĩa permissions theo role
                var rolePermissions = new Dictionary<string, List<string>>
                {
                    [UserRoles.SuperAdmin] = new List<string> { "all" },
                    [UserRoles.Admin] = new List<string> { "manage_users", "manage_products", "view_reports", "manage_orders" },
                    [UserRoles.Manager] = new List<string> { "manage_products", "view_reports", "manage_orders" },
                    [UserRoles.User] = new List<string> { "view_products", "manage_own_orders" },
                    [UserRoles.Guest] = new List<string> { "view_products" }
                };

                if (rolePermissions.ContainsKey(user.role))
                {
                    var permissions = rolePermissions[user.role];
                    return permissions.Contains("all") || permissions.Contains(permission);
                }

                return await Task.FromResult( false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", permission, userId);
                return false;
            }
        }

        public async Task<bool> RemoveUserRoleAsync(int userId, int adminUserId, string reason = null)
        {
            try
            {
                var assignRequest = new AssignRoleRequest
                {
                    UserId = userId,
                    Role = UserRoles.Guest, // Đặt về Guest role
                    Reason = reason ?? "Role removed"
                };

                await AssignRoleAsync(assignRequest, adminUserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role for user {UserId}", userId);
                return false;
            }
        }
    }
}
