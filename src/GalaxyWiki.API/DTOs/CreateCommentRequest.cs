namespace GalaxyWiki.API.DTOs
{
    public class CreateCommentRequest
    {
        public string CommentText { get; set; }
        public int CelestialBodyId { get; set; }
    }
}