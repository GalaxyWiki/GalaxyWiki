namespace GalaxyWiki.Core.Entities
{
    public class Role
    {
        public virtual int Id { get; set; }
        public virtual required string RoleName { get; set; }
        public virtual IList<User> Users { get; set; } = new List<User>();
    }
}
