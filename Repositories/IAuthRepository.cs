namespace LoginService;

public interface IAuthRepository
{
    Task<User?> getUserByEmail(string email);
    Task<User> RegisterUser(User user);
}
