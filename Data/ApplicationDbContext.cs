using Microsoft.EntityFrameworkCore;

namespace Merge.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public ApplicationDbContext() { }

        public DbSet<ListaMergesPorTipo> ListaMergesPorTipos { get; set; }
        public DbSet<ListaMergesPorMes> ListaMergesPorMeses { get; set; }
        public DbSet<ListaMergesPorVersao> ListaMergesPorVersoes { get; set; }
        public DbSet<ListaMergesPorEquipe> ListaMergesPorEquipes { get; set; }
        public DbSet<ListaMergesPorUsuario> ListaMergesPorUsuarios { get; set; }
        public DbSet<ListaMergesPorCategoria> ListaMergesPorCategorias { get; set; }
        public DbSet<SubirVersaoOsResult> SubirVersaoOsResults { get; set; }
        public DbSet<TrafegoMergesPorVersao> TrafegoMergesPorVersoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TrafegoMergesPorVersao>().HasNoKey();
            modelBuilder.Entity<ListaMergesPorTipo>().HasNoKey();
            modelBuilder.Entity<ListaMergesPorMes>().HasNoKey();
            modelBuilder.Entity<ListaMergesPorVersao>().HasNoKey();
            modelBuilder.Entity<ListaMergesPorCategoria>().HasNoKey();
            modelBuilder.Entity<ListaMergesPorUsuario>().HasNoKey();
            modelBuilder.Entity<ListaMergesPorEquipe>().HasNoKey();
            modelBuilder.Entity<SubirVersaoOsResult>().HasNoKey();
        }
    }

    public class SubirVersaoOsResult
    {
        public int FkIdOrdemServico { get; set; }
        public DateTime DataHora { get; set; }
        public string? Versao { get; set; }
        public string? Motivo { get; set; }
        public string? NomeUsuario { get; set; }
        public double? FkIdVendedor { get; set; }
        public string? DescricaoEquipe { get; set; }
        public string? TicketType { get; set; }
        public string? TicketPriority { get; set; }
        public string? TicketOwner { get; set; }
        public string? TicketMilestone { get; set; }
        public string? Categoria { get; set; }
        public string? Status { get; set; }
    }
    public class ListaMergesPorEquipe
    {
        public string? Descricao { get; set; }
        public int? Merges { get; set; }
    }

    public class ListaMergesPorUsuario
    {
        public string? Nome { get; set; }
        public int? Merges { get; set; }
    }
    public class ListaMergesPorCategoria
    {
        public string? Descricao { get; set; }
        public int? Merges { get; set; }
    }
    public class ListaMergesPorVersao
    {
        public DateTime? versao { get; set; }
        public int? Merges { get; set; }
    }
    public class ListaMergesPorMes
    {
        public string? Mes { get; set; }
        public int? Merges { get; set; }
        public int? BugDeImpacto { get; set; }
        public int? BugSemImpacto { get; set; }
        public int? ErroInterno { get; set; }
        public int? Alteracao { get; set; }
        public int? Outros { get; set; }
    }
    public class ListaMergesPorTipo
    {
        public string? Type { get; set; }
        public int? Merges { get; set; }
    }

    public class TrafegoMergesPorVersao
    {
        public DateTime? Mes { get; set; }
        public int? Merges { get; set; }
        public int? BugDeImpacto { get; set; }
        public int? BugSemImpacto { get; set; }
        public int? ErroInterno { get; set; }
        public int? Alteracao { get; set; }
        public int? Outros { get; set; }
    }
}
