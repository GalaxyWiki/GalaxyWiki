namespace GalaxyWiki.Core.Entities
{
    public class StarSystems
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual CelestialBodies CenterCb { get; set; }
    }
}
