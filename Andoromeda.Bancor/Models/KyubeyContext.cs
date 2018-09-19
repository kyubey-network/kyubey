using Pomelo.AspNetCore.Extensions.BlobStorage.Models;
using Microsoft.EntityFrameworkCore;

namespace Andoromeda.Bancor.Models
{
    public class KyubeyContext : DbContext, IBlobStorageDbContext
    {
        public KyubeyContext(DbContextOptions opt) : base(opt)
        { }

        public DbSet<Currency> Currencies { get; set; }

        public DbSet<Blob> Blobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.SetupBlobStorage();

            modelBuilder.Entity<Currency>(e =>
            {
                e.HasIndex(x => x.PRI);
            });
        }
    }
}
