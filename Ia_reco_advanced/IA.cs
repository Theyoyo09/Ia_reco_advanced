using Ia_reco;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;
using IA_reco;
using System.Threading;
using Ia_reco.DataStructures;
using System.Windows.Forms;
using Ia_reco_advanced;
using Microsoft.VisualBasic.ApplicationServices;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;

namespace IA_reco
{
    public class IA
    {
        const string modelPath = @"C:\Users\ydemo\source\repos\Ia_reco_advanced\Ia_reco_advanced\Assets\yolov4.onnx";
        string imgfacile;
        Bitmap bitmapmain;
        Stopwatch sw;
        PredictionEngine<YoloV4BitmapData, YoloV4Prediction> predictionEngine;
        int i = 0;
        List<YoloV4Result> old_result = new();
        MLContext mlContext;
        Microsoft.ML.Data.TransformerChain<Microsoft.ML.Transforms.Onnx.OnnxTransformer> model;
        Microsoft.ML.Data.EstimatorChain<Microsoft.ML.Transforms.Onnx.OnnxTransformer> pipeline;
    
    public IA()
        {
            //cree le context
            mlContext = new MLContext();

            //wiki yolov4 pas grand chose a faire

            // Define scoring pipeline
            pipeline = mlContext.Transforms.ResizeImages(inputColumnName: "bitmap", outputColumnName: "input_1:0", imageWidth: 416, imageHeight: 416, resizing: ResizingKind.IsoPad)
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input_1:0", scaleImage: 1f / 255f, interleavePixelColors: true))
                .Append(mlContext.Transforms.ApplyOnnxModel(
                    shapeDictionary: new Dictionary<string, int[]>()
                    {
                        { "input_1:0", new[] { 1, 416, 416, 3 } },
                        { "Identity:0", new[] { 1, 52, 52, 3, 85 } },
                        { "Identity_1:0", new[] { 1, 26, 26, 3, 85 } },
                        { "Identity_2:0", new[] { 1, 13, 13, 3, 85 } },
                    },
                    inputColumnNames: new[]
                    {
                        "input_1:0"
                    },
                    outputColumnNames: new[]
                    {
                        "Identity:0",
                        "Identity_1:0",
                        "Identity_2:0"
                    },
                    modelFile: modelPath, recursionLimit: 100));

            // Fit on empty list to obtain input data schema
            
            model = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<YoloV4BitmapData>()));

            // Create prediction engine
          //  predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model);

            sw = new Stopwatch();
            sw.Start();

        }
       
        //list fourni par le model
        static readonly string[] classesNames = new string[] { "Vilainpasbeau", "yélo", "Voiture", "Moto", "aeroplane", "bus", "train", "truck", "boat", "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "Pomme", "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "Chaise", "sofa", "pottedplant", "Lit", "Table", "toilet", "TV", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush" };
        public Bitmap go(string imgfacile)
        {
           
            this.imgfacile = imgfacile;
            return computeAsync(1,bitmapmain).Result;
        }

        public Bitmap record(Bitmap bitmap)
        {;
            this.bitmapmain = new Bitmap(bitmap);

            if (i <= 6)
            {
              
                i++;
            }
            else if (i == 7)
            {
                prediction(new Bitmap(bitmap));
                i = 0;
            }
            return computeAsync(2,new Bitmap(bitmap)).Result;
            
        }

        public async Task<Bitmap> computeAsync(int mode,Bitmap mybit)
        {

            if (mode == 1)
            {
               return photo_compute(predictionEngine,sw);
            }
            else if (mode == 2)
            {
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.

               

                return new Bitmap( await Task.Run(() => video_compute(mybit).Result));
            }
            else
            {
                throw new Exception("erreur choix mode ia");
            }
            
        }

        public Bitmap photo_compute(PredictionEngine<YoloV4BitmapData, YoloV4Prediction> predictionEngine,Stopwatch sw)
        {
            using (var bitmap = new Bitmap(System.Drawing.Image.FromFile(imgfacile)))
            {
                // predict
                var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });
                var results = predict.GetResults(classesNames, 0.3f, 0.7f);

                using (var g = Graphics.FromImage(bitmap))
                {
                    foreach (var res in results)
                    {
                        // fait dessiner les boiboites
                        var x1 = res.BBox[0];
                        var y1 = res.BBox[1];
                        var x2 = res.BBox[2];
                        var y2 = res.BBox[3];
                        g.DrawRectangle(Pens.Red, x1, y1, x2 - x1, y2 - y1);
                        using (var brushes = new SolidBrush(Color.FromArgb(50, Color.Red)))
                        {
                            g.FillRectangle(brushes, x1, y1, x2 - x1, y2 - y1);
                        }

                        g.DrawString(res.Label + " " + res.Confidence.ToString("0.00"),
                                     new Font("Arial", 12), Brushes.Blue, new PointF(x1, y1));
                    }
                    // string chemin_analyser = Path.Combine(imgfacile, Path.ChangeExtension(imgfacile, "_analyser" + Path.GetExtension(imgfacile)));
                    // bitmap.Save(chemin_analyser);
                    return new Bitmap(bitmap);
                }
            }

            sw.Stop();
            Console.WriteLine($"Fait en {sw.ElapsedMilliseconds}ms.");
        }

        public async Task<Bitmap> video_compute(Bitmap mybit)
        {
            using (var bitmap = new Bitmap(mybit))
            {
                // predict


                // prediction(bitmap);

                using (var g = Graphics.FromImage(bitmap))
                {
                    foreach (var res in old_result)
                    {
                        // fait dessiner les boiboites
                        var x1 = res.BBox[0];
                        var y1 = res.BBox[1];
                        var x2 = res.BBox[2];
                        var y2 = res.BBox[3];
                        g.DrawRectangle(Pens.Red, x1, y1, x2 - x1, y2 - y1);
                        using (var brushes = new SolidBrush(Color.FromArgb(50, Color.Red)))
                        {
                            g.FillRectangle(brushes, x1, y1, x2 - x1, y2 - y1);
                        }

                        g.DrawString(res.Label + " " + res.Confidence.ToString("0.00"),
                                    new Font("Arial", 12), Brushes.Blue, new PointF(x1, y1));

                    }
                }


                sw.Stop();

                return new Bitmap(bitmap);
            }
        }

        public async void prediction(Bitmap bitmap)
        {
            Bitmap mybitmap = bitmap;
            PredictionEngine<YoloV4BitmapData, YoloV4Prediction> custompredictionEngine = await Task.Run(() => mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model));
            YoloV4Prediction predict = await Task.Run(() => custompredictionEngine.Predict(new YoloV4BitmapData() { Image = mybitmap }));
            IReadOnlyList<YoloV4Result> results = await Task.Run(() =>predict.GetResults(classesNames, 0.3f, 0.7f));
            old_result =await Task.Run(() => results.ToList());
        }
    }
}
