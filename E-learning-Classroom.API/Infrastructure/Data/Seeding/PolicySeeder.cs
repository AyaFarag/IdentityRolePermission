using E_learning_Classroom.API.Domain.Entities;
using E_learning_Classroom.API.Extentions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace E_learning_Classroom.API.Infrastructure.Data.Seeding
{
    public class PolicySeeder
    {
        public static async Task SeedRolesAndPermissions(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            var roles = new[] { "Admin", "Manager", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Define permissions for roles
            var rolePermissions = new Dictionary<string, List<string>>()
        {
            { "Admin", new List<string> {
                Permissions.User.View, Permissions.User.Create, Permissions.User.Edit, Permissions.User.Delete,
                Permissions.Category.View, Permissions.Category.Create, Permissions.Category.Edit, Permissions.Category.Delete
            }},
            { "Manager", new List<string> {
                Permissions.User.View, Permissions.User.Edit,
                Permissions.Category.View, Permissions.Category.Create, Permissions.Category.Edit
            }},
            { "User", new List<string> {
                Permissions.User.View, Permissions.Category.View
            }}
        };

            foreach (var role in rolePermissions)
            {
                var identityRole = await roleManager.FindByNameAsync(role.Key);
                if (identityRole != null)
                {
                    foreach (var permission in role.Value)
                    {
                        var claimExists = (await roleManager.GetClaimsAsync(identityRole)).Any(c => c.Type == "Permission" && c.Value == permission);
                        if (!claimExists)
                        {
                            await roleManager.AddClaimAsync(identityRole, new Claim("Permission", permission));
                        }
                    }
                }
            }

            // Seed admin user
            var adminUser = await userManager.FindByEmailAsync("admin@example.com");
            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }

}
