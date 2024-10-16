using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Merge.Data;

namespace Merge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MergesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MergesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("testa-comunicacao")]
        public IActionResult TestDatabaseConnection()
        {
            try
            {
                if (_context.Database.CanConnect())
                {
                    return Ok("Conexão com o banco de dados bem-sucedida!");
                }
                else
                {
                    return StatusCode(500, "Falha na conexão com o banco de dados.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao conectar ao banco de dados: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSubirVersaoOs(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? new DateTime(2024, 10, 01);
                var end = (endDate?.AddDays(1)) ?? new DateTime(2024, 10, 31);

                string sqlQuery = @"
                       SELECT
                           s.fkIdOrdemServico, s.dataHora, s.versao, s.motivo,
                           SUBSTRING_INDEX(w.nome, ' ', 1) AS NomeUsuario, w.fkIdVendedor, REGEXP_REPLACE(t.milestone, '(Projeto|-|[0-9])', '') AS DescricaoEquipe,
                           t.`type` AS TicketType, t.priority AS TicketPriority, t.owner AS TicketOwner, t.milestone AS TicketMilestone,
                           (
                               SELECT p.descricao
                               FROM sgps.protec t2
                               INNER JOIN catatp p ON p.codigo = t2.codcat
                               WHERE t2.codord = s.fkIdOrdemServico
                               ORDER BY t2.datafim DESC
                               LIMIT 1
                           ) AS Categoria,
                           su.descricao as Status
                       FROM websubirversaoos s
                       INNER JOIN webusuario w ON w.id = s.fkIdUsuario
                       LEFT JOIN sgsistemas.ticket t ON t.id = s.fkIdOrdemServico
                       LEFT JOIN suptec t2 on t2.codord = s.fkIdOrdemServico
                       LEFT JOIN webstatusparasubirversao su ON su.id = s.fkIdWebStatusSubirVersao
                    WHERE s.dataHora >= {0} AND s.dataHora <= {1}
--                       and s.fkIdWebStatusSubirVersao = 1
                       order by
                       case when `t2`.`codcat` = 3 then 'A' when `t2`.`codcat` in (29, 15) then 'B' when `t2`.`codcat` = 27 then 'C' when `t2`.`codcat` = 4 then 'D' when `t2`.`codcat` = 2 then 'E' else 'F' end";

                var result = await _context.SubirVersaoOsResults
                    .FromSqlRaw(sqlQuery, start, end)
                    .AsNoTracking()
                    .ToListAsync();

                var groupedResults = result.GroupBy(r => r.FkIdOrdemServico)
                    .Select(group => new
                    {
                        fkIdOrdemServico = group.Key,
                        dataHora = group.Select(x => x.DataHora).ToList(),
                        versao = group.Select(x => x.Versao).ToList(),
                        motivo = group.Select(x => x.Motivo).ToList(),
                        nomeUsuario = group.Select(x => x.NomeUsuario).ToList(),
                        fkIdVendedor = group.First().FkIdVendedor,
                        descricaoEquipe = group.First().DescricaoEquipe,
                        ticketType = group.Select(x => x.TicketType).ToList(),
                        ticketPriority = group.Select(x => x.TicketPriority).ToList(),
                        ticketOwner = group.Select(x => x.TicketOwner).ToList(),
                        ticketMilestone = group.Select(x => x.TicketMilestone).ToList(),
                        categoria = group.First().Categoria,
                        status = group.Select(x => x.Status).ToList(),
                    }).ToList();


                return Ok(groupedResults);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar a consulta: {ex.Message}");
                return StatusCode(500, $"Erro ao processar a consulta: {ex.Message}");
            }
        }

        [HttpGet("por-equipe")]
        public async Task<IActionResult> ListaMergesPorEquipe(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? new DateTime(2024, 10, 01);
                var end = (endDate?.AddDays(1)) ?? new DateTime(2024, 10, 31);

                string sqlQuery = @"
                                  SELECT
                                      e.descricao,
                                      COUNT(DISTINCT s.fkIdOrdemServico) AS Merges
                                  FROM
                                      webequipe e
                                  INNER JOIN
                                      webusuario w2 ON w2.fkIdEquipePadrao = e.id
                                  INNER JOIN
                                      websubirversaoos s ON w2.id = s.fkIdUsuario
                                  LEFT JOIN
                                      webstatusparasubirversao su ON su.id = s.fkIdWebStatusSubirVersao
                                  WHERE
                                      s.dataHora >= {0}
                                      AND s.dataHora <= {1}
                                  GROUP BY
                                      e.descricao
                                  order by
                                      COUNT(DISTINCT s.fkIdOrdemServico) desc";

                var result = await _context.ListaMergesPorEquipes
                    .FromSqlRaw(sqlQuery, start, end)
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar a consulta: {ex.Message}");
                return StatusCode(500, $"Erro ao processar a consulta: {ex.Message}");
            }
        }

        [HttpGet("por-usuario")]
        public async Task<IActionResult> ListaMergesPorUsuario(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? new DateTime(2024, 10, 01);
                var end = (endDate?.AddDays(1)) ?? new DateTime(2024, 10, 31);

                string sqlQuery = @"
                                  SELECT
                                      e.nome,
                                      COUNT(DISTINCT s.fkIdOrdemServico) AS Merges
                                  FROM
                                      webusuario e
                                  INNER JOIN
                                      websubirversaoos s ON e.id = s.fkIdUsuario
                                  LEFT JOIN
                                      webstatusparasubirversao su ON su.id = s.fkIdWebStatusSubirVersao
                                  WHERE
                                      s.dataHora >= {0}
                                      AND s.dataHora <= {1}
                                      and e.dataInativacao is null

                                  GROUP BY
                                      e.nome
                                  order by
                                      COUNT(DISTINCT s.fkIdOrdemServico) desc limit 10";

                var result = await _context.ListaMergesPorUsuarios
                    .FromSqlRaw(sqlQuery, start, end)
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar a consulta: {ex.Message}");
                return StatusCode(500, $"Erro ao processar a consulta: {ex.Message}");
            }
        }

        [HttpGet("por-categoria")]
        public async Task<IActionResult> ListaMergesPorCategoria(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? new DateTime(2024, 10, 01);
                var end = (endDate?.AddDays(1)) ?? new DateTime(2024, 10, 31);

                string sqlQuery = @"
                                  SELECT
                                      e.descricao,
                                      COUNT(DISTINCT s.fkIdOrdemServico) AS Merges
                                  FROM
                                      catatp e
                                  INNER JOIN
                                    protec p ON e.codigo = p.codcat
                                  INNER JOIN
                                      websubirversaoos s ON p.codord = s.fkIdOrdemServico
                                  LEFT JOIN
                                      webstatusparasubirversao su ON su.id = s.fkIdWebStatusSubirVersao
                                  WHERE s.dataHora >= {0}
                                    AND s.dataHora <= {1}
                                  GROUP BY
                                      e.descricao
                                  order by
                                      COUNT(DISTINCT s.fkIdOrdemServico) desc";

                var result = await _context.ListaMergesPorCategorias
                    .FromSqlRaw(sqlQuery, start, end)
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar a consulta: {ex.Message}");
                return StatusCode(500, $"Erro ao processar a consulta: {ex.Message}");
            }
        }

        [HttpGet("por-versao")]
        public async Task<IActionResult> ListaMergesPorVersao(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? new DateTime(2024, 10, 01);
                var end = (endDate?.AddDays(1)) ?? new DateTime(2024, 10, 31);

                string sqlQuery = @"
                                  SELECT
                                       STR_TO_DATE(s.versao, '%d/%m/%y') as versao,
                                      COUNT(DISTINCT s.fkIdOrdemServico) AS Merges
                                  FROM
                                      websubirversaoos S
                                  WHERE s.dataHora >= {0}
                                    AND s.dataHora <= {1}
                                  GROUP BY
                                      s.versao
                                  order by
                                       STR_TO_DATE(s.versao, '%d/%m/%y') desc ";

                var result = await _context.ListaMergesPorVersoes
                    .FromSqlRaw(sqlQuery, start, end)
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar a consulta: {ex.Message}");
                return StatusCode(500, $"Erro ao processar a consulta: {ex.Message}");
            }
        }

        [HttpGet("por-trafego-mes")]
        public async Task<IActionResult> ListaMergesPorMes(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate.HasValue ? startDate.Value.AddMonths(-1) : new DateTime(2024, 10, 01);
                var end = (endDate?.AddDays(1)) ?? new DateTime(2024, 10, 31);

                string sqlQuery = @"
                                  SELECT
                                      CONCAT(YEAR(s.dataHora), ' ', MONTHNAME(s.dataHora)) AS Mes,
                                      COUNT(DISTINCT s.fkIdOrdemServico) AS Merges,
                                      COUNT(DISTINCT CASE WHEN e.descricao = 'BUG DE IMPACTO' THEN s.fkIdOrdemServico END) AS BugDeImpacto,
                                      COUNT(DISTINCT CASE WHEN e.descricao = 'BUG SEM IMPACTO' THEN s.fkIdOrdemServico END) AS BugSemImpacto,
                                      COUNT(DISTINCT CASE WHEN e.descricao = 'ERRO INTERNO' THEN s.fkIdOrdemServico END) AS ErroInterno,
                                      COUNT(DISTINCT CASE WHEN e.descricao = 'ALTERACAO DO SISTEMA' THEN s.fkIdOrdemServico END) AS Alteracao,
                                      COUNT(DISTINCT CASE WHEN e.descricao not in ('BUG DE IMPACTO','BUG SEM IMPACTO','ERRO INTERNO','ALTERACAO DO SISTEMA') THEN s.fkIdOrdemServico END) AS Outros
                                  FROM
                                      websubirversaoos s
                                  INNER JOIN
                                      protec p ON s.fkIdOrdemServico = p.codord
                                  INNER JOIN
                                      catatp e ON p.codcat = e.codigo
                                  WHERE s.dataHora >= {0}
                                    AND s.dataHora <= {1}
                                     -- YEAR(s.dataHora) = YEAR(CURDATE())
                                  GROUP BY
                                      Mes
                                  ORDER BY
                                      s.dataHora";

                var result = await _context.ListaMergesPorMeses
                    .FromSqlRaw(sqlQuery, start, end)
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar a consulta: {ex.Message}");
                return StatusCode(500, $"Erro ao processar a consulta: {ex.Message}");
            }
        }

        [HttpGet("por-tipo")]
        public async Task<IActionResult> ListaMergesPorTipo(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? new DateTime(2024, 10, 01);
                var end = (endDate?.AddDays(1)) ?? new DateTime(2024, 10, 31);

                string sqlQuery = @"
                                  SELECT
                                      t.type,
                                      COUNT(DISTINCT s.fkIdOrdemServico) AS Merges
                                  FROM
                                      sgsistemas.ticket t
                                  INNER JOIN
                                      websubirversaoos s
                                      ON t.id = s.fkIdOrdemServico
                                  WHERE s.dataHora >= {0}
                                    AND s.dataHora <= {1}
                                  GROUP BY
                                      t.type
                                  ORDER BY
                                      COUNT(DISTINCT s.fkIdOrdemServico) DESC";

                var result = await _context.ListaMergesPorTipos
                    .FromSqlRaw(sqlQuery, start, end)
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar a consulta: {ex.Message}");
                return StatusCode(500, $"Erro ao processar a consulta: {ex.Message}");
            }
        }

        [HttpGet("por-trafego-versao")]
        public async Task<IActionResult> TrafegoMergesPorVersao(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate.HasValue ? startDate.Value.AddMonths(-1) : new DateTime(2024, 10, 01);
                var end = (endDate?.AddDays(1)) ?? new DateTime(2024, 10, 31);

                string sqlQuery = @"
                                SELECT
                                      STR_TO_DATE(s.versao, '%d/%m/%y') AS Mes,
                                      COUNT(DISTINCT s.fkIdOrdemServico) AS Merges,
                                      COUNT(DISTINCT CASE WHEN e.descricao = 'BUG DE IMPACTO' THEN s.fkIdOrdemServico END) AS BugDeImpacto,
                                      COUNT(DISTINCT CASE WHEN e.descricao = 'BUG SEM IMPACTO' THEN s.fkIdOrdemServico END) AS BugSemImpacto,
                                      COUNT(DISTINCT CASE WHEN e.descricao = 'ERRO INTERNO' THEN s.fkIdOrdemServico END) AS ErroInterno,
                                      COUNT(DISTINCT CASE WHEN e.descricao = 'ALTERACAO DO SISTEMA' THEN s.fkIdOrdemServico END) AS Alteracao,
                                      COUNT(DISTINCT CASE WHEN e.descricao not in ('BUG DE IMPACTO','BUG SEM IMPACTO','ERRO INTERNO','ALTERACAO DO SISTEMA') THEN s.fkIdOrdemServico END) AS Outros
                                  FROM
                                      websubirversaoos s
                                  INNER JOIN
                                      protec p ON s.fkIdOrdemServico = p.codord
                                  INNER JOIN
                                      catatp e ON p.codcat = e.codigo
                                 WHERE STR_TO_DATE(s.versao, '%d/%m/%y') >= {0}
                                   AND STR_TO_DATE(s.versao, '%d/%m/%y') <= {1}
                                     -- YEAR(s.dataHora) = YEAR(CURDATE())
                                  GROUP BY
                                      Mes
                                  ORDER BY
                                      STR_TO_DATE(s.versao, '%d/%m/%y')";

                var result = await _context.TrafegoMergesPorVersoes
                    .FromSqlRaw(sqlQuery, start, end)
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar a consulta: {ex.Message}");
                return StatusCode(500, $"Erro ao processar a consulta: {ex.Message}");
            }
        }

    }
}