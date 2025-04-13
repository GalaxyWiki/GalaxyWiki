namespace GalaxyWiki.Api.DTOs
{
    public class CreateCommentDto
    {
        public string CommentText { get; set; }
        public string UserId { get; set; }
        public int CelestialBodyId { get; set; }
    }
} 