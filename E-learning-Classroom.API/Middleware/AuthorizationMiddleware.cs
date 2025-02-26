using E_learning_Classroom.API.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace E_learning_Classroom.API.Middleware
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
        {
            var user = context.User;

            if (user.Identity.IsAuthenticated)
            {
                var appUser = await userManager.GetUserAsync(user);
                if (appUser != null)
                {
                    var roles = await userManager.GetRolesAsync(appUser);
                    var claims = roles.Select(role => new Claim(ClaimTypes.Role, role));

                    var identity = new ClaimsIdentity(claims);
                    user.AddIdentity(identity);
                }
            }

            await _next(context);
        }
    }

}
