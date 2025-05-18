using ContactApplication.Repositories.DataObjects;
using Microsoft.EntityFrameworkCore;


namespace ContactApplication.Repositories
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Contact> Contact { get; set; }
    }
}
