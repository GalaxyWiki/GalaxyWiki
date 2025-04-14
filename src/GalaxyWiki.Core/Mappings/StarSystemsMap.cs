using FluentNHibernate.Mapping;
using GalaxyWiki.Core.Entities;

namespace GalaxyWiki.Core.Mappings
{
    public class StarSystemsMap : ClassMap<StarSystems>
    {
        public StarSystemsMap()
        {
            Table("star_systems");
            Id(x => x.Id).Column("system_id").GeneratedBy.Assigned();
            Map(x => x.Name).Column("name").Not.Nullable();

            References(x => x.CenterCb).Column("center_cb").Not.Nullable();
        }
    }
}
