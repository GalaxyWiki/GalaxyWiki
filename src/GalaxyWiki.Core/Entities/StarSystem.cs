namespace GalaxyWiki.Core.Entities
{
    public class StarSystem
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual CelestialBody CenterCb { get; set; }
    }
}
