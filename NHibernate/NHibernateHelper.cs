// using FluentNHibernate.Cfg;
// using FluentNHibernate.Cfg.Db;
// using NHibernate;
// using NHibernate.Tool.hbm2ddl;
// using GalaxyWiki.Mappings; // Update with your actual namespace

// namespace GalaxyWiki.NHibernate // Update with your actual namespace.ConnectionString("Host=localhost;Port=5432;Database=testdb;Username=postgres;Password=postgres")

// {
//     public static class NHibernateHelper
//     {
//         private static ISessionFactory _sessionFactory;

//         private static ISessionFactory SessionFactory =>
//             _sessionFactory ??= Fluently.Configure()
//                 .Database(PostgreSQLConfiguration
//                     .Standard
//                     .ConnectionString("Host=localhost;Port=5432;Database=testdb;Username=postgres;Password=postgres")
//                     .ShowSql())
//                 .Mappings(m => m.FluentMappings.AddFromAssemblyOf<PersonMap>())
//                 .ExposeConfiguration(cfg => new SchemaExport(cfg).Create(false, true)) // true = drop/create schema every run
//                 .BuildSessionFactory();

//         public static ISession OpenSession()
//         {
//             return SessionFactory.OpenSession();
//         }
//     }
// }

using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using System;
using System.IO;
using GalaxyWiki.Mappings;

namespace GalaxyWiki.NHibernate
{
    public static class NHibernateHelper
    {
        private static ISessionFactory _sessionFactory;

        private static ISessionFactory SessionFactory =>
            _sessionFactory ??= Fluently.Configure()
                .Database(PostgreSQLConfiguration
                    .Standard
                    .ConnectionString("Host=localhost;Port=5432;Database=testdb;Username=postgres;Password=postgres")
                    .ShowSql())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<UserMap>())
                .ExposeConfiguration(cfg =>
                {
                    try
                    {
                        var schemaExport = new SchemaExport(cfg)
                            .SetOutputFile("schema.sql");

                        Console.WriteLine("🚀 Starting schema export...");
                        schemaExport.Create(useStdOut: true, execute: true);

                        Console.WriteLine("✅ Schema export completed. SQL written to schema.sql.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("❌ Schema export failed:");
                        Console.WriteLine(ex.Message);
                        File.WriteAllText("schema-error.txt", ex.ToString());
                        throw;
                    }
                })
                .BuildSessionFactory();

        public static ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }
    }
}
