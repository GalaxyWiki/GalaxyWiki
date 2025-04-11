namespace GalaxyWiki.Application.DTO
{
    public class CreateRevisionRequest
    {
        public string CelestialBodyPath { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
