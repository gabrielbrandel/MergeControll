using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Merge.Data;
using System.Threading.Tasks;
using MySqlConnector;

public class QueryHelper
{
    public static (string query, List<MySqlParameter> parameters) BuildQueryAndParameters(
        string baseQuery,
        DateTime start, DateTime end,
        double? empresa, double? codigoCliente,
        string? nomeEmpresa, string? cgc,
        double? ticket, string? contato,
        double? codigoCategoria, string? categoria,
        string? observacao, string? type,
        string? produto, string? descricaoModulo,
        string? solucao, string? owner,
        string? version, string? milestone,
        string? descricaoEquipe, string? descricaoServicos,
        string? detalhesAtendimento, string? detalheCliente,
        string? tecnico, string? commit, double? statusAberto, double? statusFechado, string? commitTickets)
    {
        baseQuery += @" WHERE s.`data` >= @startDate AND s.`data` <= @endDate";
        string? whereQuery = null;
        string condicaoOu = " ";
        string formattedCommitTickets = commitTickets != null
            ? string.Join(", ", commitTickets)
            : string.Empty;


        var parameters = new List<MySqlParameter>
        {
            new MySqlParameter("@startDate", start),
            new MySqlParameter("@endDate", end)
        };

        if (!string.IsNullOrEmpty(categoria))
        {
            whereQuery += condicaoOu;
            whereQuery += @" p.descricao REGEXP CONCAT('(?i)',@categoria)";
            parameters.Add(new MySqlParameter("@categoria", categoria));
            condicaoOu = " OR ";
        }

        if (!string.IsNullOrEmpty(nomeEmpresa))
        {
            whereQuery += condicaoOu;
            whereQuery += @" s2.nomeempr REGEXP CONCAT('(?i)', @nomeEmpresa)";
            parameters.Add(new MySqlParameter("@nomeEmpresa", nomeEmpresa));
            condicaoOu = " OR ";
        }

        if (!string.IsNullOrEmpty(cgc))
        {
            whereQuery += condicaoOu;
            whereQuery += @" s2.cgc = TRIM(@cgc)";
            parameters.Add(new MySqlParameter("@cgc", cgc));
            condicaoOu = " OR ";
        }

        if (!string.IsNullOrEmpty(contato))
        {
            whereQuery += condicaoOu;
            whereQuery += @" s.contato REGEXP CONCAT('(?i)', @contato)";
            parameters.Add(new MySqlParameter("@contato", contato));
            condicaoOu = " OR ";
        }

        if (!string.IsNullOrEmpty(observacao))
        {
            whereQuery += condicaoOu;
            whereQuery += @" s.observacao REGEXP CONCAT('(?i)', @observacao)";
            parameters.Add(new MySqlParameter("@observacao", observacao));
            condicaoOu = " OR ";
        }

        if (!string.IsNullOrEmpty(type))
        {
            whereQuery += condicaoOu;
            whereQuery += @" t.type = REGEXP CONCAT('(?i)', @type)";
            parameters.Add(new MySqlParameter("@type", type));
            condicaoOu = " OR ";
        }

        if (!string.IsNullOrEmpty(produto))
        {
            whereQuery += condicaoOu;
            whereQuery += @" t.component REGEXP CONCAT('(?i)',@produto)";
            parameters.Add(new MySqlParameter("@produto", produto));
            condicaoOu = " OR ";
        }

        if (!string.IsNullOrEmpty(descricaoModulo))
        {
            whereQuery += condicaoOu;
            whereQuery += @" m.descricao REGEXP CONCAT('(?i)', @descricaoModulo)";
            parameters.Add(new MySqlParameter("@descricaoModulo", descricaoModulo));
            condicaoOu = " OR ";
        }

        if (!string.IsNullOrEmpty(owner))
        {
            whereQuery += condicaoOu;
            whereQuery += @" t.owner REGEXP CONCAT('(?i)', @owner)";
            parameters.Add(new MySqlParameter("@owner", owner));
            condicaoOu = " OR ";
        }

        if (!string.IsNullOrEmpty(version))
        {
            whereQuery += condicaoOu;
            whereQuery += @" LOWER(t.version) = LOWER(TRIM(@version))";
            parameters.Add(new MySqlParameter("@version", version));
            condicaoOu = " OR ";
        }

        if (!string.IsNullOrEmpty(milestone))
        {
            whereQuery += condicaoOu;
            whereQuery += @" LOWER(t.milestone) = LOWER(TRIM(@milestone))";
            parameters.Add(new MySqlParameter("@milestone", milestone));
            condicaoOu = " OR ";
        }

        if (!string.IsNullOrEmpty(descricaoEquipe))
        {
            whereQuery += condicaoOu;
            whereQuery += @" LOWER(REGEXP_REPLACE(t.milestone, '(Projeto|-|[0-9])', '')) = LOWER(TRIM(@descricaoEquipe))";
            parameters.Add(new MySqlParameter("@descricaoEquipe", descricaoEquipe));
            condicaoOu = " OR ";
        }

        if (!string.IsNullOrEmpty(descricaoServicos))
        {
            whereQuery += condicaoOu;
            whereQuery += @" t.summary REGEXP CONCAT('(?i)',@descricaoServicos)";
            parameters.Add(new MySqlParameter("@descricaoServicos", descricaoServicos));
            condicaoOu = " OR ";
        }

        if (!string.IsNullOrEmpty(detalhesAtendimento))
        {
            whereQuery += condicaoOu;
            whereQuery += $" w.detalhesAtendimento REGEXP CONCAT('(?i)', '{detalhesAtendimento}')";
            parameters.Add(new MySqlParameter("@detalhesAtendimento", detalhesAtendimento));
            condicaoOu = " OR ";
        }

        if (!string.IsNullOrEmpty(detalheCliente))
        {
            whereQuery += condicaoOu;
            whereQuery += @" ws.detalheCliente REGEXP CONCAT('(?i)',@detalheCliente)";
            parameters.Add(new MySqlParameter("@detalheCliente", detalheCliente));
            condicaoOu = " OR ";
        }

        if (!string.IsNullOrEmpty(tecnico))
        {
            whereQuery += condicaoOu;
            whereQuery += @" (SELECT CONCAT(p.tecnico, ' - ', us.nome) FROM protec p
                                INNER JOIN webusuario us ON us.fkIdVendedor = p.tecnico
                                WHERE p.codord = s.codord ORDER BY data DESC LIMIT 1) REGEXP CONCAT('(?i)', @tecnico)";
            parameters.Add(new MySqlParameter("@tecnico", tecnico));
            condicaoOu = " OR ";
        }

        if (!string.IsNullOrEmpty(commit) || !string.IsNullOrEmpty(commitTickets))
        {
            whereQuery += condicaoOu;
            whereQuery += $" s.codord IN ({commitTickets})";
            parameters.Add(new MySqlParameter("@commit", commit));
            condicaoOu = " OR ";
            // Console.WriteLine($"Query detalhe1: {whereQuery}");
        }

        if (!string.IsNullOrEmpty(solucao))
        {

            whereQuery += condicaoOu;
            whereQuery += $" concat(s.descricao4, ' ', s.descricao5) REGEXP CONCAT('(?i)', '{solucao}')";
            parameters.Add(new MySqlParameter("@solucao", solucao));
            condicaoOu = " OR ";
        }

        if (empresa.HasValue)
        {
            whereQuery += condicaoOu;
            whereQuery += @" s2.empresa = @empresa";
            parameters.Add(new MySqlParameter("@empresa", empresa));
            condicaoOu = " OR ";
        }

        if (codigoCliente.HasValue)
        {
            whereQuery += condicaoOu;
            whereQuery += @" c.codcli10 = @codigoCliente";
            parameters.Add(new MySqlParameter("@codigoCliente", codigoCliente));
            condicaoOu = " OR ";
        }

        if (ticket.HasValue)
        {
            whereQuery += condicaoOu;
            whereQuery += @" s.codord = @ticket";
            parameters.Add(new MySqlParameter("@ticket", ticket));
            condicaoOu = " OR ";
        }

        if (codigoCategoria.HasValue)
        {
            whereQuery += condicaoOu;
            whereQuery += @" s.codcat = @codigoCategoria";
            parameters.Add(new MySqlParameter("@codigoCategoria", codigoCategoria));
            condicaoOu = " OR ";
        }

        if (statusFechado.HasValue)
        {
            baseQuery += @" AND s.codsitant = @statusFechado";
            parameters.Add(new MySqlParameter("@statusFechado", statusFechado));
        }

        if (statusAberto.HasValue)
        {
            baseQuery += @" AND s.codsitant <> @statusAberto";
            parameters.Add(new MySqlParameter("@statusAberto", statusAberto));
        }

        if (!string.IsNullOrEmpty(whereQuery)){
            whereQuery = " AND (" + whereQuery + ")";

            baseQuery += whereQuery;
            // Console.WriteLine($"Query detalhe: {baseQuery}");
        }

            return (baseQuery, parameters);
    }
}
// w.detalhesAtendimento REGEXP CONCAT('(?i)', @detalhesAtendimento) OR  s.codord IN ({commitTickets})

// w.detalhesAtendimento REGEXP CONCAT('(?i)', @detalhesAtendimento) OR  s.codord IN ({commitTickets}) OR

