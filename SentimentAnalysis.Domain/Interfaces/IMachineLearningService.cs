using SentimentAnalysis.Domain.Entities;

namespace SentimentAnalysis.Domain.Interfaces
{
    public interface IMachineLearningService
    {
        // Modeli eğiten metod
        Task TrainModelAsync(string dataPath, string algorithm, int nGramRange, int minFreq);

        // Gelen metni analiz eden metod
        SentimentResult AnalyzeText(string text);
    }
}
