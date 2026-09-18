using Dapper;
using brokenaccesscontrol.Models;

namespace brokenaccesscontrol.Repositories;

public static class TransferRepository
{
    public static async Task<int> Insert(Transfer t)
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        var query = "insert into transfers(fromAccount,toAccount,amount,status,createdAt,idempotencyKey) " +
                    "values(@FromAccount,@ToAccount,@Amount,@Status,@CreatedAt,@IdempotencyKey); select last_insert_rowid();";
        return await conn.ExecuteScalarAsync<int>(query, t);
    }

    public static async Task<Transfer> GetById(int id)
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        string query = "Select id, fromAccount, toAccount, amount, status, createdAt, idempotencyKey from transfers where id = @id";
        var t = await conn.QueryAsync<Transfer>(query, new{ id });
        return t.FirstOrDefault();
    }

    public static async Task<IEnumerable<Transfer>> GetByAccount(int accountId)
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        string query = "Select id, fromAccount, toAccount, amount, status, createdAt, idempotencyKey from transfers where fromAccount = @accountId or toAccount = @accountId";
        return await conn.QueryAsync<Transfer>(query, new{ accountId });
    }
}
