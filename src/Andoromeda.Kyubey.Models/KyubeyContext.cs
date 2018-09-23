using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Andoromeda.Kyubey.Models
{
    public class KyubeyContext : IdentityDbContext<User, IdentityRole<long>, long>
    {
        public KyubeyContext(DbContextOptions options) : base(options)
        {
        }

        protected KyubeyContext()
        {
        }

        public Task InitializeDatabaseAsync(CancellationToken token)
        {

        }

        public DbSet<Token> Tokens { get; set; }

        public DbSet<Otc> Otcs { get; set; }

        public DbSet<Bancor> Bancors { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Token>(e =>
            {
                e.HasIndex(x => x.Priority);
                e.HasIndex(x => x.Name).ForMySqlIsSpatial();
            });

            builder.Entity<Bancor>(e =>
            {
                e.HasIndex(x => x.Status);
            });

            builder.Entity<Otc>(e =>
            {
                e.HasIndex(x => x.Status);
            });
        }
    }
}
