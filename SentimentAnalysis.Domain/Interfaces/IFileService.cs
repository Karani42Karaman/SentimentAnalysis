
namespace SentimentAnalysis.Domain.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(Stream fileStream, string fileName);
    }
}
