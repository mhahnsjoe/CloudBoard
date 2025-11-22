namespace CloudBoard.Api.Services
{
    using CloudBoard.Api.Models.DTO;
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(UserRegisterDto dto);
        Task<AuthResponseDto> Login(UserLoginDto dto);
        Task<UserDto> GetCurrentUser(string userId);
    }
}