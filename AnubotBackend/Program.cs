using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenAI;
using OpenAI.Managers;
using System.Reflection;
using System.Text.Json.Serialization;

namespace AnubotBackend;

/// <summary>
/// �ƴ��� ���� Ŭ����
/// </summary>
public class Program
{
    /// <summary>
    /// ���ø����̼��� ������
    /// </summary>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;
        var configuration = builder.Configuration;

        // �����ͺ��̽� ���� ����
        services.AddDbContext<Context>(options =>
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection is not set.");
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        });

        // ���� �����ͺ��̽� ���� ����
        services.AddSingleton<VectorRepository>();

        // OpenAI ���� ����
        services.AddSingleton(new OpenAIService(
            new OpenAiOptions()
            {
                ApiKey = configuration["OpenAI:ApiKey"] ?? throw new Exception("OpenAI:ApiKey is not set."),
            }));

        // JSON ����ȭ ����
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
            });

        // CORS ���� ����
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

        // Swagger ����
        app.UseSwagger();

        // SwaggerUI ����
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