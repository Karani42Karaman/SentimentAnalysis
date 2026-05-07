namespace SentimentAnalysis.Domain.Exceptions
{
    public class AlgorithmNotSupportedException : Exception
    {

        public string AttemptedAlgorithm { get; set; }
        public IEnumerable<string> SupportedAlgorithms { get; set; } = Enumerable.Empty<string>();
        public AlgorithmNotSupportedException(string attemptedAlgorithm) :
            base($"'{attemptedAlgorithm}' algoritması şu anda desteklenmiyor. Lütfen desteklenen algoritmalardan birini seçin.")
        {
            AttemptedAlgorithm = attemptedAlgorithm;
            SupportedAlgorithms = new List<string> { "naive_bayes", "svm" };
        }
    }
}
