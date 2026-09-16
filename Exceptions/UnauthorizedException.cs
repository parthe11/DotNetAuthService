namespace LoginService;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
        
    }
}
