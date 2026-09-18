using Microsoft.EntityFrameworkCore;

namespace LoginService;

public class AuthRepository : IAuthRepository
{
    public readonly AuthDbContext _dbContext;

    public AuthRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(user => user.Email == email);
    }

    public async Task<User> RegisterUser(User user)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return user;
    }

    public async Task<List<User>> GetAllUsers()
    {
        var allUsers = await _dbContext.Users.ToListAsync();
        return allUsers;
    }
}
