using Dapper;
using brokenaccesscontrol.Models;
using brokenaccesscontrol.Services;

namespace brokenaccesscontrol.Repositories;

public static class UserRepository
{
    public static async Task<UserResponse> Insert(UserRequest userRequest){

        var userResponse = new UserResponse {
                User = new Models.User{
                Id = Guid.NewGuid().ToString(),
                DateInsert = DateTime.UtcNow,
                IsAdmin = userRequest.IsAdmin.HasValue ? userRequest.IsAdmin.Value : false,
                Role = "customer",
                DailyLimit = 1000,
                Login = userRequest.Login,
                Name = userRequest.Name,
                Cpf = userRequest.Cpf,
                Password = UtilService.ReturnMD5(userRequest.Password)
            }
        };

        var conn = SqliteConfigConnection.GetSQLiteConnection();
        var query = "insert into users (id,name,cpf,login,password,role,dailyLimit,isAdmin,dateInsert) " +
                    "values(@id,@name,@cpf,@login,@password,@role,@dailyLimit,@isAdmin,@dateInsert)";
        await conn.ExecuteAsync(query, new{
            id = userResponse.User.Id,
            name = userResponse.User.Name,
            cpf = userResponse.User.Cpf,
            login = userResponse.User.Login,
            password = userResponse.User.Password,
            role = userResponse.User.Role,
            dailyLimit = userResponse.User.DailyLimit,
            isAdmin = userResponse.User.IsAdmin,
            dateInsert = userResponse.User.DateInsert.ToString("yyyy-MM-dd HH:mm:ss")
        });

        return userResponse;
    }

    // A06 - Insecure Design: recovery apenas INATIVA a conta pelo login informado,
    // sem provar identidade -> DoS de conta alheia e prep de takeover.
    // A04 - grava um token de reset PREVISÍVEL.
    public static async Task RecoveryPassword(PasswordRecovery recovery){
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        var token = UtilService.GenerateResetToken(recovery.Login ?? "");
        var query = "update users set inativo=@inativo, resetToken=@token, dateChangePassword=@dateChangePassword where login=@login";
        await conn.ExecuteAsync(query, new{
            inativo = 1,
            token = token,
            dateChangePassword = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            login = recovery.Login
        });
    }

    // A06/A04 - reset aceita token previsível e reativa a conta. Sem checar dono.
    public static async Task<bool> ResetPassword(PasswordReset reset){
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        var query = "update users set password=@password, inativo=0, resetToken=null " +
                    "where login=@login and resetToken=@token";
        var rows = await conn.ExecuteAsync(query, new{
            password = UtilService.ReturnMD5(reset.NewPassword ?? ""),
            login = reset.Login,
            token = reset.Token
        });
        return rows > 0;
    }

    // A01/A09 - lista todos com password hash e cpf, sem auth.
    public static async Task<IEnumerable<User>> GetAllUsers()
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        string query = "Select id, name, cpf, login, password, role, isAdmin, inativo, dateChangePassword from users";
        return await conn.QueryAsync<User>(query);
    }

    public static async Task<User> GetUserById(String id)
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        string query = "Select id, name, cpf, login, password, role, dailyLimit, dateInsert, dateUpdate, isAdmin, inativo, dateChangePassword from users where id = @id";
        var user = await conn.QueryAsync<User>(query, new{ id });
        return user.FirstOrDefault();
    }

    public static async Task<User> GetByLogin(string login)
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        string query = "Select id, name, cpf, login, password, role, dailyLimit, isAdmin, inativo from users where login = @login";
        var user = await conn.QueryAsync<User>(query, new{ login });
        return user.FirstOrDefault();
    }

    public static async Task<bool> Delete(string id){
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        var query = "delete from users where id = @id";
        var rows = await conn.ExecuteAsync(query, new{ id });
        return rows > 0;
    }

    public static async Task<User> Login(LoginRequest login)
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        string query = "Select id, name, cpf, login, password, role, dailyLimit, dateInsert, dateUpdate, isAdmin, inativo, dateChangePassword from users where login = @login and inativo = 0";
        var user = await conn.QueryAsync<User>(query, new{ login = login.Login });
        return user.FirstOrDefault();
    }

    // Fixed: replaced string concatenation with parameterized query to prevent SQL injection.
    public static async Task<User> LoginSQL(LoginRequest login)
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        string query = "Select id, name, login, password, role, isAdmin, inativo from users " +
            "where login = @login and password = @password and inativo = 0";
        var user = await conn.QueryAsync<User>(query, new{
            login = login.Login,
            password = UtilService.ReturnMD5(login.Password ?? "")
        });
        return user.FirstOrDefault();
    }

    // Atualiza perfil fazendo overposting dos campos recebidos (A01).
    public static async Task UpdateProfile(string id, ProfileUpdateRequest req){
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        var query = "update users set " +
                    "name = coalesce(@name, name), " +
                    "cpf = coalesce(@cpf, cpf), " +
                    "role = coalesce(@role, role), " +
                    "dailyLimit = coalesce(@dailyLimit, dailyLimit), " +
                    "isAdmin = coalesce(@isAdmin, isAdmin), " +
                    "dateUpdate = @dateUpdate where id = @id";
        await conn.ExecuteAsync(query, new{
            name = req.Name,
            cpf = req.Cpf,
            role = req.Role,
            dailyLimit = req.DailyLimit,
            isAdmin = req.IsAdmin,
            dateUpdate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            id
        });
    }

    public static async Task<bool> LoginExist(string login)
    {
        var conn = SqliteConfigConnection.GetSQLiteConnection();
        string query = "Select count(*) from users where login = @login";
        var count = await conn.QueryAsync<int>(query, new{ login });
        return count.FirstOrDefault() > 0;
    }
}
