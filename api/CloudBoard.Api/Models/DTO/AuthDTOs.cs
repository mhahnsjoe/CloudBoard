namespace CloudBoard.Api.Models.DTO
{
    public class UserRegisterDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    public class UserLoginDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = null!;
        public UserDto User { get; set; } = null!;
    }
}