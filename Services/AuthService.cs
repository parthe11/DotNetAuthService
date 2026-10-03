using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Common;

namespace LoginService;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepo;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IJWTService _jwtService;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(IAuthRepository authRepo, IRefreshTokenService refreshTokenService,IRefreshTokenRepository refreshTokenRepo,IPasswordHasher<User> passwordHasher, IJWTService jwtService)
    {
        _authRepo = authRepo;
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepo = refreshTokenRepo;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<UserDto> CreateUser(RegisterRequest request)
    {
        //Validation

        //Check if same email already exists
        var existingUser = await _authRepo.GetUserByEmail(request.Email);

        if(existingUser != null)
        {
            throw new ConflictException("Email already registered!");
        }

        //Hash password
        var userToCreate = UserMapper.ToUser(request);
        var passwordHash = _passwordHasher.HashPassword(userToCreate, request.Password);

        userToCreate.PasswordHash = passwordHash;
        userToCreate.Role = Roles.User;

        //Create User
        var user = await _authRepo.RegisterUser(userToCreate);

        return UserMapper.ToUserDto(user);
    }

    public async Task<TokenResponse> Login(LoginRequest request)
    {
        //get user for matching email
        var user = await _authRepo.GetUserByEmail(request.Email);
        
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

        // Generate JWT
        var accessToken = _jwtService.Generatetoken(user);

        // Generate refresh token
        var refreshToken =
            _refreshTokenService.GenerateToken();

        // Hash refresh token
        var refreshTokenHash =
            _refreshTokenService.HashToken(refreshToken);

        // Store hash in database
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.UserId,
            TokenHash = refreshTokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _refreshTokenRepo.AddRefreshTokenAsync(refreshTokenEntity);

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<List<UserDto>> GetAllUsers()
    {
        var allUsers = await _authRepo.GetAllUsers();
        var allUsersResponse = new List<UserDto>();
        
        foreach(User user in allUsers){
            allUsersResponse.Add(UserMapper.ToUserDto(user));
        };

        return allUsersResponse;
    }

    public async Task<TokenResponse> GetRefreshToken(string refreshToken)
    {
        string refreshTokenHash = _refreshTokenService.HashToken(refreshToken);

        var storedRefreshToken = await _refreshTokenRepo.GetByTokenHashAsync(refreshTokenHash);

        if(storedRefreshToken == null || storedRefreshToken.RevokedAt != null)
        {
            throw new UnauthorizedException("Invalid refresh token!");
        }

        if(storedRefreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedException("Refresh token expired!");  
        }

        // generate new access token
        string newAccessToken = _jwtService.Generatetoken(storedRefreshToken.User);

        // post all validations genrate new refreshtoken
        string newRefreshToken = _refreshTokenService.GenerateToken();

        string newRefreshTokenHash = _refreshTokenService.HashToken(newRefreshToken);

        //revoke the current refresh token
        storedRefreshToken.ReplacedByTokenHash =  newRefreshTokenHash;
        storedRefreshToken.RevokedAt = DateTime.UtcNow;

        //add the newly created refresh token
        RefreshToken refTokenToAdd = new RefreshToken
        {
            UserId = storedRefreshToken.UserId,
            TokenHash = newRefreshTokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _refreshTokenRepo.RotateTokenAsync(storedRefreshToken, refTokenToAdd);

        return new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task LogoutAsync(string token)
    {
        var tokenHash = _refreshTokenService.HashToken(token);

        var storedToken = await _refreshTokenRepo.GetByTokenHashAsync(tokenHash);

        if(storedToken == null){ return; }

        if(storedToken.RevokedAt != null){ return ; }

        storedToken.RevokedAt = DateTime.UtcNow;

        await _refreshTokenRepo.RevokeTokenAsync(storedToken);
    }
}
