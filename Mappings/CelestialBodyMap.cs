using FluentNHibernate.Mapping;
using GalaxyWiki.Models;
using NHibernate.Type; // ✅ Needed for GuidType

namespace GalaxyWiki.Mappings
{
    public class CelestialBodyMap : ClassMap<CelestialBody>
    {
        public CelestialBodyMap()
        {
            Table("celestial_bodies");

            Id(x => x.CelestialBodyId)
                .Column("celestial_body_id")
                .CustomType<GuidType>()         // ✅ Use C# type, not string
                .GeneratedBy.GuidComb();        // ✅ Proper generator

            Map(x => x.Name);

            References(x => x.Orbits)
                .Column("orbits");

            References(x => x.BodyType)
                .Column("body_type_id");

            HasMany(x => x.Comments)
                .KeyColumn("celestial_body_id")
                .Inverse()
                .Cascade.All();

            HasMany(x => x.ContentRevisions)
                .KeyColumn("celestial_body")
                .Inverse()
                .Cascade.All();
        }
    }
}
