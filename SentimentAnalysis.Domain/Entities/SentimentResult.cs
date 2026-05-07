namespace SentimentAnalysis.Domain.Entities
{
    public class SentimentResult
    {
        public string PredicatedLabel { get; set; } = string.Empty;
        // float[] yerine, hangi duygunun yüzde kaç olduğunu tutacak Dictionary yapıyoruz
        public Dictionary<string, float> Probabilities { get; set; }
    }
}
