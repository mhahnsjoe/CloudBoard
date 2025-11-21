namespace CloudBoard.Api.Services
{
    using CloudBoard.Api.Models;
    using CloudBoard.Api.Models.DTO;

    /// <summary>
    /// Business logic for work item operations.
    /// Separates concerns from controller layer.
    /// </summary>
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(UserRegisterDto dto);
        Task<AuthResponseDto> Login(UserLoginDto dto);
        Task<UserDto> GetCurrentUser(string userId);
    }
}