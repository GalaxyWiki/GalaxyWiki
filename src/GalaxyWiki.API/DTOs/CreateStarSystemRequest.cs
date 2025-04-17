namespace GalaxyWiki.API.DTOs
{
    public class CreateStarSystemRequest
    {
        public string Name { get; set; } = string.Empty;
        public int CenterCbId { get; set; }
    }
}
