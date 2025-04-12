namespace GalaxyWiki.Core.Entities
{
    public class BodyTypes
    {
        public virtual int Id { get; set; }
        public virtual required string TypeName { get; set; }

        public virtual string? Description { get; set; }

        //   body_type_id SERIAL PRIMARY KEY NOT NULL,
        //   type_name VARCHAR(100) UNIQUE NOT NULL,
        //   description VARCHAR(100)
    }
}
