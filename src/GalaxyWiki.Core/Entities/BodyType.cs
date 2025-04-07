using GalaxyWiki.Core.Enums;

namespace GalaxyWiki.Core.Entities
{
    public class BodyType
    {
        public virtual int BodyTypeId { get; protected set; }
        public virtual string Type { get; set; }
        
        protected BodyType() { }

        public BodyType(string type)
        {
            Type = type;
        }
    }
} 