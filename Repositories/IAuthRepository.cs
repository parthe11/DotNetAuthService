namespace LoginService;

public interface IAuthRepository
{
    Task<User?> GetUserByEmail(string email);
    Task<User> RegisterUser(User user);
    Task<List<User>> GetAllUsers();
}
