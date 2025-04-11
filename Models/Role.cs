namespace GalaxyWiki.Models
{
    public class Role
    {
        public virtual int RoleId { get; set; }
        public virtual string RoleName { get; set; }
        public virtual IList<User> Users { get; set; } = new List<User>();
    }
}
