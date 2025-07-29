using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoloDotNet;
using YoloDotNet.Enums;
using YoloDotNet.Extensions;
using YoloDotNet.Models;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
namespace PointCloud.Wpf
{
    internal class YoloDetector : IDisposable
    {
        public Yolo yolo { get; set; }
        public YoloDetector(string ModelPathOnnx, bool CudaEnabled=false)
        {
            // Fire it up! Create an instance of YoloDotNet and reuse it across your app's lifetime.
            // Prefer the 'using' pattern for automatic cleanup if you're done after a single run.
            yolo = new Yolo(new YoloOptions
            {
                OnnxModel = ModelPathOnnx,
                Cuda = CudaEnabled,
                PrimeGpu = CudaEnabled,
                GpuId = 0,
                ImageResize = ImageResize.Proportional
                // Choose between Proportional or Stretched resizing.
                // Use 'Proportional' if your model was trained with images that preserve aspect ratio (e.g., padded borders).
                // Use 'Stretched' if your training data was resized to fit the model's input dimensions directly.
                // This setting influence detection accuracy, so be sure it aligns with how the model was trained.
            });
        }

        public void Dispose()
        {
            // Clean up – unless you're using 'using' above.
            yolo?.Dispose();
        }

        public Task<(byte[] image,List<ObjectDetection> objects)> DetectObjects(byte[] imageFrame)
        {
            try
            {
                // Which YOLO magic is this? Let’s find out!
                Console.WriteLine($"Model Type: {yolo.ModelInfo}");

                // Load image with SkiaSharp
                using var image = SKBitmap.Decode(imageFrame);

                // Run object detection with default values
                var results = yolo.RunObjectDetection(image, confidence: 0.20,iou: 0.7d);

                image.Draw(results);        // Overlay results on image

                using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
                byte[] encodedBytes = data.ToArray(); // Compressed PNG data

                // Save to file            
                //image.Save("result.jpg");   // Save to disk – boom, done!
                return Task.FromResult((encodedBytes, results));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return default;
            
        }
    }
}
