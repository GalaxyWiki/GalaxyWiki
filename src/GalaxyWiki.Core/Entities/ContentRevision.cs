namespace GalaxyWiki.Core.Entities
{
    public class ContentRevision
    {
        public virtual int Id { get; set; }
        public virtual string Content { get; set; }
        public virtual DateTime? CreatedAt { get; set; }
        public virtual CelestialBody CelestialBody { get; set; }
        public virtual User Author { get; set; }
    }
}
