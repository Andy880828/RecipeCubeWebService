using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecipeCubeWebService.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity.UI.Services;
using RecipeCubeWebService.Controllers;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddDbContext<RecipeCubeContext>(options =>
options.UseMySQL(builder.Configuration.GetConnectionString("RecipeCube")));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddScoped<IPasswordHasher<user>, PasswordHasher<user>>();

builder.Services.AddHttpClient<UsersController>(client =>
{
    client.BaseAddress = new Uri("https://localhost:32768"); // API 路徑
});

// 確保 Kestrel 綁定到所有 IP 地址
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // 綁定所有 IP 地址到 5000 端口
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// 設定 Data Protection
builder.Services.AddDataProtection()
    .SetApplicationName("RecipeCubeWebService");

// JWT 認證配置
var key = Encoding.ASCII.GetBytes("thisisaverylongsecretkeyforjwtwhichis256bits!!");
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); // 使用 HTTPS 重新導向
app.UseStaticFiles(); // 使用靜態檔案
app.UseCors("AllowAll"); // 使用 CORS 策略
app.UseAuthentication(); // 啟用 JWT 驗證
app.UseAuthorization(); // 啟用授權
app.MapControllers(); // 映射控制器

app.Run(); // 運行應用程式

