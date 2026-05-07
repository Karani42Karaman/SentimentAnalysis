using SentimentAnalysis.Domain.Interfaces;

namespace SentimentAnalysis.Infrastructure.Services;

public class FileService : IFileService
{
    public async Task<string> SaveFileAsync(Stream fileStream, string fileName)
    {
        // Dosyaları API projesinin içinde "Uploads" klasörüne kaydedeceğiz
        var uploadsFolder = Path.Combine(Environment.CurrentDirectory, "Uploads");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var filePath = Path.Combine(uploadsFolder, fileName);

        using var fileStreamOnDisk = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fileStreamOnDisk);

        return filePath; // Modelin eğitilebilmesi için bu yolu geriye dönüyoruz
    }
}