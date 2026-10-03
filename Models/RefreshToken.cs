namespace LoginService;

public class RefreshToken
{
    public int RefreshTokenId {get; set;}
    public int UserId {get; set;}
    public string TokenHash { get; set;}
    public DateTime CreatedAt { get; set;}
    public DateTime ExpiresAt {get; set;}
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public byte[] RowVersion { get; set; }
    public User User { get; set; } = null!;
}
