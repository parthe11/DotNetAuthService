using System.ComponentModel.DataAnnotations;

namespace LoginService;

public class User
{
    public int UserId {get; set;}
    [Required]
    public string UserName {get; set;}
    [Required]
    [EmailAddress]
    public string Email {get; set;}
    public string PasswordHash {get; set;}
}
