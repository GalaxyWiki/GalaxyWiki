using FluentNHibernate.Mapping;
using GalaxyWiki.Models;

namespace GalaxyWiki.Mappings
{
    public class BodyTypeMap : ClassMap<BodyType>
    {
        public BodyTypeMap()
        {
            Table("body_types");
            Id(x => x.BodyTypeId).Column("body_type_id");
            Map(x => x.Type);
            HasMany(x => x.CelestialBodies).KeyColumn("body_type_id").Inverse().Cascade.All();
        }
    }
}
