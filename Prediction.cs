using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace БД_НТИ
{
    public class Prediction
    {
        public Prediction(string modelpath)
        {
            MLNetModelPath = modelpath;
        }

        #region model input class
        public class ModelInput
        {
            [ColumnName(@"ObjID")]
            public float ObjID { get; set; }


            [ColumnName(@"RelLength")]
            public float RelLength { get; set; }


            [ColumnName(@"Re")]
            public float Re { get; set; }


            [ColumnName(@"CellHeight")]
            public float CellHeight { get; set; }


            [ColumnName(@"y")]
            public float Y { get; set; }


            [ColumnName(@"LHRatio")]
            public float LHRatio { get; set; }


            [ColumnName(@"LayerNumber")]
            public float LayerNumber { get; set; }


            [ColumnName(@"BLHeight")]
            public float BLHeight { get; set; }


            [ColumnName(@"ElNumber")]
            public float ElNumber { get; set; }


            [ColumnName(@"GCSize")]
            public float GCSize { get; set; }


            [ColumnName(@"TurbModel")]
            public string TurbModel { get; set; }


            [ColumnName(@"LossFactorCFX")]
            public float LossFactorCFX { get; set; }

        }
        #endregion

        #region model output class
        public class ModelOutput
        {
            public float Score { get; set; }
        }
        #endregion

        private static string MLNetModelPath; //= Path.GetFullPath("LoseCoefModel.zip");

        public static readonly Lazy<PredictionEngine<ModelInput, ModelOutput>> PredictEngine = new Lazy<PredictionEngine<ModelInput, ModelOutput>>(() => CreatePredictEngine(), true);

        public ModelOutput Predict(ModelInput input)
        {
            var predEngine = PredictEngine.Value;
            return predEngine.Predict(input);
        }

        private static PredictionEngine<ModelInput, ModelOutput> CreatePredictEngine()
        {
            var mlContext = new MLContext();
            ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out var _);
            return mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
        }
    }
}
