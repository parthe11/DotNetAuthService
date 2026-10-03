namespace LoginService;

public interface IRefreshTokenRepository
{
    Task AddRefreshTokenAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash); 
    Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
    Task RotateTokenAsync(RefreshToken oldToken, RefreshToken newToken);
    Task RevokeTokenAsync(RefreshToken token);
}
