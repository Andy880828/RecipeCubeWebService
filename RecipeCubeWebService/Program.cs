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
    client.BaseAddress = new Uri("https://localhost:32768"); // API ���|
});

// �T�O Kestrel �j�w��Ҧ� IP �a�}
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // �j�w�Ҧ� IP �a�}�� 5000 �ݤf
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// �]�w Data Protection
builder.Services.AddDataProtection()
    .SetApplicationName("RecipeCubeWebService");

// JWT �{�Ұt�m
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

app.UseHttpsRedirection(); // �ϥ� HTTPS ���s�ɦV
app.UseStaticFiles(); // �ϥ��R�A�ɮ�
app.UseCors("AllowAll"); // �ϥ� CORS ����
app.UseAuthentication(); // �ҥ� JWT ����
app.UseAuthorization(); // �ҥα��v
app.MapControllers(); // �M�g���

app.Run(); // �B�����ε{��

