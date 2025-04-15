namespace GalaxyWiki.API.DTOs
{
    public class CommentRequest
    {
        public int CommentId { get; set; }
        public string CommentText { get; set; }
        public string CreatedDate { get; set; }
        public string UserId { get; set; }
        public int CelestialBodyId { get; set; }
    }
} 