namespace GalaxyWiki.Models
{
    public class User
    {
        public virtual string GoogleSub { get; set; }
        public virtual string Email { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual Role Role { get; set; }
    }
}
