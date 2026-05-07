using FluentValidation;
using MediatR;
using SentimentAnalysis.Application.DTOs;
using SentimentAnalysis.Domain.Common;
using SentimentAnalysis.Domain.Entities;
using SentimentAnalysis.Domain.Interfaces;

namespace SentimentAnalysis.Application.Features.Analysis;

// 1. Query (İstek)
// Dışarıya Result tipinde içinde SentimentResult olan bir paket dönecek
public record AnalyzeTextQuery(AnalyzeRequestDto Dto) : IRequest<Result<SentimentResult>>;

// 2. Validator (Doğrulama Kuralları)
public class AnalyzeTextQueryValidator : AbstractValidator<AnalyzeTextQuery>
{
    public AnalyzeTextQueryValidator()
    {
        RuleFor(x => x.Dto.Text)
            .NotEmpty().WithMessage("Analiz edilecek metin boş olamaz.")
            .MinimumLength(3).WithMessage("Anlamlı bir analiz için metin en az 3 karakter olmalıdır.");
    }
}

// 3. Handler (İşi yapan kısım)
public class AnalyzeTextQueryHandler : IRequestHandler<AnalyzeTextQuery, Result<SentimentResult>>
{
    private readonly IMachineLearningService _mlService;

    public AnalyzeTextQueryHandler(IMachineLearningService mlService)
    {
        _mlService = mlService;
    }

    public async Task<Result<SentimentResult>> Handle(AnalyzeTextQuery request, CancellationToken cancellationToken)
    {
        // Domain'deki sözleşmemizi (Interface) çağırıp analizi yapıyoruz
        // (Tahminleme işlemi RAM'den yapılacağı için genellikle senkrondur)
        var prediction = _mlService.AnalyzeText(request.Dto.Text);

        return await Task.FromResult(Result<SentimentResult>.Success(prediction));
    }
}