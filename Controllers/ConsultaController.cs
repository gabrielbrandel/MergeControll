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

                var (sqlQuery, parameters) = QueryHelper.BuildQueryAndParameters(
                    start, end, empresa, codigoCliente, nomeEmpresa, cgc, ticket, contato,
                    codigoCategoria, categoria, observacao, type, produto, descricaoModulo,
                    solucao, owner, version, milestone, descricaoEquipe, descricaoServicos,
                    detalhesAtendimento, detalheCliente, tecnico, commit, statusAberto, statusFechado, null, "");

                var countQuery = baseQuery + sqlQuery;
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
                                                         string? requisito,
                                                         double? empresa2,
                                                         double? codigoCliente2,
                                                         string? nomeEmpresa2,
                                                         string? cgc2,
                                                         double? ticket2,
                                                         string? contato2,
                                                         double? codigoCategoria2,
                                                         string? categoria2,
                                                         string? observacao2,
                                                         string? type2,
                                                         string? produto2,
                                                         string? descricaoModulo2,
                                                         string? solucao2,
                                                         string? owner2,
                                                         string? version2,
                                                         string? milestone2,
                                                         string? descricaoEquipe2,
                                                         string? descricaoServicos2,
                                                         string? detalhesAtendimento2,
                                                         string? detalheCliente2,
                                                         string? tecnico2,
                                                         string? commit2,
                                                         double? statusFechado2,
                                                         double? statusAberto2,
                                                         string? requisito2,
                                                         int pageNumber = 1,
                                                         int pageSize = 5000)

        {
            try
            {
                var start = startDate.HasValue ? startDate.Value.AddMonths(0) : new DateTime(2024, 10, 01);
                var end = (endDate?.AddDays(1)) ?? new DateTime(2024, 10, 31);
                string? commitTickets = null;

                // _logger.LogInformation("Resultado consulta - Tickets encontrados: {filtro1}", filtro1);

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
                                        co.commit,
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
                                       sgsistemas.ticket_custom tm ON tm.ticket = s.codord and name = 'requirements'
                                   Left join
                                         (SELECT tc.ticket, GROUP_CONCAT(CONCAT(newvalue, ' ') SEPARATOR '\n\n') AS commit
                                         FROM
                                              sgsistemas.ticket_change tc
                                         WHERE 1=1
                                          AND field = 'comment'
                                          AND author = 'SGPS'
                                        GROUP BY tc.ticket ) as co on co.ticket = s.codticket";

                baseQuery += @" WHERE s.`data` >= @startDate AND s.`data` <= @endDate";

                var (sqlQuery, parameters) = QueryHelper.BuildQueryAndParameters(
                    start, end, empresa, codigoCliente, nomeEmpresa, cgc, ticket, contato,
                    codigoCategoria, categoria, observacao, type, produto, descricaoModulo,
                    solucao, owner, version, milestone, descricaoEquipe, descricaoServicos,
                    detalhesAtendimento, detalheCliente, tecnico, commit, statusAberto, statusFechado, commitTickets, requisito);

                var (sqlWhere2, parameters2) = QueryHelper.BuildQueryAndParameters(
                    start, end, empresa2, codigoCliente2, nomeEmpresa2, cgc2, ticket2, contato2,
                    codigoCategoria2, categoria2, observacao2, type2, produto2, descricaoModulo2,
                    solucao2, owner2, version2, milestone2, descricaoEquipe2, descricaoServicos2,
                    detalhesAtendimento2, detalheCliente2, tecnico2, commit2, statusAberto2, statusFechado2, null, requisito2);

                if (statusFechado.HasValue)
                {
                    baseQuery += $" AND s.codsitant = {statusFechado}";
                    parameters.Add(new MySqlParameter("@statusFechado", statusFechado));
                }

                if (statusAberto.HasValue)
                {
                    baseQuery += $" AND s.codsitant <> {statusAberto} AND s.usufinal is null";
                    parameters.Add(new MySqlParameter("@statusAberto", statusAberto));
                }

                baseQuery += sqlQuery;
                baseQuery += sqlWhere2;

                _logger.LogInformation("Resultado consulta - TEcnico encontrados: {tecnico2}", tecnico2);
                _logger.LogInformation("Resultado consulta - Sql encontrados: {sqlQuery2}", sqlWhere2);

                baseQuery += $" ORDER BY s.`data` LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize};";

                var result = await _context.ConsultaOrdemServicos
                    .FromSqlRaw(baseQuery, parameters.ToArray())
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