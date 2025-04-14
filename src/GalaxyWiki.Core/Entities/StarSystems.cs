namespace GalaxyWiki.Core.Entities
{
    public class StarSystems
    {
        public virtual int Id { get; set; }
        public virtual required string Name { get; set; }
        public virtual required CelestialBodies CenterCb { get; set; }
    }
}
