using FluentNHibernate.Mapping;
using GalaxyWiki.Core.Entities;

public class CommentsMap : ClassMap<Comments>
{
    public CommentsMap()
    {
        Table("comments");
        Id(x => x.Id).Column("comment_id").GeneratedBy.Identity();
        Map(x => x.CommentText).Column("comment").Not.Nullable();
        Map(x => x.CreatedAt).Column("created_at").Default("CURRENT_TIMESTAMP");

        Map(x => x.CelestialBody).Column("celestial_body_id").Not.Nullable();
        Map(x => x.User).Column("user_id").Nullable();

        // References(x => x.CelestialBody).Column("celestial_body_id").Not.Nullable();
        // References(x => x.User).Column("user_id").Nullable();
    }
}
