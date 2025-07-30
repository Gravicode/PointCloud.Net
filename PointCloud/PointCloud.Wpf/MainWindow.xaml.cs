using HelixToolkit.Wpf;
using SharpDX.Direct3D9;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PointCloud.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        YoloDetector detector { set; get; }
        public MainWindow()
        {
            InitializeComponent();
            var modelPath = @"C:\experiment\pointcloud\yolov8s.onnx";
            detector = new(modelPath);
            //viewPort3d.CameraMode = HelixToolkit.Wpf.CameraMode.WalkAround;
            //viewPort3d.CameraRotationMode = HelixToolkit.Wpf.CameraRotationMode.Turntable;
            //viewPort3d.ModelUpDirection = new Vector3D(0,0,1);
            //viewPort3d.ZoomExtentsWhenLoaded = true;
            //viewPort3d.IsRotationEnabled = true;
            //viewPort3d.IsPanEnabled = true;
            //viewPort3d.IsZoomEnabled = true;
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            var pos = camera.Position;
            var moveSpeed = 0.5;

            switch (e.Key)
            {
                case Key.W: camera.Position += new Vector3D(0, 0, -moveSpeed); break;
                case Key.S: camera.Position += new Vector3D(0, 0, moveSpeed); break;
                case Key.A: camera.Position += new Vector3D(-moveSpeed, 0, 0); break;
                case Key.D: camera.Position += new Vector3D(moveSpeed, 0, 0); break;
                case Key.Q: camera.Position += new Vector3D(0, moveSpeed, 0); break;
                case Key.E: camera.Position += new Vector3D(0, -moveSpeed, 0); break;
            }

            base.OnKeyDown(e);
        }

        ModelVisual3D LoadedModel { set; get; }
        private void LoadModel_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "OBJ files (*.obj)|*.obj";

            if (dialog.ShowDialog() == true)
            {
                //var importer = new HelixToolkit.Wpf.ModelImporter();              
                //var model = importer.Load(dialog.FileName);
                // Create the reader
                var objReader = new ObjReader();
                FileInfo info = new FileInfo(dialog.FileName);
                // Load the model (ensure full path is used)
                //objReader.TexturePath = System.IO.Path.Combine(info.Directory.FullName, "textures");
                Model3DGroup model = objReader.Read(dialog.FileName);
               
                var visual = new ModelVisual3D { Content = model };
                
                if (LoadedModel != null)
                    viewPort3d.Children.Remove(LoadedModel);

                LoadedModel = visual;
               
                viewPort3d.Children.Add(visual);
            }
        }

        private void Capture_Click(object sender, RoutedEventArgs e)
        {
            var rtb = new RenderTargetBitmap((int)viewPort3d.ActualWidth, (int)viewPort3d.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(viewPort3d);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            using (var stream = File.Create("capture.png"))
            {
                encoder.Save(stream);
            }
        }

        private async void Detect_Click(object sender, RoutedEventArgs e)
        {
            var rtb = new RenderTargetBitmap((int)viewPort3d.ActualWidth, (int)viewPort3d.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(viewPort3d);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            byte[] Data;
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                Data = stream.ToArray();
            }
            if (Data != null)
            {
                //yolo process
                var res = await detector.DetectObjects(Data);
                if(res.objects != null)
                {
                    Debug.WriteLine("Detected Objects:");
                    foreach (var item in res.objects)
                    {
                        Debug.WriteLine($"{item.Label} - {item.Confidence}");    
                    }
                    File.WriteAllBytes("detect.png", res.image);
                }
            }
        }


    }
}