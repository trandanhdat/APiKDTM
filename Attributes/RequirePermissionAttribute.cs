using APi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace APi.Attributes
{
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _permission;

        public RequirePermissionAttribute(string permission) 
        {
            _permission = permission;
        }
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {

            var roleservice = context.HttpContext.RequestServices.GetRequiredService<IRoleService>();

            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            var hasPermission =await roleservice.HasPermissionAsync(userId, _permission);
            if (hasPermission)
            {
                context.Result = new ForbidResult();
            }
            throw new NotImplementedException();
        }
    }
}
