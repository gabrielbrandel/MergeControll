using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Merge.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Merge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConsultaController> _logger;

        public ConsultaController(ApplicationDbContext context, ILogger<ConsultaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("totalrecords")]
        public async Task<IActionResult> GetTotalRecords(DateTime? startDate,
                                                         DateTime? endDate,
                                                         double? empresa,
                                                         double? codigoCliente,
                                                         string? nomeEmpresa,
                                                         string? cgc,
                                                         double? ticket,
                                                         string? contato,
                                                         double? codigoCategoria,
                                                         string? categoria,
                                                         string? observacao,
                                                         string? type,
                                                         string? produto,
                                                         string? descricaoModulo,
                                                         string? solucao,
                                                         string? owner,
                                                         string? version,
                                                         string? milestone,
                                                         string? descricaoEquipe,
                                                         string? descricaoServicos,
                                                         string? detalhesAtendimento,
                                                         string? detalheCliente,
                                                         string? tecnico,
                                                         string? commit,
                                                         double? statusAberto,
                                                         double? statusFechado)
        {
            try
            {
                var start = startDate.HasValue ? startDate.Value.AddMonths(-1) : new DateTime(2024, 10, 01);
                var end = (endDate?.AddDays(1)) ?? new DateTime(2024, 10, 31);

                var baseQuery =  @"
                                 SELECT COUNT(*)
                                 FROM suptec s
                                 INNER JOIN cadcli c ON c.codcli10 = s.codcli
                                 INNER JOIN sgscli s2 ON s2.cgc = c.cgccpf10
                                 INNER JOIN catatp p ON p.codigo = s.codcat
                                 INNER JOIN webordemservico w ON w.id = s.codord
                                 INNER JOIN modatp m ON m.codigo = s.modulatend
                                 INNER JOIN webclienteservidor ws ON ws.fkIdCliente = c.codcli10
                                 LEFT JOIN sgsistemas.ticket t ON t.id = s.codord";

                var (countQuery, parameters) = QueryHelper.BuildQueryAndParameters(
                    baseQuery, start, end, empresa, codigoCliente, nomeEmpresa, cgc, ticket, contato,
                    codigoCategoria, categoria, observacao, type, produto, descricaoModulo,
                    solucao, owner, version, milestone, descricaoEquipe, descricaoServicos,
                    detalhesAtendimento, detalheCliente, tecnico, commit, statusAberto, statusFechado, null);

                int totalRecords;

                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = countQuery;

                        foreach (var parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }

                        totalRecords = Convert.ToInt32(await command.ExecuteScalarAsync());
                    }
                }

                return Ok(totalRecords);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao processar a consulta: {ex.Message}");
            }
        }


        [HttpGet("sgps")]
        public async Task<IActionResult> ConsultaOrdemServico(DateTime? startDate,
                                                         DateTime? endDate,
                                                         double? empresa,
                                                         double? codigoCliente,
                                                         string? nomeEmpresa,
                                                         string? cgc,
                                                         double? ticket,
                                                         string? contato,
                                                         double? codigoCategoria,
                                                         string? categoria,
                                                         string? observacao,
                                                         string? type,
                                                         string? produto,
                                                         string? descricaoModulo,
                                                         string? solucao,
                                                         string? owner,
                                                         string? version,
                                                         string? milestone,
                                                         string? descricaoEquipe,
                                                         string? descricaoServicos,
                                                         string? detalhesAtendimento,
                                                         string? detalheCliente,
                                                         string? tecnico,
                                                         string? commit,
                                                         double? statusFechado,
                                                         double? statusAberto,
                                                         int pageNumber = 1,
                                                         int pageSize = 1000)

        {
            try
            {
                var start = startDate.HasValue ? startDate.Value.AddMonths(0) : new DateTime(2024, 10, 01);
                var end = (endDate?.AddDays(1)) ?? new DateTime(2024, 10, 31);
                string? commitTickets = null;

                if (!string.IsNullOrEmpty(commit))
                {
                    var commitQuery = @"
                                      SELECT GROUP_CONCAT(tc.ticket SEPARATOR ',') AS Tickets
                                      FROM sgsistemas.ticket_change tc
                                      WHERE tc.newvalue REGEXP CONCAT('(?i)', @commit)
                                      AND field = 'comment'
                                      AND author = 'SGPS'
                                      AND tc.time BETWEEN UNIX_TIMESTAMP(@startDate) * 1000000 AND UNIX_TIMESTAMP(@endDate) * 1000000";

                    commitTickets = await _context.Set<TicketResult>()
                        .FromSqlRaw(commitQuery,
                                    new MySqlParameter("@commit", commit),
                                    new MySqlParameter("@startDate", start),
                                    new MySqlParameter("@endDate", end))
                        .Select(r => r.Tickets)
                        .FirstOrDefaultAsync();
                }

                // _logger.LogInformation("Resultado consulta - Tickets encontrados: {Tickets}", commitTickets);

                string baseQuery = @"
                                   SELECT
                                       s2.empresa,
                                       c.codcli10 as codigoCliente,
                                       s2.nomeempr as nomeEmpresa,
                                       s2.cgc,
                                       s.codord as Ticket,
                                       s.contato,
                                       s.codcat as CodigoCategoria,
                                       p.descricao as Categoria,
                                       s.observacao,
                                       CAST(s.`data` AS DATE) as `data`,
                                       t.`type`,
                                       t.component AS produto,
                                       m.descricao as descricaoModulo,
                                       concat(s.descricao4, ' ', s.descricao5) as Solucao,
                                       t.owner,
                                       t.version,
                                       t.milestone,
                                       REGEXP_REPLACE(t.milestone, '(Projeto|-|[0-9])', '') AS DescricaoEquipe,
                                       t.summary AS descricaoServicos,
                                       w.detalhesAtendimento,
                                       ws.detalheCliente,
                                       (select concat(p.tecnico, ' - ', us.nome) as tecnico
                                        from protec p
                                        inner join webusuario us on us.fkIdVendedor = p.tecnico
                                        where p.codord = s.codord order by data desc limit 1) as tecnico,
                                       ' ' as commit,
                                        s.codsitant as codigoStatus,
                                        (SELECT descricao from sitsup
                                         where codsit = s.codsitant) as status,
                                         tm.value as requisito
                                   FROM
                                       suptec s
                                   INNER JOIN
                                       cadcli c ON c.codcli10 = s.codcli
                                   INNER JOIN
                                       sgscli s2 ON s2.cgc = c.cgccpf10
                                   INNER JOIN
                                       catatp p ON p.codigo = s.codcat
                                   INNER JOIN
                                       webordemservico w ON w.id = s.codord
                                   INNER JOIN
                                       modatp m ON m.codigo = s.modulatend
                                   INNER JOIN
                                       webclienteservidor ws ON ws.fkIdCliente = c.codcli10
                                   LEFT JOIN
                                       sgsistemas.ticket t ON t.id = s.codord
                                   LEFT JOIN
                                       sgsistemas.ticket_custom tm ON tm.ticket = s.codord and name = 'requirements'";

                var (sqlQuery, parameters) = QueryHelper.BuildQueryAndParameters(
                    baseQuery, start, end, empresa, codigoCliente, nomeEmpresa, cgc, ticket, contato,
                    codigoCategoria, categoria, observacao, type, produto, descricaoModulo,
                    solucao, owner, version, milestone, descricaoEquipe, descricaoServicos,
                    detalhesAtendimento, detalheCliente, tecnico, commit, statusAberto, statusFechado, commitTickets);

                sqlQuery += $" ORDER BY s.`data` LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize};";

                // _logger.LogInformation("Resultado consulta - Tickets encontrados: {solucao}", solucao);
                var result = await _context.ConsultaOrdemServicos
                    .FromSqlRaw(sqlQuery, parameters.ToArray())
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(new
                {
                    Data = result,
                    CurrentPage = pageNumber
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar a consulta: {ex.Message}");
                return StatusCode(500, $"Erro ao processar a consulta: {ex.Message}");
            }
        }

    }
}