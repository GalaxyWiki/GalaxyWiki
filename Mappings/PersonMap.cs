using FluentNHibernate.Mapping;
using GalaxyWiki.Models;

namespace GalaxyWiki.Mappings
{
    public class PersonMap : ClassMap<Person>
    {
        public PersonMap()
        {
            Table("People");
            Id(x => x.Id);
            Map(x => x.Name);
        }
    }
}
