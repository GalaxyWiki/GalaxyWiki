namespace GalaxyWiki.Core.Entities
{
    public class Roles
    {
        public virtual int Id { get; set; }
        public virtual required string RoleName { get; set; }
        public virtual IList<Users> Users { get; set; } = new List<Users>();
    }
}
