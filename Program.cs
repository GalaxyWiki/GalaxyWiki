
using System;
using NHibernate;
using GalaxyWiki.Models;
using GalaxyWiki.NHibernate;

class Program
{
    static void Main()
    {
        using (var session = NHibernateHelper.OpenSession())
        using (var tx = session.BeginTransaction())
        {
            // Roles
            var adminRole = new Role { RoleName = "Admin" };
            var userRole = new Role { RoleName = "User" };
            session.Save(adminRole);
            session.Save(userRole);

            // Users
            var user1 = new User
            {
                GoogleSub = Guid.NewGuid().ToString(),
                Email = "admin@galaxywiki.local",
                DisplayName = "Admin User",
                Role = adminRole
            };
            var user2 = new User
            {
                GoogleSub = Guid.NewGuid().ToString(),
                Email = "user@galaxywiki.local",
                DisplayName = "Regular User",
                Role = userRole
            };
            session.Save(user1);
            session.Save(user2);

            // Body Types
            // var starType = new BodyType { Type = "Star" };
            // var planetType = new BodyType { Type = "Planet" };
            // session.Save(starType);
            // session.Save(planetType);

            // Celestial Bodies
            // var sun = new CelestialBody
            // {
            //     CelestialBodyId = Guid.NewGuid(),
            //     Name = "Sun",
            //     BodyType = starType
            // };
            // var earth = new CelestialBody
            // {
            //     CelestialBodyId = Guid.NewGuid(),
            //     Name = "Earth",
            //     BodyType = planetType,
            //     Orbits = sun
            // };
            // session.Save(sun);
            // session.Save(earth);

            // Star System
            // var solarSystem = new StarSystem
            // {
            //     SystemId = Guid.NewGuid(),
            //     Name = "Solar System",
            //     CenterCb = sun
            // };
            // session.Save(solarSystem);

            // Comment
            // var comment = new Comment
            // {
            //     CommentId = Guid.NewGuid(),
            //     CelestialBody = earth,
            //     User = user2,
            //     CommentText = "Earth is amazing!",
            //     CreatedAt = DateTime.UtcNow
            // };
            // session.Save(comment);

            // Content Revision
            // var revision = new ContentRevision
            // {
            //     Content = "The Earth is the third planet from the Sun.",
            //     CreatedAt = DateTime.UtcNow,
            //     CelestialBody = earth,
            //     Author = user1
            // };
            // session.Save(revision);

            // tx.Commit();
        }

        Console.WriteLine("Database seeded successfully.");
    }
}
