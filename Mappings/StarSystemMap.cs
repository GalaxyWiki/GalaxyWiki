using FluentNHibernate.Mapping;
using GalaxyWiki.Models;

namespace GalaxyWiki.Mappings
{
    public class StarSystemMap : ClassMap<StarSystem>
    {
        public StarSystemMap()
        {
            Table("star_systems");

            Id(x => x.SystemId)
                .Column("system_id")
                .GeneratedBy.Guid();

            Map(x => x.Name)
                .Column("name")
                .Not.Nullable();

            References(x => x.CenterCb)
                .Column("center_cb_id")
                .Not.Nullable();
        }
    }
} 