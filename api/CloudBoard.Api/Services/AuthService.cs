        using Microsoft.AspNetCore.Identity;
        using Microsoft.IdentityModel.Tokens;
        using System.IdentityModel.Tokens.Jwt;
        using System.Security.Claims;
        using System.Text;
        using CloudBoard.Api.Models;
        using CloudBoard.Api.Models.DTO;

namespace CloudBoard.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> Register(UserRegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new InvalidOperationException("User with this email already exists");

            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException("Failed to create user");
                // return BadRequest(new { message = "Failed to create user", errors = result.Errors });

            var token = GenerateJwtToken(user);
            return new AuthResponseDto
            {
                Token = token,
                User = new UserDto { Id = user.Id, Email = user.Email!, Name = user.Name, CreatedAt = user.CreatedAt }
            };
        }

        public async Task<AuthResponseDto> Login(UserLoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                throw new UnauthorizedAccessException ("Invalid email or password");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                throw new UnauthorizedAccessException ("Invalid email or password");

            var token = GenerateJwtToken(user);
            return new AuthResponseDto
            {
                Token = token,
                User = new UserDto { Id = user.Id, Email = user.Email!, Name = user.Name, CreatedAt = user.CreatedAt }
            };
        }

        public async Task<UserDto> GetCurrentUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) 
                throw new NullReferenceException();

            return new UserDto { Id = user.Id, Email = user.Email!, Name = user.Name, CreatedAt = user.CreatedAt };
        }


        //TODO: Check if maybe this should be moved but placed here for now
         private string GenerateJwtToken(User user)
        {
            var jwtSecret = _configuration["Jwt:Secret"] ?? "YourSuperSecretKeyForJWTTokenGeneration123!";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "CloudBoardAPI";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "CloudBoardClient";

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}