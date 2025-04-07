namespace GalaxyWiki.Core.Entities
{
    public class CelestialBody
    {
        public virtual Guid CelestialBodyId { get; protected set; }
        public virtual string Name { get; set; } = string.Empty;
        public virtual CelestialBody Orbits { get; set; }
        public virtual BodyType BodyType { get; set; }

        protected CelestialBody() { }

        public CelestialBody(string name, CelestialBody orbits, BodyType bodyType)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Celestial body name cannot be empty", nameof(name));
            
            CelestialBodyId = Guid.NewGuid();
            Name = name;
            Orbits = orbits;
            BodyType = bodyType;
        }
    }
} 