namespace GalaxyWiki.Models
{
    public class Comment
    {
        public virtual Guid CommentId { get; set; }
        public virtual CelestialBody CelestialBody { get; set; }
        public virtual User User { get; set; }
        public virtual string CommentText { get; set; }
        public virtual DateTime CreatedAt { get; set; }
    }
}
