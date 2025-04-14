namespace GalaxyWiki.API.DTOs
{

    public class UpdateStarSystemRequest
    {
        public string Name { get; set; } = string.Empty;
        public int? CenterCbId { get; set; }
    }
}
