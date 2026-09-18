using Dapper;
using brokenaccesscontrol.Models;

namespace brokenaccesscontrol.Repositories;

public static class AccountRepository
{
    public static async Task<Account> GetById(int accountId)
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        string query = "Select accountId, agencia, conta, cpf, ownerId, saldo, status from accounts where accountId = @accountId";
        var acc = await conn.QueryAsync<Account>(query, new{ accountId });
        return acc.FirstOrDefault();
    }

    public static async Task<IEnumerable<Account>> GetByOwner(string ownerId)
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        string query = "Select accountId, agencia, conta, cpf, ownerId, saldo, status from accounts where ownerId = @ownerId";
        return await conn.QueryAsync<Account>(query, new{ ownerId });
    }

    public static async Task<IEnumerable<Account>> GetAll()
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        string query = "Select accountId, agencia, conta, cpf, ownerId, saldo, status from accounts";
        return await conn.QueryAsync<Account>(query);
    }

    public static async Task<decimal> GetBalance(int accountId)
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        var v = await conn.QueryAsync<decimal>("Select saldo from accounts where accountId = @accountId", new{ accountId });
        return v.FirstOrDefault();
    }

    public static async Task SetBalance(int accountId, decimal saldo)
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        await conn.ExecuteAsync("update accounts set saldo = @saldo where accountId = @accountId", new{ saldo, accountId });
    }
}
