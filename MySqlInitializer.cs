using CornerkickWebMvc.Models;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace CornerkickWebMvc
{
  public class MySqlInitializer : IDatabaseInitializer<ApplicationDbContext>
  {
    public void InitializeDatabase(ApplicationDbContext context)
    {
      if (!context.Database.Exists()) {
        // if database did not exist before - create it
        context.Database.Create();
      } else {
        // query to check if MigrationHistory table is present in the database 
        //var migrationHistoryTableExists = ((IObjectContextAdapter)context).ObjectContext.ExecuteStoreQuery<int>("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'ckmysqldb' AND table_name = '__MigrationHistory'");

        // if MigrationHistory table is not there (which is the case first time we run) - create it
        //if (migrationHistoryTableExists.FirstOrDefault() == 0) {
        if (MvcApplication.ckcore.ltUser.Count == 0) {
          context.Database.Delete();
          context.Database.Create();
        }
      }
    }
  }
}