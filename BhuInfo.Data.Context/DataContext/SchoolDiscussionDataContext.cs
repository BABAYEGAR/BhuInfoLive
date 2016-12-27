using System.Data.Entity;
using BhuInfo.Data.Objects.Entities;

namespace BhuInfo.Data.Context.DataContext
{
    public class SchoolDiscussionDataContext : DbContext
    {
        // Your context has been configured to use a 'NewsCategoryDataContext' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'BhuInfo.Data.Context.DataContext.NewsCategoryDataContext' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'NewsCategoryDataContext' 
        // connection string in the application configuration file.
        public SchoolDiscussionDataContext()
            : base("name=BhuInfo")
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<SchoolDiscussion> SchoolDiscussions { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
           // Database.SetInitializer<SchoolDiscussionDataContext>(null);
           
        }
    }
}