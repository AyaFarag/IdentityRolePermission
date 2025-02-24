using E_learning_Classroom.API.Domain.Entities;
using E_learning_Classroom.API.Extentions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_learning_Classroom.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermissionController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 1. Get All User Permissions
        [HttpGet("user/{email}")]
        public async Task<IActionResult> GetUserPermissions(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound("User not found");

            var claims = await _userManager.GetClaimsAsync(user);
            return Ok(claims.Select(c => new { c.Type, c.Value }));
        }

        // 2. Add Permission to User
        [HttpPost("user/add")]
        public async Task<IActionResult> AddUserPermission([FromBody] UserPermissionDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return NotFound("User not found");

            var result = await _userManager.AddClaimAsync(user, new Claim("Permission", model.Permission));
            if (result.Succeeded)
                return Ok("Permission added");

            return BadRequest(result.Errors);
        }

        // 3. Remove Permission from User
        [HttpPost("user/remove")]
        public async Task<IActionResult> RemoveUserPermission([FromBody] UserPermissionDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return NotFound("User not found");

            var claim = (await _userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == "Permission" && c.Value == model.Permission);
            if (claim == null) return NotFound("Permission not found");

            var result = await _userManager.RemoveClaimAsync(user, claim);
            if (result.Succeeded)
                return Ok("Permission removed");

            return BadRequest(result.Errors);
        }

        // 4. Get Role Permissions
        [HttpGet("role/{roleName}")]
        public async Task<IActionResult> GetRolePermissions(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return NotFound("Role not found");

            var claims = await _roleManager.GetClaimsAsync(role);
            return Ok(claims.Select(c => new { c.Type, c.Value }));
        }

        // 5. Add Permission to Role
        [HttpPost("role/add")]
        public async Task<IActionResult> AddRolePermission([FromBody] RolePermissionDto model)
        {
            var role = await _roleManager.FindByNameAsync(model.Role);
            if (role == null) return NotFound("Role not found");

            var result = await _roleManager.AddClaimAsync(role, new Claim("Permission", model.Permission));
            if (result.Succeeded)
                return Ok("Permission added to role");

            return BadRequest(result.Errors);
        }

        // 6. Remove Permission from Role
        [HttpPost("role/remove")]
        public async Task<IActionResult> RemoveRolePermission([FromBody] RolePermissionDto model)
        {
            var role = await _roleManager.FindByNameAsync(model.Role);
            if (role == null) return NotFound("Role not found");

            var claim = (await _roleManager.GetClaimsAsync(role)).FirstOrDefault(c => c.Type == "Permission" && c.Value == model.Permission);
            if (claim == null) return NotFound("Permission not found");

            var result = await _roleManager.RemoveClaimAsync(role, claim);
            if (result.Succeeded)
                return Ok("Permission removed from role");

            return BadRequest(result.Errors);
        }
    
    }
}
