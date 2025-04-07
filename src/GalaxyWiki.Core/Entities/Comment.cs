using System;

namespace GalaxyWiki.Core.Entities
{
    public class Comment
    {
        public virtual Guid CommentId { get; protected set; }
        public virtual CelestialBody CelestialBody { get; protected set; }
        public virtual User User { get; protected set; }
        public virtual string CommentText { get; set; }
        public virtual DateTime CreatedAt { get; protected set; }

        protected Comment() { }

        public Comment(CelestialBody celestialBody, User user, string commentText)
        {
            if (string.IsNullOrWhiteSpace(commentText))
                throw new ArgumentException("Comment text cannot be empty", nameof(commentText));

            CommentId = Guid.NewGuid();
            CelestialBody = celestialBody;
            User = user;
            CommentText = commentText;
            CreatedAt = DateTime.UtcNow;
        }
    }
} 