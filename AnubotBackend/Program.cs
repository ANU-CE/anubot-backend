using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenAI;
using OpenAI.Managers;
using System.Reflection;
using System.Text.Json.Serialization;

namespace AnubotBackend;

/// <summary>
/// 아누봇 메인 클래스
/// </summary>
public class Program
{
    /// <summary>
    /// 애플리케이션의 진입점
    /// </summary>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<Context>(options =>
        {
            options.UseMySql(
                connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
                serverVersion: ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")));
        });

        // 벡터 데이터베이스 의존성 등록
        builder.Services.AddSingleton<VectorRepository>();

        // OpenAI 서비스 객체 등록
        builder.Services.AddSingleton(new OpenAIService(
            new OpenAiOptions()
            {
                ApiKey = builder.Configuration["OpenAI:ApiKey"] ?? throw new Exception("OpenAI:ApiKey is not set."),
            }));

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
            });

        // CORS 관련 설정
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
            });
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Anubot Backend API",
            });

            var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            options.RoutePrefix = string.Empty;
        });

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}