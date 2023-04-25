namespace Core.Entities;

public class User : BaseEntity
{
    public string Names { get; set; }
    public string PaternLastName { get; set; }
    public string MotherLastName { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string PhoneNumber { get; set; }
    public ICollection<Rol> Rols { get; set; } = new HashSet<Rol>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new HashSet<RefreshToken>();
    public ICollection<UserRols> UserRols { get; set; }
}
