namespace GalaxyWiki.Models
{
    public class CelestialBody
    {
        public virtual int CelestialBodyId { get; set; }
        public virtual required string Name { get; set; }
        public virtual CelestialBody? Orbits { get; set; }
        public virtual required BodyType BodyType { get; set; }
        public virtual IList<Comment> Comments { get; set; } = new List<Comment>();
        public virtual IList<ContentRevision> ContentRevisions { get; set; } = new List<ContentRevision>();
    }
}
