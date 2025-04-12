using FluentNHibernate.Mapping;
using GalaxyWiki.Core.Entities;

public class UsersMap : ClassMap<Users>
{
    public UsersMap()
    {
        Table("users");
        Id(x => x.Id).Column("google_sub").GeneratedBy.Assigned(); // string PK
        Map(x => x.Email).Column("email").Not.Nullable().Unique();
        Map(x => x.DisplayName).Column("display_name").Not.Nullable();

        References(x => x.Role).Column("role_id").Nullable();

        // HasMany(x => x.Comments)
        //     .KeyColumn("user_id")
        //     .Inverse()
        //     .Cascade.All();

        // HasMany(x => x.Revisions)
        //     .KeyColumn("author")
        //     .Inverse()
        //     .Cascade.All();
    }
}
