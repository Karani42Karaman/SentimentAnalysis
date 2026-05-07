using FluentValidation;
using SentimentAnalysis.Application.Behaviors;
using SentimentAnalysis.Application.DTOs;
using SentimentAnalysis.Domain.Interfaces;
using SentimentAnalysis.Infrastructure.Services;

namespace SentimentAnalysis.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- Application Katmaný Kayýtlarý (CQRS & Validation) ---
            var applicationAssembly = typeof(TrainRequestDto).Assembly;

            builder.Services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(applicationAssembly);

                // Validation Behavior'ý MediatR arasýna sýkýþtýrýyoruz
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            // FluentValidation kurallarýný otomatik bulup ekler
            builder.Services.AddValidatorsFromAssembly(applicationAssembly);

            builder.Services.AddScoped<IMachineLearningService, MachineLearningService>();
            builder.Services.AddScoped<IFileService, FileService>();


            // Add services to the container.

            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
