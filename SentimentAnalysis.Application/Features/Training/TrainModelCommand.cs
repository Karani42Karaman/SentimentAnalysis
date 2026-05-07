using FluentValidation;
using MediatR;
using SentimentAnalysis.Application.DTOs;
using SentimentAnalysis.Domain.Common;
using SentimentAnalysis.Domain.Exceptions;
using SentimentAnalysis.Domain.Interfaces;

namespace SentimentAnalysis.Application.Features.Training;

// 1. Command (İstek)
public record TrainModelCommand(TrainRequestDto Dto, string CorpusPath) : IRequest<Result<string>>;

// 2. Validator (Doğrulama Kuralları)
public class TrainModelCommandValidator : AbstractValidator<TrainModelCommand>
{
    public TrainModelCommandValidator()
    {
        RuleFor(x => x.Dto.Algorithm)
            .NotEmpty().WithMessage("Algoritma adı boş olamaz.");

        RuleFor(x => x.Dto.NGramRange)
            .GreaterThan(0).WithMessage("N-Gram değeri 0'dan büyük olmalıdır.");

        RuleFor(x => x.CorpusPath)
            .NotEmpty().WithMessage("Corpus (veri seti) dosya yolu bulunamadı.");
    }
}

// 3. Handler (İşi yapan kısım)
public class TrainModelCommandHandler : IRequestHandler<TrainModelCommand, Result<string>>
{
    private readonly IMachineLearningService _mlService;

    public TrainModelCommandHandler(IMachineLearningService mlService)
    {
        _mlService = mlService;
    }

    public async Task<Result<string>> Handle(TrainModelCommand request, CancellationToken cancellationToken)
    {
        // Geçersiz algoritma koruması (Custom Exception'ı burada fırlatıyoruz)
        var validAlgorithms = new[] { "naive_bayes", "svm" };
        if (!validAlgorithms.Contains(request.Dto.Algorithm.ToLower()))
        {
            throw new AlgorithmNotSupportedException(request.Dto.Algorithm);
        }

        // Domain'deki sözleşmemizi (Interface) çağırıyoruz. Alt yapıyı (ML.NET) hiç bilmiyoruz!
        await _mlService.TrainModelAsync(
            request.CorpusPath,
            request.Dto.Algorithm,
            request.Dto.NGramRange,
            request.Dto.MinFrequency);

        return Result<string>.Success("Model başarıyla eğitildi ve kaydedildi.");
    }
}