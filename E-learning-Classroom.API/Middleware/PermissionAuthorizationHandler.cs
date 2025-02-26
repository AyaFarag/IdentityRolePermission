using E_learning_Classroom.API.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace E_learning_Classroom.API.Middleware
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermissionAuthorizationHandler(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var user = await _userManager.GetUserAsync(context.User);
            if (user == null)
            {
                return;
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                var identityRole = await _roleManager.FindByNameAsync(role);
                if (identityRole != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(identityRole);
                    if (roleClaims.Any(c => c.Type == "Permission" && c.Value == requirement.Permission))
                    {
                        context.Succeed(requirement);
                        return;
                    }
                }
            }
        }
    }

}
