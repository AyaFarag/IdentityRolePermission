using E_learning_Classroom.API.Domain.Entities;
using E_learning_Classroom.API.Infrastructure.Data.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace E_learning_Classroom.API.Infrastructure.Context
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            //PolicySeeder.SeedRolesAndPermissions(roleManager, userManager);
            base.OnModelCreating(builder);


        }
    }
}
