using FluentNHibernate.Mapping;
using GalaxyWiki.Models;

namespace GalaxyWiki.Mappings
{
    public class RoleMap : ClassMap<Role>
    {
        public RoleMap()
        {
            Table("roles");
            Id(x => x.RoleId).Column("role_id");
            Map(x => x.RoleName).Column("role_name");
            HasMany(x => x.Users).KeyColumn("role_id").Inverse().Cascade.All();
        }
    }
}
