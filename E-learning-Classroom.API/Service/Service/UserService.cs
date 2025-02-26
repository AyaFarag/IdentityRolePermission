using AutoMapper;
using E_learning_Classroom.API.Domain.Entities;
using E_learning_Classroom.API.Extentions;
using E_learning_Classroom.API.Service.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace E_learning_Classroom.API.Service.Service
{
    public class UserService : IUserService
    {
        private readonly ITokenService _tokenService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(ITokenService tokenService,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager, 
            IMapper mapper, ILogger<UserService> logger)
        {
            _tokenService = tokenService;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UserResponse> RegisterAsync(UserRegisterRequest request)
        {
           //1 - ensure the user is not exist before
            
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                
                throw new Exception("Email already exists");
            }
            //------------------------------------------------------------------------



            var newUser = _mapper.Map<ApplicationUser>(request);

            // Generate a unique username
            newUser.UserName = GenerateUserName(request.FirstName, request.LastName);

            var result = await _userManager.CreateAsync(newUser, request.Password); // registered
           
            
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user: {errors}", errors);
                throw new Exception($"Failed to create user: {errors}");
            }



            _logger.LogInformation("User created successfully");
            var response =  _mapper.Map<UserResponse>(newUser);
            var token = await _tokenService.GenerateToken(newUser);

            newUser.CreateAt = DateTime.Now;
            newUser.UpdateAt = DateTime.Now;

            response.AccessToken = token;
            response.CreateAt = newUser.CreateAt;
            response.UpdateAt = newUser.UpdateAt;

            return response;
        }

        private string GenerateUserName(string firstName, string lastName)
        {
            var baseUsername = $"{firstName}{lastName}".ToLower();

            // Check if the username already exists
            var username = baseUsername;
            var count = 1;
            while (_userManager.Users.Any(u => u.UserName == username))
            {
                username = $"{baseUsername}{count}";
                count++;
            }
            return username;
        }

        public async Task<UserResponse> LoginAsync(UserLoginRequest request)
        {
            if (request == null)
            {
                _logger.LogError("Login request is null");
                throw new ArgumentNullException(nameof(request));
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                _logger.LogError("Invalid email or password");
                throw new Exception("Invalid email or password");
            }

            // Generate access token
            var token = await _tokenService.GenerateToken(user); // gernerate token

            // Generate refresh token
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Hash the refresh token and store it in the database or override the existing refresh token
            using var sha256 = SHA256.Create();
            var refreshTokenHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
            user.RefreshToken = Convert.ToBase64String(refreshTokenHash);
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(2);

            user.CreateAt = DateTime.Now;

            // Update user information in database
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to update user: {errors}", errors);
                throw new Exception($"Failed to update user: {errors}");
            }

            var userResponse = _mapper.Map<ApplicationUser, UserResponse>(user);
            userResponse.AccessToken = token;
            userResponse.RefreshToken = refreshToken;

            return userResponse;
        }

        public async Task<UserResponse> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting user by id");
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                _logger.LogError("User not found");
                throw new Exception("User not found");
            }
            _logger.LogInformation("User found");
            return _mapper.Map<UserResponse>(user);
        }
        public string? GetUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId;
        }
        public async Task<CurrentUserResponse> GetCurrentUserAsync()
        {
            var user = await _userManager.FindByIdAsync(GetUserId());
            if (user == null)
            {
                _logger.LogError("User not found");
                throw new Exception("User not found");
            }
            return _mapper.Map<CurrentUserResponse>(user);
        }

        public async Task<CurrentUserResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            _logger.LogInformation("RefreshToken");

            // Hash the incoming RefreshToken and compare it with the one stored in the database
            using var sha256 = SHA256.Create();
            var refreshTokenHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(request.RefreshToken));
            var hashedRefreshToken = Convert.ToBase64String(refreshTokenHash);

            // Find user based on the refresh token
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == hashedRefreshToken);
            if (user == null)
            {
                _logger.LogError("Invalid refresh token");
                throw new Exception("Invalid refresh token");
            }

            // Validate the refresh token expiry time
            if (user.RefreshTokenExpiryTime < DateTime.Now)
            {
                _logger.LogWarning("Refresh token expired for user ID: {UserId}", user.Id);
                throw new Exception("Refresh token expired");
            }

            // Generate a new access token
            var newAccessToken = await _tokenService.GenerateToken(user);
            _logger.LogInformation("Access token generated successfully");
            var currentUserResponse = _mapper.Map<CurrentUserResponse>(user);
            currentUserResponse.AccessToken = newAccessToken;
            return currentUserResponse;
        }

        public async Task<RevokeRefreshTokenResponse> RevokeRefreshToken(RefreshTokenRequest refreshTokenRemoveRequest)
        {
            _logger.LogInformation("Revoking refresh token");

            try
            {
                // Hash the refresh token
                using var sha256 = SHA256.Create();
                var refreshTokenHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshTokenRemoveRequest.RefreshToken));
                var hashedRefreshToken = Convert.ToBase64String(refreshTokenHash);

                // Find the user based on the refresh token
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == hashedRefreshToken);
                if (user == null)
                {
                    _logger.LogError("Invalid refresh token");
                    throw new Exception("Invalid refresh token");
                }

                // Validate the refresh token expiry time
                if (user.RefreshTokenExpiryTime < DateTime.Now)
                {
                    _logger.LogWarning("Refresh token expired for user ID: {UserId}", user.Id);
                    throw new Exception("Refresh token expired");
                }

                // Remove the refresh token
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;

                // Update user information in database
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to update user");
                    return new RevokeRefreshTokenResponse
                    {
                        Message = "Failed to revoke refresh token"
                    };
                }
                _logger.LogInformation("Refresh token revoked successfully");
                return new RevokeRefreshTokenResponse
                {
                    Message = "Refresh token revoked successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to revoke refresh token: {ex}", ex.Message);
                throw new Exception("Failed to revoke refresh token");
            }
        }

        public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                _logger.LogError("User not found");
                throw new Exception("User not found");
            }

            user.UpdateAt = DateTime.Now;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.Gender = request.Gender;

            await _userManager.UpdateAsync(user);
            return _mapper.Map<UserResponse>(user);
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                _logger.LogError("User not found");
                throw new Exception("User not found");
            }
            await _userManager.DeleteAsync(user);
        }

        public async Task<IEnumerable<UserResponse>> GetAllAsync()
        {
            var users = _userManager.Users.ToList();  // repository - Interface DI -- Performance /// DI
            var map = _mapper.Map<IEnumerable<UserResponse>>(users);
            return map;
        }
    }
}
