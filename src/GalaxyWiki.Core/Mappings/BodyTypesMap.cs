using FluentNHibernate.Mapping;
using GalaxyWiki.Core.Entities;
using NHibernate.Mapping;

public class BodyTypesMap : ClassMap<BodyTypes>
{
    public BodyTypesMap()
    {
        Table("body_types");
        Id(x => x.Id).Column("body_type_id").GeneratedBy.Identity();
        Map(x => x.TypeName).Column("type_name").Not.Nullable().Unique();
        Map(x => x.Description).Column("description");
    }
}
