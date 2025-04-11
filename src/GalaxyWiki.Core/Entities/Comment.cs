namespace GalaxyWiki.Core.Entities
{
    public class Comment
    {
        public virtual int Id { get; set; }
        public virtual int CelestialBody { get; set; }
        public virtual int User { get; set; }
        public virtual string CommentText { get; set; }
        public virtual DateTime CreatedAt { get; set; }
    }
}
