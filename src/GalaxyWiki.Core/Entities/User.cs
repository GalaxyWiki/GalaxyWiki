namespace GalaxyWiki.Core.Entities
{
    public class User
    {
        public virtual string Id { get; set; } // google_sub
        public virtual string Email { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual Role Role { get; set; }
    }
}
