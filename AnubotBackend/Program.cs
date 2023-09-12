using AnubotBackend.Services;
using Microsoft.OpenApi.Models;
using OpenAI;
using OpenAI.Managers;
using System.Reflection;
using System.Text.Json.Serialization;

namespace AnubotBackend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddHealthChecks();

        // 벡터 데이터베이스 서비스 주입
        services.AddSingleton<VectorRepository>();

        services.AddSingleton<BingCustomSearch>();

        // OpenAI 서비스 주입
        services.AddSingleton(new OpenAIService(
            new OpenAiOptions()
            {
                ApiKey = configuration["OpenAI:ApiKey"] ?? throw new Exception("OpenAI:ApiKey is not set."),
            }));

        // JSON 직렬화 설정
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
            });

        // CORS 관련 설정
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
            });
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddSwaggerGen(options =>
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

        // Swagger 설정
        app.UseSwagger();

        // SwaggerUI 설정
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            options.RoutePrefix = string.Empty;
        });

        app.UseHttpsRedirection();

        app.MapControllers();

        app.MapHealthChecks("/healthz");

        app.Run();
    }
}