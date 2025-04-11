namespace GalaxyWiki.Core.Entities
{
    public class CelestialBody
    {
        public virtual int Id { get; set; }
        public virtual required string Name { get; set; }
        public virtual int? Orbits { get; set; }
        public virtual required int BodyType { get; set; }
        // public virtual IList<Comment> Comments { get; set; } = new List<Comment>();
        public virtual int? ActiveRevision { get; set; }
    }
}
