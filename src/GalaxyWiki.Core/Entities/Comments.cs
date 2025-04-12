namespace GalaxyWiki.Core.Entities
{
    public class Comments
    {
        public virtual Guid CommentId { get; set; }
        public virtual Guid CelestialBodyId { get; set; }
        public virtual Guid UserId { get; set; }
        public virtual string CommentText { get; set; }
        public virtual DateTime CreatedAt { get; set; }

    }
}
