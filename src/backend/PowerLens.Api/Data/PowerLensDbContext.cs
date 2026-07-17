using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PowerLens.Api.Domain;

namespace PowerLens.Api.Data;

public sealed class PowerLensDbContext(DbContextOptions<PowerLensDbContext> options)
    : IdentityDbContext<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Projeto> Projetos => Set<Projeto>();
    public DbSet<Componente> Componentes => Set<Componente>();
    public DbSet<Documentacao> Documentacoes => Set<Documentacao>();
    public DbSet<Relacionamento> Relacionamentos => Set<Relacionamento>();
    public DbSet<Historico> Historicos => Set<Historico>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Empresa>(entity =>
        {
            entity.Property(x => x.Nome).HasMaxLength(200).IsRequired();
            entity.HasMany(x => x.Projetos).WithOne(x => x.Empresa).HasForeignKey(x => x.EmpresaId);
        });

        builder.Entity<Projeto>(entity =>
        {
            entity.Property(x => x.Nome).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DataverseUrl).HasMaxLength(500);
            entity.Property(x => x.DataverseTenantId).HasMaxLength(100);
            entity.Property(x => x.DataverseClientId).HasMaxLength(100);
        });

        builder.Entity<Componente>(entity =>
        {
            entity.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.Nome).HasMaxLength(300).IsRequired();
            entity.Property(x => x.NomeLogico).HasMaxLength(300).IsRequired();
            entity.Property(x => x.MetadataJson).HasColumnType("jsonb");
            entity.HasIndex(x => new { x.ProjetoId, x.Tipo, x.NomeLogico }).IsUnique();
            entity.HasOne(x => x.ParentComponent).WithMany(x => x.Children).HasForeignKey(x => x.ParentComponentId);
            entity.HasOne(x => x.Documentacao).WithOne(x => x.Componente).HasForeignKey<Documentacao>(x => x.ComponenteId);
        });

        builder.Entity<Documentacao>(entity =>
        {
            entity.Property(x => x.ConteudoMarkdown).IsRequired();
            entity.Property(x => x.UltimoAutorId).HasMaxLength(100);
        });

        builder.Entity<Relacionamento>(entity =>
        {
            entity.Property(x => x.TipoRelacionamento).HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.MetadataJson).HasColumnType("jsonb");
            entity.HasIndex(x => new { x.OrigemComponenteId, x.DestinoComponenteId, x.TipoRelacionamento }).IsUnique();
            entity.HasOne(x => x.OrigemComponente).WithMany().HasForeignKey(x => x.OrigemComponenteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DestinoComponente).WithMany().HasForeignKey(x => x.DestinoComponenteId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                SetProperty(entry.Entity, "CreatedAt", now);
                SetProperty(entry.Entity, "UpdatedAt", now);
            }

            if (entry.State == EntityState.Modified)
            {
                SetProperty(entry.Entity, "UpdatedAt", now);
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private static void SetProperty(object entity, string propertyName, DateTimeOffset value)
    {
        var property = entity.GetType().GetProperty(propertyName);
        if (property?.PropertyType == typeof(DateTimeOffset))
        {
            property.SetValue(entity, value);
        }
    }
}
