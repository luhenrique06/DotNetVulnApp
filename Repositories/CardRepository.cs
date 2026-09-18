using Dapper;
using brokenaccesscontrol.Models;

namespace brokenaccesscontrol.Repositories;

public static class CardRepository
{
    public static async Task<IEnumerable<Card>> GetByOwner(string ownerId)
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        string query = "Select id, ownerId, pan, cvv, expiry, cardLimit from cards where ownerId = @ownerId";
        return await conn.QueryAsync<Card>(query, new{ ownerId });
    }

    public static async Task<Card> GetById(int id)
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        string query = "Select id, ownerId, pan, cvv, expiry, cardLimit from cards where id = @id";
        var c = await conn.QueryAsync<Card>(query, new{ id });
        return c.FirstOrDefault();
    }
}
