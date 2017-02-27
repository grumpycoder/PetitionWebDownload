using System.Data.Entity;
using System.Diagnostics;

namespace DataWebDownload
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

        public DbSet<Person> Persons { get; set; }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Person>().ToTable("Petitions");

        }
    }
}
