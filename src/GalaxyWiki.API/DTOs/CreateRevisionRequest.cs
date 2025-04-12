namespace GalaxyWiki.API.DTO
{
    public class CreateRevisionRequest
    {
        public required string CelestialBodyPath { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
