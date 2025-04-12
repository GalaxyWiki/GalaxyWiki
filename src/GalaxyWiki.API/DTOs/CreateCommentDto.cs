namespace GalaxyWiki.Api.DTOs
{
    public class CreateCommentDto
    {
        public string CommentText { get; set; }
        public Guid UserId { get; set; }
        public Guid CelestialBodyId { get; set; }
    }
} 