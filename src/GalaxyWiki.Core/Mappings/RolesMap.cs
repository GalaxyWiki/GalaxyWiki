using FluentNHibernate.Mapping;
using GalaxyWiki.Core.Entities;

public class RolesMap : ClassMap<Roles>
{
    public RolesMap()
    {
        Table("roles");
        Id(x => x.Id).Column("role_id").GeneratedBy.Identity();
        Map(x => x.RoleName).Column("role_name").Not.Nullable();

        // HasMany(x => x.Users)
        //     .KeyColumn("role_id")
        //     .Inverse()
        //     .Cascade.All();
    }
}
