using System.Transactions;
using Microsoft.EntityFrameworkCore;

namespace LoginService;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    public readonly AuthDbContext _dbContext;

    public RefreshTokenRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }    

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
    {
        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();
    }
    public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
    {
        _dbContext.RefreshTokens.Update(refreshToken);
        await _dbContext.SaveChangesAsync();
    }
    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await _dbContext.RefreshTokens
                        .Include(x => x.User)
                        .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
    }  
    public async Task RotateTokenAsync(RefreshToken oldToken, RefreshToken newToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            _dbContext.RefreshTokens.Update(oldToken);
            _dbContext.RefreshTokens.Add(newToken);

            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    } 
    public async Task RevokeTokenAsync(RefreshToken token)
    {
        token.RevokedAt = DateTime.UtcNow;
        _dbContext.RefreshTokens.Update(token);
        await _dbContext.SaveChangesAsync();
    }
}
