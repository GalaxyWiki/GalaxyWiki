namespace GalaxyWiki.Core.Entities
{
    public class CelestialBodies
    {
        public virtual int Id { get; set; }
        public virtual required string BodyName { get; set; }
        public virtual CelestialBodies? Orbits { get; set; }
        public virtual required int BodyType { get; set; }
        // public virtual IList<Comment> Comments { get; set; } = new List<Comment>();
        public virtual int? ActiveRevision { get; set; }
    }
}
