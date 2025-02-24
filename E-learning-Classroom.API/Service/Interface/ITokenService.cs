using E_learning_Classroom.API.Domain.Entities;

namespace E_learning_Classroom.API.Service.Interface
{
    public interface ITokenService
    {
        Task<string> GenerateToken(ApplicationUser user);
        string GenerateRefreshToken();
    }
}
