namespace SentimentAnalysis.Application.DTOs;

public class TrainRequestDto
{
    public string Algorithm { get; set; } = string.Empty;
    public int NGramRange { get; set; } = 1;
    public int MinFrequency { get; set; } = 1;
}