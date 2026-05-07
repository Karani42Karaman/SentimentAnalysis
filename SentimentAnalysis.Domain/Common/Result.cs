namespace SentimentAnalysis.Domain.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; }
        public T? Data { get; private set; }
        public List<string> Errors { get; private set; }

        // Constructor 4 parametre alıyor
        private Result(bool isSuccess, string message, T? data, List<string> errors)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
            Errors = errors ?? new List<string>();
        }

        // Başarılı işlem
        public static Result<T> Success(T data, string message = "İşlem başarılı.")
        {
            return new Result<T>(true, message, data, null!);
        }

        // Hatalı işlem (Hem ana mesaj hem de hata listesi alabilir)
        public static Result<T> Failure(string message, List<string>? errors = null)
        {
            return new Result<T>(false, message, default, errors!);
        }
    }
}