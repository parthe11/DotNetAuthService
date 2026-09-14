namespace LoginService;

public interface IJWTService
{
    string Generatetoken(User user);
}
