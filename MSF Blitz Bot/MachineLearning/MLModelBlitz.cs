using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.LightGbm;

namespace MSFBlitzBot.MachineLearning
{
    public partial class MLModelBlitz
    {
        /// <summary>
        /// model input class for MLModelBlitz.
        /// </summary>
        #region model input class
        public class ModelInput
        {
            [ColumnName(@"PlayerHeroId1")]
            [LoadColumn(0)]
            public string PlayerHeroId1 { get; set; }

            [ColumnName(@"PlayerHeroPower1")]
            [LoadColumn(1)]
            public float PlayerHeroPower1 { get; set; }

            [ColumnName(@"PlayerHeroId2")]
            [LoadColumn(2)]
            public string PlayerHeroId2 { get; set; }

            [ColumnName(@"PlayerHeroPower2")]
            [LoadColumn(3)]
            public float PlayerHeroPower2 { get; set; }

            [ColumnName(@"PlayerHeroId3")]
            [LoadColumn(4)]
            public string PlayerHeroId3 { get; set; }

            [ColumnName(@"PlayerHeroPower3")]
            [LoadColumn(5)]
            public float PlayerHeroPower3 { get; set; }

            [ColumnName(@"PlayerHeroId4")]
            [LoadColumn(6)]
            public string PlayerHeroId4 { get; set; }

            [ColumnName(@"PlayerHeroPower4")]
            [LoadColumn(7)]
            public float PlayerHeroPower4 { get; set; }

            [ColumnName(@"PlayerHeroId5")]
            [LoadColumn(8)]
            public string PlayerHeroId5 { get; set; }

            [ColumnName(@"PlayerHeroPower5")]
            [LoadColumn(9)]
            public float PlayerHeroPower5 { get; set; }

            [ColumnName(@"OpponentHeroId1")]
            [LoadColumn(10)]
            public string OpponentHeroId1 { get; set; }

            [ColumnName(@"OpponentHeroPower1")]
            [LoadColumn(11)]
            public float OpponentHeroPower1 { get; set; }

            [ColumnName(@"OpponentHeroId2")]
            [LoadColumn(12)]
            public string OpponentHeroId2 { get; set; }

            [ColumnName(@"OpponentHeroPower2")]
            [LoadColumn(13)]
            public float OpponentHeroPower2 { get; set; }

            [ColumnName(@"OpponentHeroId3")]
            [LoadColumn(14)]
            public string OpponentHeroId3 { get; set; }

            [ColumnName(@"OpponentHeroPower3")]
            [LoadColumn(15)]
            public float OpponentHeroPower3 { get; set; }

            [ColumnName(@"OpponentHeroId4")]
            [LoadColumn(16)]
            public string OpponentHeroId4 { get; set; }

            [ColumnName(@"OpponentHeroPower4")]
            [LoadColumn(17)]
            public float OpponentHeroPower4 { get; set; }

            [ColumnName(@"OpponentHeroId5")]
            [LoadColumn(18)]
            public string OpponentHeroId5 { get; set; }

            [ColumnName(@"OpponentHeroPower5")]
            [LoadColumn(19)]
            public float OpponentHeroPower5 { get; set; }

            [ColumnName(@"IsVictory")]
            [LoadColumn(20)]
            public float IsVictory { get; set; }

        }

        #endregion

        /// <summary>
        /// model output class for MLModelBlitz.
        /// </summary>
        #region model output class
        public class ModelOutput
        {
            public float Score { get; set; }
        }
        #endregion

        private static readonly string MLDataPath = Path.GetFullPath("MachineLearning/data.csv");
        private static readonly string MLNetModelPath = Path.GetFullPath("MachineLearning/MLModelBlitz.zip");
        private static PredictionEngine<ModelInput, ModelOutput> PredictionEngine;

        private static void LoadEngine()
        {
            MLContext mlContext = new();

            // Load model & create prediction engine
            ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out _);
            PredictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
        }

        public static void Retrain(IEnumerable<ModelInput> inputDataToTrain)
        {
            MLContext mlContext = new();

            // On mixe le data du model existant avec le nouveau data à entrainer en un seul IDataView.
            var oldData = mlContext.Data.CreateEnumerable<ModelInput>(mlContext.Data.LoadFromTextFile<ModelInput>(MLDataPath, ',', true), false);
            var allData = oldData.Concat(inputDataToTrain);
            var newData = mlContext.Data.LoadFromEnumerable(allData);

            var pipeline = BuildPipeline(mlContext);
            var retrainModel = pipeline.Fit(newData);
            PredictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(retrainModel);
        }

        /// <summary>
        /// build the pipeline that is used from model builder. Use this function to retrain model.
        /// </summary>
        /// <param name="mlContext"></param>
        /// <returns></returns>
        private static IEstimator<ITransformer> BuildPipeline(MLContext mlContext)
        {
            // Data process configuration with pipeline data transformations
            var pipeline = mlContext.Transforms.ReplaceMissingValues(new[] { new InputOutputColumnPair(@"PlayerHeroPower1", @"PlayerHeroPower1"), new InputOutputColumnPair(@"PlayerHeroPower2", @"PlayerHeroPower2"), new InputOutputColumnPair(@"PlayerHeroPower3", @"PlayerHeroPower3"), new InputOutputColumnPair(@"PlayerHeroPower4", @"PlayerHeroPower4"), new InputOutputColumnPair(@"PlayerHeroPower5", @"PlayerHeroPower5"), new InputOutputColumnPair(@"OpponentHeroPower1", @"OpponentHeroPower1"), new InputOutputColumnPair(@"OpponentHeroPower2", @"OpponentHeroPower2"), new InputOutputColumnPair(@"OpponentHeroPower3", @"OpponentHeroPower3"), new InputOutputColumnPair(@"OpponentHeroPower4", @"OpponentHeroPower4"), new InputOutputColumnPair(@"OpponentHeroPower5", @"OpponentHeroPower5") })
                .Append(mlContext.Transforms.Text.FeaturizeText(@"PlayerHeroId1", @"PlayerHeroId1"))
                .Append(mlContext.Transforms.Text.FeaturizeText(@"PlayerHeroId2", @"PlayerHeroId2"))
                .Append(mlContext.Transforms.Text.FeaturizeText(@"PlayerHeroId3", @"PlayerHeroId3"))
                .Append(mlContext.Transforms.Text.FeaturizeText(@"PlayerHeroId4", @"PlayerHeroId4"))
                .Append(mlContext.Transforms.Text.FeaturizeText(@"PlayerHeroId5", @"PlayerHeroId5"))
                .Append(mlContext.Transforms.Text.FeaturizeText(@"OpponentHeroId1", @"OpponentHeroId1"))
                .Append(mlContext.Transforms.Text.FeaturizeText(@"OpponentHeroId2", @"OpponentHeroId2"))
                .Append(mlContext.Transforms.Text.FeaturizeText(@"OpponentHeroId3", @"OpponentHeroId3"))
                .Append(mlContext.Transforms.Text.FeaturizeText(@"OpponentHeroId4", @"OpponentHeroId4"))
                .Append(mlContext.Transforms.Text.FeaturizeText(@"OpponentHeroId5", @"OpponentHeroId5"))
                .Append(mlContext.Transforms.Concatenate(@"Features", new[] { @"PlayerHeroPower1", @"PlayerHeroPower2", @"PlayerHeroPower3", @"PlayerHeroPower4", @"PlayerHeroPower5", @"OpponentHeroPower1", @"OpponentHeroPower2", @"OpponentHeroPower3", @"OpponentHeroPower4", @"OpponentHeroPower5", @"PlayerHeroId1", @"PlayerHeroId2", @"PlayerHeroId3", @"PlayerHeroId4", @"PlayerHeroId5", @"OpponentHeroId1", @"OpponentHeroId2", @"OpponentHeroId3", @"OpponentHeroId4", @"OpponentHeroId5" }))
                .Append(mlContext.Regression.Trainers.LightGbm(new LightGbmRegressionTrainer.Options()
                {
                    NumberOfLeaves = 11,
                    MinimumExampleCountPerLeaf = 39,
                    NumberOfIterations = 1096,
                    MaximumBinCountPerFeature = 402,
                    LearningRate = 1F,
                    LabelColumnName = @"IsVictory",
                    FeatureColumnName = @"Features",
                    Booster = new GradientBooster.Options() { SubsampleFraction = 1F, FeatureFraction = 0.859155036402985F, L1Regularization = 7.98240180413192E-08F, L2Regularization = 26931.7644320021F }
                }));

            return pipeline;
        }

        /// <summary>
        /// Use this method to predict on <see cref="ModelInput"/>.
        /// </summary>
        /// <param name="input">model input.</param>
        /// <returns><seealso cref=" ModelOutput"/></returns>
        public static ModelOutput Predict(ModelInput input)
        {
            if (PredictionEngine == null) LoadEngine();

            ModelOutput result = PredictionEngine.Predict(input);
            return result;
        }
    }
}
