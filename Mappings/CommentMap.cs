using FluentNHibernate.Mapping;
using GalaxyWiki.Models;
using NHibernate.Type;

namespace GalaxyWiki.Mappings
{
    public class CommentMap : ClassMap<Comment>
    {
        public CommentMap()
        {
            Table("comments");

            Id(x => x.CommentId)
                .Column("comment_id")
                .CustomType<GuidType>()      // ✅ fixed type mapping
                .GeneratedBy.GuidComb();

            References(x => x.CelestialBody).Column("celestial_body_id");
            References(x => x.User).Column("user_id");

            Map(x => x.CommentText).Column("comment");
            Map(x => x.CreatedAt).Column("created_at");
        }
    }
}
