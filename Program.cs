using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using JwtApi.Data;
using JwtApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// ✅ Claim 타입 자동 매핑 제거 (가장 중요!)
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// 🔐 JWT 설정 가져오기
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey =
    jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey 누락");

// 🔗 MySQL 연결
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DB 연결 문자열 누락"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

// ✅ JWT 인증 설정
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();

                Console.WriteLine($"[🔍 Authorization 헤더] >{authHeader}<");

                if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    var token = authHeader
                        .Substring("Bearer ".Length)
                        .Replace("\n", "")
                        .Replace("\r", "")
                        .Trim()
                        .Trim('"');

                    Console.WriteLine($"[💡 context.Token 세팅 전 원본] >{token}<");

                    context.Token = token;

                    Console.WriteLine($"[🎯 context.Token 최종 세팅] >{context.Token}<");

                    // 수동 디코딩도 시도
                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jwt = handler.ReadJwtToken(token);
                        Console.WriteLine("[🧪 수동 JWT 디코딩 결과]");
                        foreach (var claim in jwt.Claims)
                        {
                            Console.WriteLine($"➡ Claim: {claim.Type} = {claim.Value}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[❌ 수동 디코딩 실패] " + ex.Message);
                    }
                }

                return Task.CompletedTask;
            },

            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"[❌ 인증 실패] {context.Exception}");
                return Task.CompletedTask;
            },
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false, // ⛔ 만료 무시
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
        };
    });

builder.Services.AddAuthorization();

// ✅ Controller + JSON 설정
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// ✅ Swagger 설정
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "JWT API",
            Version = "v1",
            Description = "JWT 기반 인증 시스템입니다.",
        }
    );

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Bearer {token} 형식으로 입력해주세요.",
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                new string[] { }
            },
        }
    );
});

// ✅ 의존성 주입
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<UserService>();

// ✅ 애플리케이션 빌드 및 실행
var app = builder.Build();

Console.WriteLine("[🔐 JWT 검증용 SecretKey] " + secretKey);

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "JWT API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.Use(
    async (context, next) =>
    {
        var auth = context.Request.Headers["Authorization"].ToString();
        Console.WriteLine($"[미들웨어 진입] Authorization: {auth}");

        if (auth.Contains('.') && auth.StartsWith("Bearer "))
        {
            var tokenPart = auth.Split(' ')[1];
            var dotCount = tokenPart.Count(c => c == '.');
            Console.WriteLine($"[검증] 토큰에 포함된 점(.) 개수: {dotCount}");
        }

        await next();
    }
);
app.UseAuthentication(); // 반드시 Authorization보다 먼저!
app.UseAuthorization();

app.MapControllers();

app.Run();
