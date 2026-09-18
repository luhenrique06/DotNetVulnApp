using Dapper;
using brokenaccesscontrol.Models;

namespace brokenaccesscontrol.Repositories;

public static class WebhookRepository
{
    public static async Task<int> Insert(string ownerId, string callbackUrl, string? secret)
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        var query = "insert into webhooks(ownerId,callbackUrl,secret) values(@ownerId,@callbackUrl,@secret); select last_insert_rowid();";
        return await conn.ExecuteScalarAsync<int>(query, new{ ownerId, callbackUrl, secret });
    }

    public static async Task<Webhook> GetById(int id)
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        string query = "Select id, ownerId, callbackUrl, secret from webhooks where id = @id";
        var w = await conn.QueryAsync<Webhook>(query, new{ id });
        return w.FirstOrDefault();
    }

    public static async Task<IEnumerable<Webhook>> GetByOwner(string ownerId)
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        string query = "Select id, ownerId, callbackUrl, secret from webhooks where ownerId = @ownerId";
        return await conn.QueryAsync<Webhook>(query, new{ ownerId });
    }
}
