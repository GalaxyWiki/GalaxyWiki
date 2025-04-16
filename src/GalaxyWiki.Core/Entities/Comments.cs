namespace GalaxyWiki.Core.Entities
{
    public class Comments
    {
         public virtual int CommentId { get; set; }
        public virtual string CommentText { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual string UserId { get; set; }
        public virtual int CelestialBodyId { get; set; }

    }
}
