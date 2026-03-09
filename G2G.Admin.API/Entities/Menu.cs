namespace G2G.Admin.API.Entities;

public class Menu
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Path { get; set; }
    public string? Icon { get; set; }
    public int? ParentId { get; set; }
    public int Sort { get; set; } = 0;
    
    // 导航属性
    public ICollection<RoleMenu> RoleMenus { get; set; } = new List<RoleMenu>();
}
