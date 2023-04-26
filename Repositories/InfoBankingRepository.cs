using Dapper;
using brokenaccesscontrol.Models;
using System.Security.Cryptography;
using System.Text;

namespace brokenaccesscontrol.Repositories;

public static class InfoBankingRepository
{

    public static async Task<IEnumerable<InfoBanking>> GetAllInfoBanking()
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        string query = "select id, agencia, conta, cpf, CAST(saldobancario as DOUBLE) as saldobancario from infobanking";
        var lstInfoBanking = await conn.QueryAsync<InfoBanking>(query);
        return lstInfoBanking;
    }
}