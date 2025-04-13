namespace GalaxyWiki.API.DTOs
{
    public class CelestialBodyUpdateRequest
    {
        public string BodyName { get; set; } = string.Empty;
        public int? BodyTypeId { get; set; }
        public int? OrbitsId { get; set; }
    }
}