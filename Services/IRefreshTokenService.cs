namespace LoginService;

public interface IRefreshTokenService
{
    string GenerateToken();
    string HashToken(string token);
}
