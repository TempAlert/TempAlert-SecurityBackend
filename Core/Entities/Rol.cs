namespace Core.Entities;

public class Rol : BaseEntity
{
    public string Name { get; set; }
    public ICollection<User> Users { get; set; } = new HashSet<User>();
    public ICollection<UserRols> UsersRols { get; set; }
}

