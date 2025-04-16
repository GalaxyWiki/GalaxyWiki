using FluentNHibernate.Mapping;
using GalaxyWiki.Core.Entities;

public class CommentsMap : ClassMap<Comments>
{
    public CommentsMap()
    {
          Table("comments");
            
            Id(x => x.CommentId).Column("comment_id").GeneratedBy.Sequence("comments_comment_id_seq");

            Map(x => x.CelestialBodyId).Column("celestial_body_id").Not.Nullable();

            References(x => x.Author).Column("user_id").Not.Nullable();

            Map(x => x.CommentText).Column("comment").Not.Nullable().Length(255);
            
            Map(x => x.CreatedAt).Column("created_at").Not.Nullable();
    }
}
