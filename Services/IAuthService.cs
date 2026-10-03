namespace LoginService;

public interface IAuthService
{
    Task<UserDto> CreateUser(RegisterRequest request);
    Task<TokenResponse> Login(LoginRequest request);
    Task<List<UserDto>> GetAllUsers();
    Task<TokenResponse> GetRefreshToken(string refreshToken);
    Task LogoutAsync(string token);
}
