namespace GalaxyWiki.Api.DTOs
{
    public class CommentDto
    {
        public Guid CommentId { get; set; }
        public string CommentText { get; set; }
        public string CreatedDate { get; set; }
        public Guid UserId { get; set; }
        public Guid CelestialBodyId { get; set; }
    }
} 