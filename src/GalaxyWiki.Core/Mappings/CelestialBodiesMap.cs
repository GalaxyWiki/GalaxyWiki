using FluentNHibernate.Mapping;
using GalaxyWiki.Core.Entities;

public class CelestialBodiesMap : ClassMap<CelestialBodies>
{
    public CelestialBodiesMap()
    {
        Table("celestial_bodies");
        Id(x => x.Id)
            .Column("celestial_body_id")
            .GeneratedBy.Sequence("hibernate_sequence");
        Map(x => x.BodyName).Column("body_name").Not.Nullable().Unique();
        Map(x => x.ActiveRevision).Column("active_revision").Nullable();

        // Map(x => x.Orbits).Column("orbits").Nullable();
        Map(x => x.BodyType).Column("body_type_id").Not.Nullable();

        References(x => x.Orbits).Column("orbits").Nullable(); // Self-referencing FK
        // References(x => x.BodyType).Column("body_type_id").Not.Nullable();

        // HasMany(x => x.Comments)
        //     .KeyColumn("celestial_body_id")
        //     .Inverse()
        //     .Cascade.All();

        // HasMany(x => x.Revis)
        //     .KeyColumn("celestial_body")
        //     .Inverse()
        //     .Cascade.All();

        // HasOne(x => x.StarSystemCenter)
        //     .PropertyRef(x => x.CenterCb)
        //     .Cascade.All();
    }
}
