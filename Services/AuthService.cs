using Microsoft.AspNetCore.Identity;

namespace LoginService;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepo;
    private readonly IJWTService _jwtService;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(IAuthRepository authRepo, IPasswordHasher<User> passwordHasher, IJWTService jwtService)
    {
        _authRepo = authRepo;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<UserDto> CreateUser(RegisterRequest request)
    {
        //Validation

        //Check if same email already exists
        var existingUser = await _authRepo.getUserByEmail(request.Email);

        if(existingUser != null)
        {
            throw new ConflictException("Email already registered!");
        }

        //Hash password
        var userToCreate = UserMapper.ToUser(request);
        var passwordHash = _passwordHasher.HashPassword(userToCreate, request.Password);

        userToCreate.PasswordHash = passwordHash;

        //Create User
        var user = await _authRepo.RegisterUser(userToCreate);

        return UserMapper.ToUserDto(user);
    }

    public async Task<string> Login(LoginRequest request)
    {
        //get user for matching email
        var user = await _authRepo.getUserByEmail(request.Email);
        
        // if user not exists return exception
        if(user == null)
        {
            throw new UnauthorizedException("Invalid Email or Password!");
        }

        // else check the retrieved user passwordHash and request passwordhash
        var passwordMatch = _passwordHasher.VerifyHashedPassword(user,user.PasswordHash, request.Password);
        // if failed, return password doesn't match
        if(passwordMatch == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedException("Invalid Email or Password!");
        }

        //else return JWT token

        return _jwtService.Generatetoken(user);
    }
}
