using System;

namespace GalaxyWiki.Core.Entities
{
    public class ContentRevision
    {
        public virtual int RevisionId { get; protected set; }
        public virtual string Content { get; protected set; }
        public virtual DateTime CreatedAt { get; protected set; }
        public virtual CelestialBody CelestialBody { get; protected set; }
        public virtual User Author { get; protected set; }

        protected ContentRevision() { }

        public ContentRevision(string content, CelestialBody celestialBody, User author)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Content cannot be empty", nameof(content));

            Content = content;
            CreatedAt = DateTime.UtcNow;
            CelestialBody = celestialBody;
            Author = author;
        }
    }
} 