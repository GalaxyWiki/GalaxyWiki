using System;

namespace GalaxyWiki.Core.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }
        public int CelestialBodyId { get; set; }
        public CelestialBodyDto CelestialBody { get; set; }
        public string CommentText { get; set; }
        public string AuthorDisplayName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 