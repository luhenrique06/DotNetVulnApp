using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// A02 - Security Misconfiguration: CORS reflete qualquer origem E permite credenciais.
// (AllowAnyOrigin + AllowCredentials seria rejeitado; refletir a origem burla isso e
//  expõe a API a qualquer site com o cookie/token da vítima.)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// A02/A07 - JWT: validação de assinatura desligada, aceita tokens não assinados
// (alg:none) e chave HMAC hardcoded compartilhada.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        RequireSignedTokens = false,
        ValidateIssuerSigningKey = false,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("fedaf7d8863b48e197b9287d492b708e")),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});

var app = builder.Build();

// A02 - Swagger e página de erro detalhada expostos em qualquer ambiente.
app.UseSwagger();
app.UseSwaggerUI();
app.UseDeveloperExceptionPage();

//app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
