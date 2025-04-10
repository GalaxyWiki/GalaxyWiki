namespace GalaxyWiki.Models
{
    public class BodyType
    {
        public virtual int BodyTypeId { get; set; }
        public virtual string Type { get; set; }
        public virtual IList<CelestialBody> CelestialBodies { get; set; } = new List<CelestialBody>();
    }
}
