using System;

namespace GalaxyWiki.Core.DTOs
{
    public class CelestialBodyDto
    {
        public int Id { get; set; }
        public string BodyName { get; set; }
        public string BodyType { get; set; }
        public int? Orbits { get; set; }
        public int? ActiveRevision { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 