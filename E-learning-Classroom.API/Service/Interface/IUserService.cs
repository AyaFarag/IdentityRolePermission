using E_learning_Classroom.API.Domain.Entities;
using E_learning_Classroom.API.Extentions;

namespace E_learning_Classroom.API.Service.Interface
{
    public interface IUserService
    {
        Task<UserResponse> RegisterAsync(UserRegisterRequest request);
        Task<UserResponse> LoginAsync(UserLoginRequest request);
        Task<RevokeRefreshTokenResponse> RevokeRefreshToken(RefreshTokenRequest refreshTokenRemoveRequest);
        Task<CurrentUserResponse> RefreshTokenAsync(RefreshTokenRequest request);
        public string? GetUserId();
        Task<CurrentUserResponse> GetCurrentUserAsync();
        Task<UserResponse> GetByIdAsync(Guid id);
        Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<UserResponse>> GetAllAsync();


    }
}
