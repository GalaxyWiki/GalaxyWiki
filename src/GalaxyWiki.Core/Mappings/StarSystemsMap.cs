using FluentNHibernate.Mapping;
using GalaxyWiki.Core.Entities;

public class StarSystemsMap : ClassMap<StarSystems>
{
    public StarSystemsMap()
    {
        Table("star_systems");
        Id(x => x.Id).Column("system_id").GeneratedBy.Identity();
        Map(x => x.Name).Column("name").Not.Nullable();

        References(x => x.CenterCb).Column("center_cb").Not.Nullable();
    }
}
