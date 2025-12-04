using APi.Models;
using APi.Models.DTO;
using APi.Models.Role;
namespace APi.Services
{
    public interface IRoleService
    {
        Task<UserRoleResponse> AssignRoleAsync(AssignRoleRequest request, int adminUserId);
        Task<List<UserRoleResponse>> BulkAssignRoleAsync(BulkAssignRoleRequest request, int adminUserId);
        Task<List<UserRoleResponse>> GetUsersWithRoleAsync(string role);
        Task<List<UserRoleHistory>> GetRoleHistoryAsync(int userId);
        Task<bool> RemoveUserRoleAsync(int userId, int adminUserId, string reason = null);
        Task<List<string>> GetAvailableRolesAsync();
        Task<UserRoleResponse> GetUserRoleAsync(int userId);
        Task<bool> HasPermissionAsync(int userId, string permission);
        Task<List<UserModel>> GetUsersByRoleAsync(string role);
    }
}
