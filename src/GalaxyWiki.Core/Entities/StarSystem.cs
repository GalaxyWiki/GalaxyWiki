using System;

namespace GalaxyWiki.Core.Entities
{
    public class StarSystem
    {
        public virtual Guid SystemId { get; protected set; }
        public virtual string Name { get; set; } = string.Empty;
        public virtual CelestialBody CenterCelestialBody { get; set; }

        protected StarSystem() { }

        public StarSystem(string name, CelestialBody centerCelestialBody)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Star system name cannot be empty", nameof(name));

            SystemId = Guid.NewGuid();
            Name = name;
            CenterCelestialBody = centerCelestialBody;
        }
    }
} 