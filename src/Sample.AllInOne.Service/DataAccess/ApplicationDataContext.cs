using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Shared.DataAccess;
using Shared.Hosting.Abstractions;

namespace Sample.AllInOne.Service.DataAccess
{
    public class ApplicationDataContextFactory : MigrationDbContextFactory<ApplicationDataContext>
    {
    }
    public class ApplicationDataContext : SqlServerContextBase
    {
        protected DbSet<ValueEntity> Values { get; set; }

        public ApplicationDataContext(DbContextOptions<ApplicationDataContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ValueEntity>(e =>
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.HasDefaultSchema(DbSchema);

                e.ToTable("ValueEntity", DbSchema);

                e.HasKey(p => p.Id);
                e.Property(p => p.Id)
                    .IsRequired()
                    .UseSqlServerIdentityColumn();

                e.Property(p => p.Created)
                    .IsRequired();

                e.Property(p => p.Value)
                    .IsUnicode()
                    .HasMaxLength(100);
            });
        }


        public Task<ValueEntity> Get(int id)
        {
            return Values.FirstOrDefaultAsync(x => x.Id == id);
        }

        public void AddValue(string value)
        {
            Values.Add(new ValueEntity
            {
                Value = value
            });
        }

        public void DeleteValue(ValueEntity value)
        {
            Values.Remove(value);
        }
    }
}