using FluentNHibernate.Mapping;
using GalaxyWiki.Models;

namespace GalaxyWiki.Mappings
{
    public class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            Table("users");

            Id(x => x.GoogleSub)
                .Column("google_sub");

            Map(x => x.Email);
            Map(x => x.DisplayName).Column("display_name");
            References(x => x.Role).Column("role_id");
        }
    }
}
