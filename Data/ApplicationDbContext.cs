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
        public DbSet<ConsultaOrdemServico> ConsultaOrdemServicos { get; set; }
        public DbSet<TotalCount> TotalCounts { get; set; }
        public DbSet<TicketResult> TicketResults { get; set; }

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
            modelBuilder.Entity<ConsultaOrdemServico>().HasNoKey();
            modelBuilder.Entity<TotalCount>().HasNoKey();
            modelBuilder.Entity<TicketResult>().HasNoKey();
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

    public class ConsultaOrdemServico
    {
        public double? Empresa { get; set; }
        public double? CodigoCliente { get; set; }
        public string? NomeEmpresa { get; set; }
        public string? Cgc { get; set; }
        public double? Ticket { get; set; }
        public string? Contato { get; set; }
        public double? CodigoCategoria { get; set; }
        public string? Categoria { get; set; }
        public string? Observacao { get; set; }
        public DateTime? Data { get; set; }
        public string? Type { get; set; }
        public string? Produto { get; set; }
        public string? DescricaoModulo { get; set; }
        public string? Solucao { get; set; }
        public string? Owner { get; set; }
        public string? Version { get; set; }
        public string? Milestone { get; set; }
        public string? DescricaoEquipe { get; set; }
        public string? DescricaoServicos { get; set; }
        public string? DetalhesAtendimento { get; set; }
        public string? DetalheCliente { get; set; }
        public string? Tecnico { get; set; }
        public string? Commit { get; set; }
        public string? Status { get; set; }
        public double? CodigoStatus { get; set; }
        public string? Requisito { get; set; }
    }

    public class TotalCount
    {
        public int TotalRecords { get; set; }
    }

    public class TicketResult
    {
        public string Tickets { get; set; }
    }

}
