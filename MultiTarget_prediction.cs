using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace БД_НТИ
{
    internal class MultiTarget_prediction
    {
        //Относительная длина RelLength +
        //Re  Re +
        //Высота ячейки первого призматического слоя  CellHeight +
        //y+	y
        //Отношение высот слоёв   LHRatio +
        //Число слоёв LayerNumber
        //Общая высота пограничного слоя BLHeight
        //Количество элементов    ElNumber +
        //Величина глобальной ячейки GCSize
        //Модель турбулентности   TurbModel

        //Предсказать:
        //y
        //GCSize
        //BLHeight
        //LayerNumber
        //TurbModel

        public MultiTarget_prediction(string y_model, string GCSize_model, string BLHeight_model, string LayerNumber_model, string TurbModel_model, string testFile_path = null)
        {
            models = new Dictionary<string, string>()
            {
                ["y"] = y_model,
                ["GCSize"] = GCSize_model,
                ["BLHeight"] = BLHeight_model,
                ["LayerNumber"] = LayerNumber_model,
                ["TurbModel"] = TurbModel_model

            };
            testfile = testFile_path;

        }

        public Dictionary<string, string> models;
        // public Dictionary<string, double> models_metrics; //точности моделей
        private string testfile;
        private MLContext context = new MLContext();

        public class ModelInput
        {

            [ColumnName(@"RelLength")]
            public float RelLength { get; set; }

            [ColumnName(@"Re")]
            public float Re { get; set; }

            [ColumnName(@"CellHeight")]
            public float CellHeight { get; set; }

            [ColumnName(@"LHRatio")]
            public float LHRatio { get; set; }

            [ColumnName(@"ElNumber")]
            public float ElNumber { get; set; }

            [ColumnName(@"LossFactorCFX")]
            public float LossFactorCFX { get; set; }

            [ColumnName(@"LayerNumber")]
            public float LayerNumber { get; set; }

            [ColumnName(@"TurbModel")]
            public string TurbModel { get; set; }

        }

        public class Results
        {
            public float y { get; set; }
            public float GCSize { get; set; }
            public float BLHeight { get; set; }
            public float LayerNumber { get; set; }
            public string TurbModel { get; set; }
        }

        private class ModelOutput
        {
            [ColumnName("Score")]
            public float Score { get; set; }

        }

        public class LayerModelOutput
        {
            [ColumnName("PredictedLabel")]
            public float Prediction { get; set; }

            public float[] Score { get; set; }
        }

        public class TurbModelOutput
        {
            [ColumnName("PredictedLabel")]
            public string Prediction { get; set; }

            public float[] Score { get; set; }
        }

        public Results Predict(ModelInput input)
        {
            //Dictionary<string, double>  models_metrics = new Dictionary<string, double>();
            Results results = new Results();

            //IDataView data = context.Data.LoadFromTextFile<ModelInput>(testfile, hasHeader: true, separatorChar: ';');
            // var trainTestData = context.Data.TrainTestSplit(data, testFraction: 0.1, seed: 4);
            // var trainData = trainTestData.TrainSet;
            // var testData = trainTestData.TestSet;

            foreach (var model_path in models)
            {
                string MLNetModelPath = Path.GetFullPath(model_path.Value);
                // IDataView testData=null; //=testfile

                if (model_path.Key == "LayerNumber")
                {
                    var mlContext = new MLContext();
                    ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out var _);
                    Lazy<PredictionEngine<ModelInput, LayerModelOutput>> PredictEngine = new Lazy<PredictionEngine<ModelInput, LayerModelOutput>>(() => mlContext.Model.CreatePredictionEngine<ModelInput, LayerModelOutput>(mlModel), true);
                    var predEngine = PredictEngine.Value;
                    var res = predEngine.Predict(input);
                    results.LayerNumber = Convert.ToSingle(res.Prediction);
                }
                else if (model_path.Key == "TurbModel")
                {
                    var mlContext = new MLContext();
                    ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out var _);
                    Lazy<PredictionEngine<ModelInput, TurbModelOutput>> PredictEngine = new Lazy<PredictionEngine<ModelInput, TurbModelOutput>>(() => mlContext.Model.CreatePredictionEngine<ModelInput, TurbModelOutput>(mlModel), true);
                    var predEngine = PredictEngine.Value;
                    var res = predEngine.Predict(input);
                    results.TurbModel = res.Prediction;
                }
                else
                {
                    var mlContext = new MLContext();
                    ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out var _);
                    Lazy<PredictionEngine<ModelInput, ModelOutput>> PredictEngine = new Lazy<PredictionEngine<ModelInput, ModelOutput>>(() => mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel), true);
                    var predEngine = PredictEngine.Value;

                    var res = predEngine.Predict(input);
                    switch (model_path.Key)
                    {
                        case "y":
                            results.y = Convert.ToSingle(res.Score);
                            break;
                        case "GCSize":
                            results.GCSize = Convert.ToSingle(res.Score);
                            break;
                        case "BLHeight":
                            results.BLHeight = Convert.ToSingle(res.Score);
                            break;

                    }
                }
            }
            return results;
        }
    }
}
