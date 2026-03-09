namespace G2G.Admin.API.Entities;

public class RoleMenu
{
    public int RoleId { get; set; }
    public int MenuId { get; set; }
    
    // 导航属性
    public Role Role { get; set; } = null!;
    public Menu Menu { get; set; } = null!;
}
