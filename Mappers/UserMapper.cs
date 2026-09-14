namespace LoginService;

public class UserMapper
{
    public static User ToUser(RegisterRequest request)
    {
        return new User
        {
            UserName = request.UserName,
            Email = request.Email,
        };
    }

    public static UserDto ToUserDto(User user)
    {
        return new UserDto
        {
            UserName = user.UserName,
            Email = user.Email
        };
    } 
}
