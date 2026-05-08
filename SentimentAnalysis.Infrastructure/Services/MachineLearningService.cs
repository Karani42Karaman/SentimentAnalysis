using Microsoft.ML;
using Microsoft.ML.Data;
using SentimentAnalysis.Domain.Entities;
using SentimentAnalysis.Domain.Interfaces;

namespace SentimentAnalysis.Infrastructure.Services;

public class MachineLearningService : IMachineLearningService
{
    private readonly MLContext _mlContext;
    private readonly string _modelPath;

    public MachineLearningService()
    {
        // Tüm ML işlemlerinin ana nesnesi
        _mlContext = new MLContext(seed: 0);

        // Eğitilen modelin diske kaydedileceği yer (Uygulamanın çalıştığı dizin)
        _modelPath = Path.Combine(Environment.CurrentDirectory, "sentiment_model.zip");
    }

    public async Task TrainModelAsync(string dataPath, string algorithm, int nGramRange, int minFreq)
    {
        // 1. Veriyi Yükle (CSV dosyasından)
        // Hocanın istediği: "metin ve etiket sütunları" olacak.
        IDataView dataView = _mlContext.Data.LoadFromTextFile<SentimentData>(
            path: dataPath,
            hasHeader: true,
            separatorChar: ',');

        // 2. Veri Ön İşleme (Pipeline)
        var dataProcessPipeline = _mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "LabelKey", inputColumnName: nameof(SentimentData.Label))
            .Append(_mlContext.Transforms.Text.FeaturizeText(
                outputColumnName: "Features",
                options: new Microsoft.ML.Transforms.Text.TextFeaturizingEstimator.Options
                {
                     
                    WordFeatureExtractor = new Microsoft.ML.Transforms.Text.WordBagEstimator.Options()
                    {
                        NgramLength = nGramRange,
                        Weighting = Microsoft.ML.Transforms.Text.NgramExtractingEstimator.WeightingCriteria.TfIdf
                    }
                },
                nameof(SentimentData.Text) 
            ));

       
        IEstimator<ITransformer> trainer;
        if (algorithm.ToLower() == "naive_bayes")
        {
            // Naive Bayes (Çok sınıflı sınıflandırma için)
            trainer = _mlContext.MulticlassClassification.Trainers.NaiveBayes(labelColumnName: "LabelKey", featureColumnName: "Features");
        }
        else // SVM alternatifi (SdcaMaximumEntropy doğrusal sınıflandırma yapar ve olasılık döner)
        {
            trainer = _mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName: "LabelKey", featureColumnName: "Features");
        }

        // 4. Eğitim Pipeline'ını Birleştir
        var trainingPipeline = dataProcessPipeline
            .Append(trainer)
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        // 5. Modeli Eğit (Ağır işlem burada gerçekleşir)
        await Task.Run(() =>
        {
            var trainedModel = trainingPipeline.Fit(dataView);

            // 6. Eğitilen Modeli Diske Kaydet (.zip olarak)
            _mlContext.Model.Save(trainedModel, dataView.Schema, _modelPath);
        });
    }

    public SentimentResult AnalyzeText(string text)
    {
        if (!File.Exists(_modelPath))
            throw new Exception("Model henüz eğitilmemiş. Lütfen önce /model/train endpoint'ini çağırın.");

        // 1. Modeli ve Tahmin Motorunu Yükle
        DataViewSchema modelSchema;
        ITransformer trainedModel = _mlContext.Model.Load(_modelPath, out modelSchema);
        var predEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(trainedModel);

        var input = new SentimentData { Text = text };
        var prediction = predEngine.Predict(input);

         
        VBuffer<ReadOnlyMemory<char>> slotNames = default;
        predEngine.OutputSchema["Score"].Annotations.GetValue("SlotNames", ref slotNames);

        var labelNames = new string[slotNames.Length];
        var slotValues = slotNames.GetValues();
        for (int i = 0; i < slotValues.Length; i++)
        {
            labelNames[i] = slotValues[i].ToString();
        }

        // 4. Olasılıkları Sözlüğe Çevir (Kurşungeçirmez Döngü)
        var probabilityDict = new Dictionary<string, float>();
        for (int i = 0; i < prediction.Score.Length; i++)
        {
            // Eğer etiket ismini bulamazsa patlamaması için güvenlik önlemi
            string currentLabel = (i < labelNames.Length && !string.IsNullOrEmpty(labelNames[i]))
                                  ? labelNames[i]
                                  : $"Kategori_{i}";

            // Dict'e eklerken aynı key varsa patlamasın diye indexleyici kullanıyoruz
            probabilityDict[currentLabel] = prediction.Score[i];
        }

        // 5. Kesin Sonucu (PredictedLabel) Bul
        float maxScore = prediction.Score.Max();
        int maxIndex = Array.IndexOf(prediction.Score, maxScore);
        string realLabel = (maxIndex < labelNames.Length && !string.IsNullOrEmpty(labelNames[maxIndex]))
                           ? labelNames[maxIndex]
                           : $"Kategori_{maxIndex}";

        return new SentimentResult
        {
            PredicatedLabel = realLabel,
            Probabilities = probabilityDict
        };
    }

    // ML.NET'in tahmin sonucunu alması için gizli bir sınıf (Sadece bu serviste kullanılır)
    private class SentimentPrediction : SentimentData
    {
        [ColumnName("PredictedLabel")]
        public string Prediction { get; set; }

        [ColumnName("Score")]
        public float[] Score { get; set; }
    }
}