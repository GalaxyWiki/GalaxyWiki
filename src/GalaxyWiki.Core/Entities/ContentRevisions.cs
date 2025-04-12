namespace GalaxyWiki.Core.Entities
{
    public class ContentRevisions
    {
        public virtual int Id { get; set; }
        public virtual string Content { get; set; }
        public virtual DateTime? CreatedAt { get; set; }
        public virtual CelestialBodies CelestialBody { get; set; }
        public virtual Users Author { get; set; }
    }
}
