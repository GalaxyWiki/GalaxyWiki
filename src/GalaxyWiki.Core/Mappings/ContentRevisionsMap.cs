using FluentNHibernate.Mapping;
using GalaxyWiki.Core.Entities;

public class ContentRevisionsMap : ClassMap<ContentRevisions>
{
    public ContentRevisionsMap()
    {
        Table("content_revisions");
        Id(x => x.Id).Column("revision_id").GeneratedBy.Identity();
        Map(x => x.Content).Column("content").CustomSqlType("TEXT").Not.Nullable();
        Map(x => x.CreatedAt).Column("created_at").Default("CURRENT_TIMESTAMP");

        References(x => x.CelestialBody).Column("celestial_body").Nullable();
        References(x => x.Author).Column("author").Nullable();
    }
}
