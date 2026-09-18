using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace LoginService;

public class JWTService : IJWTService
{
    public readonly IConfiguration _configuration;

    public JWTService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Generatetoken(User user)
    {
        var claims = new List<Claim>()
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.UserId.ToString()
            ),
            new Claim(
                ClaimTypes.Name,
                user.UserName
            ),
            new Claim(
                ClaimTypes.Email,
                user.Email
            ),
            new Claim(
                ClaimTypes.Role,
                user.Role
            )
        };

        var key = _configuration["Jwt:Key"];

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));

        var credentials = new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
