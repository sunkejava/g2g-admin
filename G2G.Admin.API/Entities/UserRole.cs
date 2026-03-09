namespace G2G.Admin.API.Entities;

public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    
    // 导航属性
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
