namespace GalaxyWiki.Models
{
    public class StarSystem
    {
        public virtual Guid SystemId { get; set; }
        public virtual string Name { get; set; }
        public virtual CelestialBody CenterCb { get; set; }
    }
}
