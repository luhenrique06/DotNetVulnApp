using Dapper;
using brokenaccesscontrol.Models;

namespace brokenaccesscontrol.Repositories;

public static class CouponRepository
{
    public static async Task<Coupon> Get(string code)
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        var c = await conn.QueryAsync<Coupon>("Select code, valuePct, usesLeft from coupons where code = @code", new{ code });
        return c.FirstOrDefault();
    }

    // A06 - decremento NÃO atômico (read-modify-write separado do apply) -> reuso em corrida.
    public static async Task DecrementUses(string code)
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        await conn.ExecuteAsync("update coupons set usesLeft = usesLeft - 1 where code = @code", new{ code });
    }
}
