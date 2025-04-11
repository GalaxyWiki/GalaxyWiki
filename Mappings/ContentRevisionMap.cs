using FluentNHibernate.Mapping;
using GalaxyWiki.Models;

namespace GalaxyWiki.Mappings
{
    public class ContentRevisionMap : ClassMap<ContentRevision>
    {
        public ContentRevisionMap()
        {
            Table("content_revisions");

            Id(x => x.RevisionId)
                .Column("revision_id");

            Map(x => x.Content);
            Map(x => x.CreatedAt).Column("created_at");

            References(x => x.CelestialBody)
                .Column("celestial_body"); // ✅ Guid FK is inferred

            References(x => x.Author)
                .Column("author"); // ✅ string FK
        }
    }
}
