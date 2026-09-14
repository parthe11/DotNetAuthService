namespace LoginService;

public interface IAuthService
{
    Task<UserDto> CreateUser(RegisterRequest request);

    Task<string> Login(LoginRequest request);
}
