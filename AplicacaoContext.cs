using FeatureLogArquivos.Models;
using Microsoft.EntityFrameworkCore;

namespace FeatureLogArquivos
{
    public class AplicacaoContext : DbContext
    {
        public AplicacaoContext(DbContextOptions<AplicacaoContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Propriedade de navegação
            //modelBuilder.Entity<Customer>()
            //    .HasOne(u => u)
            //    .WithMany();

        

            //modelBuilder.BuildIndexesFromAnnotations();
        }
        public DbSet<Historico> Historicos { get; set; }

        public DbSet<Customer>  Customers { get; set; }
        

        //Sobreescrito o metodo saveChanges apenas para setar a data cadastro default para todas entidades que possui esse atriubuto
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries().Where(entry => entry.GetType().GetProperty("DataCadastro") == null))
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("DataCadastro").CurrentValue = Brasilia.DataAtual;
                    entry.Property("DataAtualizacao").CurrentValue = Brasilia.DataAtual;
                }
                if (entry.State == EntityState.Modified)
                {
                    entry.Property("DataCadastro").IsModified = false;
                    entry.Property("DataAtualizacao").CurrentValue = Brasilia.DataAtual;
                }
            }
            return await base.SaveChangesAsync();
        }
    }
}
