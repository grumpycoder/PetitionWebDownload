using System.Data.Entity;
using System.Diagnostics;
using DataWebDownload.Models;

namespace DataWebDownload.Persitence
{
    public class DataContext : DbContext
    {
        public DataContext()
            : base("name=DataContext")
        {
            Database.Log = msg => Debug.WriteLine(msg);
        }

        public static DataContext Create()
        {
            return new DataContext();
        }

        public DbSet<Petition> Petitions { get; set; }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Petition>().ToTable("Petitions");

        }
    }
}
